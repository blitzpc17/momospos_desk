using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using Npgsql;
using momospos.Models;

namespace momospos.Repositories
{
    public class VentaRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public void RegistrarVenta(Venta venta)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // Insertar Venta y obtener el ID
                        string sqlVenta = @"INSERT INTO Ventas (Folio, CajaSesionId, ClienteId, Fecha, Total, Pagado, Cambio, DescuentoTotal, Estado, UsuarioId, MedicoNombre, MedicoCedula, RecetaRetenida, RecetaRutaImagen) 
                                            VALUES (@Folio, @CajaSesionId, @ClienteId, @Fecha, @Total, @Pagado, @Cambio, @DescuentoTotal, @Estado, @UsuarioId, @MedicoNombre, @MedicoCedula, @RecetaRetenida, @RecetaRutaImagen) RETURNING Id;";
                        int ventaId = db.QuerySingle<int>(sqlVenta, venta, transaction);

                        // Insertar Detalles
                        foreach (var detalle in venta.Detalles)
                        {
                            detalle.VentaId = ventaId;
                            string sqlDetalle = @"INSERT INTO VentaDetalles (VentaId, ProductoId, Descripcion, Cantidad, PrecioUnitario, Subtotal, DescuentoManual) 
                                                  VALUES (@VentaId, @ProductoId, @Descripcion, @Cantidad, @PrecioUnitario, @Subtotal, @DescuentoManual) RETURNING Id;";
                            int detalleId = db.QuerySingle<int>(sqlDetalle, detalle, transaction);
                            detalle.Id = detalleId;

                            // Actualizar Stock (Solo si no es servicio)
                            bool aplicaCaducidad = db.QuerySingleOrDefault<bool>("SELECT AplicaCaducidad FROM Productos WHERE Id = @ProductoId", new { ProductoId = detalle.ProductoId }, transaction);
                            
                            if (aplicaCaducidad)
                            {
                                var lotes = db.Query<ProductoLote>("SELECT * FROM ProductoLotes WHERE ProductoId = @ProductoId AND StockActual > 0 ORDER BY FechaCaducidad ASC", new { ProductoId = detalle.ProductoId }, transaction).ToList();
                                decimal cantidadPendiente = detalle.Cantidad;

                                foreach(var lote in lotes)
                                {
                                    if(cantidadPendiente <= 0) break;

                                    decimal cantidadADescontar = Math.Min(cantidadPendiente, lote.StockActual);
                                    
                                    db.Execute("UPDATE ProductoLotes SET StockActual = StockActual - @Cantidad WHERE Id = @Id", new { Cantidad = cantidadADescontar, Id = lote.Id }, transaction);
                                    db.Execute("INSERT INTO VentaDetalleLotes (VentaDetalleId, ProductoLoteId, Cantidad) VALUES (@VentaDetalleId, @ProductoLoteId, @Cantidad)", new { VentaDetalleId = detalleId, ProductoLoteId = lote.Id, Cantidad = cantidadADescontar }, transaction);

                                    cantidadPendiente -= cantidadADescontar;
                                }
                            }
                            
                            // Siempre actualizamos el global
                            string sqlStock = "UPDATE Productos SET StockActual = StockActual - @Cantidad WHERE Id = @ProductoId AND EsServicio = FALSE;";
                            db.Execute(sqlStock, new { Cantidad = detalle.Cantidad, ProductoId = detalle.ProductoId }, transaction);
                        }

                        // Insertar Pagos
                        foreach (var pago in venta.Pagos)
                        {
                            pago.VentaId = ventaId;
                            string sqlPago = @"INSERT INTO VentaPagos (VentaId, MetodoPago, Importe, Fecha) 
                                               VALUES (@VentaId, @MetodoPago, @Importe, @Fecha);";
                            db.Execute(sqlPago, pago, transaction);

                            // Actualizar saldo si es crédito
                            if (pago.MetodoPago == "CREDITO" && venta.ClienteId.HasValue)
                            {
                                string sqlCredito = "UPDATE Clientes SET Saldo = Saldo + @Importe WHERE Id = @ClienteId;";
                                db.Execute(sqlCredito, new { Importe = pago.Importe, ClienteId = venta.ClienteId.Value }, transaction);
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public (decimal TotalEfectivo, decimal TotalTarjeta, decimal TotalVendido, List<Venta> Historial) ObtenerReporteVentas(DateTime inicio, DateTime fin)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Ajustar fechas para que abarque todo el día final y comience desde la medianoche del inicio
                inicio = inicio.Date;
                fin = fin.Date.AddDays(1).AddTicks(-1);
                
                string sqlVentas = "SELECT * FROM Ventas WHERE Fecha BETWEEN @Inicio AND @Fin AND Estado = 'CONFIRMADO' ORDER BY Fecha DESC";
                var historial = db.Query<Venta>(sqlVentas, new { Inicio = inicio, Fin = fin }).ToList();

                string sqlPagos = @"SELECT vp.MetodoPago, SUM(vp.Importe) as Total 
                                    FROM VentaPagos vp 
                                    INNER JOIN Ventas v ON vp.VentaId = v.Id 
                                    WHERE v.Fecha BETWEEN @Inicio AND @Fin AND v.Estado = 'CONFIRMADO'
                                    GROUP BY vp.MetodoPago";
                
                var pagos = db.Query(sqlPagos, new { Inicio = inicio, Fin = fin }).ToList();

                decimal totalEfectivo = 0;
                decimal totalTarjeta = 0;

                foreach (var p in pagos)
                {
                    if (p.metodopago == "EFECTIVO") totalEfectivo = Convert.ToDecimal(p.total);
                    if (p.metodopago == "TARJETA") totalTarjeta = Convert.ToDecimal(p.total);
                }

                decimal totalVendido = historial.Sum(x => x.Total);

                return (totalEfectivo, totalTarjeta, totalVendido, historial);
            }
        }

        public Venta ObtenerVentaPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                var venta = db.QueryFirstOrDefault<Venta>("SELECT * FROM Ventas WHERE Id = @Id", new { Id = id });
                if (venta != null)
                {
                    venta.Detalles = db.Query<VentaDetalle>("SELECT * FROM VentaDetalles WHERE VentaId = @Id", new { Id = id }).ToList();
                    venta.Pagos = db.Query<VentaPago>("SELECT * FROM VentaPagos WHERE VentaId = @Id", new { Id = id }).ToList();
                }
                return venta;
            }
        }

        public List<MedicamentoControladoDTO> ObtenerReporteMedicamentosControlados(DateTime inicio, DateTime fin)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                inicio = inicio.Date;
                fin = fin.Date.AddDays(1).AddTicks(-1);

                string sql = @"
                    SELECT 
                        v.Folio as FolioVenta, 
                        v.Fecha as FechaVenta, 
                        v.MedicoNombre, 
                        v.MedicoCedula, 
                        c.Nombre as ClienteNombre,
                        p.CodigoBarras, 
                        p.Nombre as NombreProducto, 
                        p.SustanciaActiva,
                        vd.Cantidad,
                        (
                            SELECT STRING_AGG(pl.NumeroLote, ', ') 
                            FROM VentaDetalleLotes vdl 
                            INNER JOIN ProductoLotes pl ON vdl.ProductoLoteId = pl.Id 
                            WHERE vdl.VentaDetalleId = vd.Id
                        ) as NumerosLote
                    FROM VentaDetalles vd
                    INNER JOIN Ventas v ON vd.VentaId = v.Id
                    INNER JOIN Productos p ON vd.ProductoId = p.Id
                    LEFT JOIN Clientes c ON v.ClienteId = c.Id
                    WHERE v.Fecha BETWEEN @Inicio AND @Fin 
                      AND v.Estado = 'CONFIRMADO' 
                      AND p.RequiereReceta = TRUE
                    ORDER BY v.Fecha DESC;";
                
                return db.Query<MedicamentoControladoDTO>(sql, new { Inicio = inicio, Fin = fin }).ToList();
            }
        }

        public List<ArticuloVendidoDTO> ObtenerArticulosVendidosPorPeriodo(DateTime inicio, DateTime fin)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                inicio = inicio.Date;
                fin = fin.Date.AddDays(1).AddTicks(-1);
                
                string sql = @"
                    SELECT 
                        p.CodigoBarras,
                        p.Nombre,
                        p.SustanciaActiva,
                        c.Nombre AS Categoria,
                        MAX(p.PrecioCompra) AS PrecioCompraUnitario,
                        MAX(p.PrecioVenta) AS PrecioVentaUnitario,
                        SUM(vd.Cantidad) as CantidadTotal,
                        SUM(vd.Subtotal) as TotalGenerado,
                        SUM(vd.Subtotal) - (SUM(vd.Cantidad) * MAX(p.PrecioCompra)) as Ganancia
                    FROM VentaDetalles vd
                    INNER JOIN Ventas v ON vd.VentaId = v.Id
                    INNER JOIN Productos p ON vd.ProductoId = p.Id
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id
                    WHERE v.Fecha BETWEEN @Inicio AND @Fin AND v.Estado = 'CONFIRMADO'
                    GROUP BY p.CodigoBarras, p.Nombre, p.SustanciaActiva, c.Nombre
                    ORDER BY CantidadTotal DESC;";
                
                return db.Query<ArticuloVendidoDTO>(sql, new { Inicio = inicio, Fin = fin }).ToList();
            }
        }

        public void RegistrarVentaAbortada(DateTime fecha, int usuarioId, decimal totalEsperado, string motivo)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO VentasAbortadas (Fecha, UsuarioId, TotalEsperado, Motivo) 
                               VALUES (@Fecha, @UsuarioId, @TotalEsperado, @Motivo);";
                db.Execute(sql, new { Fecha = fecha, UsuarioId = usuarioId, TotalEsperado = totalEsperado, Motivo = motivo });
            }
        }

        public void SolicitarCancelacionVenta(int ventaId, int usuarioSolicitaId, string motivo)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        // Insertar solicitud
                        string sqlInsert = @"INSERT INTO VentaCancelaciones (VentaId, Motivo, UsuarioSolicitaId, Estado) 
                                             VALUES (@VentaId, @Motivo, @UsuarioSolicitaId, 'PENDIENTE');";
                        db.Execute(sqlInsert, new { VentaId = ventaId, Motivo = motivo, UsuarioSolicitaId = usuarioSolicitaId }, tx);

                        // Cambiar estado de venta
                        string sqlUpdate = "UPDATE Ventas SET Estado = 'PENDIENTE_CANCELAR' WHERE Id = @VentaId;";
                        db.Execute(sqlUpdate, new { VentaId = ventaId }, tx);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<VentaCancelacion> ObtenerCancelacionesPendientes()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string query = @"
                    SELECT vc.Id, vc.VentaId, vc.Motivo, vc.UsuarioSolicitaId, vc.FechaSolicitud, vc.Estado,
                           v.Folio as VentaFolio, v.Total as VentaTotal,
                           us.Nombre as NombreSolicitante
                    FROM VentaCancelaciones vc
                    INNER JOIN Ventas v ON vc.VentaId = v.Id
                    INNER JOIN Usuarios us ON vc.UsuarioSolicitaId = us.Id
                    WHERE vc.Estado = 'PENDIENTE'
                    ORDER BY vc.FechaSolicitud DESC";
                return db.Query<VentaCancelacion>(query).ToList();
            }
        }

        public void ProcesarCancelacion(int cancelacionId, int usuarioAutorizaId, bool aprobar)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        string nuevoEstado = aprobar ? "APROBADA" : "RECHAZADA";
                        
                        string updateCancelacion = @"
                            UPDATE VentaCancelaciones 
                            SET Estado = @Estado, UsuarioAutorizaId = @UsuarioAutorizaId, FechaAutorizacion = CURRENT_TIMESTAMP
                            WHERE Id = @Id RETURNING VentaId;";
                        
                        int ventaId = db.QuerySingle<int>(updateCancelacion, new { Estado = nuevoEstado, UsuarioAutorizaId = usuarioAutorizaId, Id = cancelacionId }, tx);

                        if (aprobar)
                        {
                            // 1. Marcar venta como CANCELADA
                            db.Execute("UPDATE Ventas SET Estado = 'CANCELADA' WHERE Id = @VentaId", new { VentaId = ventaId }, tx);

                            // 2. Devolver stock (Solo si no es servicio)
                            var detalles = db.Query<VentaDetalle>("SELECT Id, ProductoId, Cantidad FROM VentaDetalles WHERE VentaId = @VentaId", new { VentaId = ventaId }, tx);
                            foreach (var det in detalles)
                            {
                                db.Execute("UPDATE Productos SET StockActual = StockActual + @Cantidad WHERE Id = @ProductoId AND EsServicio = FALSE", new { det.Cantidad, det.ProductoId }, tx);
                                
                                bool aplicaCaducidad = db.QuerySingleOrDefault<bool>("SELECT AplicaCaducidad FROM Productos WHERE Id = @ProductoId", new { det.ProductoId }, tx);
                                if (aplicaCaducidad)
                                {
                                    var lotesVendidos = db.Query("SELECT ProductoLoteId, Cantidad FROM VentaDetalleLotes WHERE VentaDetalleId = @VentaDetalleId", new { VentaDetalleId = det.Id }, tx);
                                    foreach(var lv in lotesVendidos)
                                    {
                                        db.Execute("UPDATE ProductoLotes SET StockActual = StockActual + @Cantidad WHERE Id = @ProductoLoteId", new { Cantidad = lv.cantidad, ProductoLoteId = lv.productoloteid }, tx);
                                    }
                                }
                            }

                            // 3. Crear movimiento de caja negativo (DEVOLUCION)
                            var venta = db.QuerySingle<Venta>("SELECT * FROM Ventas WHERE Id = @VentaId", new { VentaId = ventaId }, tx);
                            
                            // Sumar los pagos en efectivo que se van a devolver
                            decimal efectivoADevolver = db.ExecuteScalar<decimal>("SELECT COALESCE(SUM(Importe), 0) FROM VentaPagos WHERE VentaId = @VentaId AND MetodoPago = 'EFECTIVO'", new { VentaId = ventaId }, tx);
                            
                            if (efectivoADevolver > 0)
                            {
                                string sqlCaja = @"
                                    INSERT INTO CajaMovimientos (CajaSesionId, Tipo, Importe, Concepto, UsuarioId)
                                    VALUES (@CajaSesionId, 'DEVOLUCION', @Importe, @Concepto, @UsuarioId);";
                                
                                db.Execute(sqlCaja, new { 
                                    CajaSesionId = venta.CajaSesionId, 
                                    Importe = -efectivoADevolver, // Negativo para restar al corte
                                    Concepto = $"Cancelación Folio {venta.Folio}",
                                    UsuarioId = usuarioAutorizaId
                                }, tx);

                                // Descontar del EfectivoEsperado en la sesión de caja (si sigue abierta)
                                db.Execute("UPDATE CajaSesiones SET EfectivoEsperado = EfectivoEsperado - @Monto WHERE Id = @CajaSesionId", 
                                    new { Monto = efectivoADevolver, CajaSesionId = venta.CajaSesionId }, tx);
                            }

                            // 4. Si la venta fue a crédito, restaurar el saldo del cliente
                            decimal creditoADevolver = db.ExecuteScalar<decimal>("SELECT COALESCE(SUM(Importe), 0) FROM VentaPagos WHERE VentaId = @VentaId AND MetodoPago = 'CREDITO'", new { VentaId = ventaId }, tx);
                            if (creditoADevolver > 0 && venta.ClienteId.HasValue)
                            {
                                db.Execute("UPDATE Clientes SET Saldo = Saldo - @Monto WHERE Id = @ClienteId", new { Monto = creditoADevolver, ClienteId = venta.ClienteId.Value }, tx);
                            }
                        }
                        else
                        {
                            // Si se rechaza, la venta vuelve a estado CONFIRMADO
                            db.Execute("UPDATE Ventas SET Estado = 'CONFIRMADO' WHERE Id = @VentaId", new { VentaId = ventaId }, tx);
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
