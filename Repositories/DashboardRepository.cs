using Dapper;
using Npgsql;
using System;
using System.Configuration;
using System.Data;

namespace momospos.Repositories
{
    public class DashboardMetrics
    {
        public decimal VentasHoy { get; set; }
        public int TicketsHoy { get; set; }
        public decimal CuentasPorCobrar { get; set; }
        public int ProductosCriticos { get; set; }
        public decimal RetirosHoy { get; set; }
        public System.Collections.Generic.List<momospos.Models.ArticuloVendidoDTO> ProductosMasVendidos { get; set; } = new System.Collections.Generic.List<momospos.Models.ArticuloVendidoDTO>();
        public System.Collections.Generic.List<momospos.Models.ArticuloVendidoDTO> ProductosMenosVendidos { get; set; } = new System.Collections.Generic.List<momospos.Models.ArticuloVendidoDTO>();
        public System.Collections.Generic.List<momospos.Models.ReporteExistenciasDTO> ProductosStockBajo { get; set; } = new System.Collections.Generic.List<momospos.Models.ReporteExistenciasDTO>();
    }

    public class DashboardRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public DashboardMetrics ObtenerMetricas(DateTime inicio, DateTime fin)
        {
            var metricas = new DashboardMetrics();
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                var inicioDia = inicio.Date;
                var finDia = fin.Date.AddDays(1).AddTicks(-1);

                // Ventas y Tickets de Hoy
                string sqlVentas = @"
                    SELECT COALESCE(SUM(Total), 0) AS VentasHoy, COUNT(Id) AS TicketsHoy
                    FROM Ventas 
                    WHERE Fecha BETWEEN @Inicio AND @Fin AND Estado = 'CONFIRMADO';";
                var ventasResult = db.QuerySingle(sqlVentas, new { Inicio = inicioDia, Fin = finDia });
                metricas.VentasHoy = Convert.ToDecimal(ventasResult.ventashoy);
                metricas.TicketsHoy = Convert.ToInt32(ventasResult.ticketshoy);

                // Cuentas por Cobrar (Suma del saldo de clientes)
                string sqlCuentas = "SELECT COALESCE(SUM(Saldo), 0) FROM Clientes WHERE Saldo > 0;";
                metricas.CuentasPorCobrar = db.ExecuteScalar<decimal>(sqlCuentas);

                // Productos Críticos (Sin stock o Bajo stock)
                string sqlCriticos = "SELECT COUNT(Id) FROM Productos WHERE EsServicio = FALSE AND StockActual <= StockMinimo;";
                metricas.ProductosCriticos = db.ExecuteScalar<int>(sqlCriticos);

                // Retiros Hoy
                string sqlRetiros = "SELECT COALESCE(SUM(Importe), 0) FROM CajaMovimientos WHERE Tipo = 'RETIRO' AND Fecha BETWEEN @Inicio AND @Fin;";
                metricas.RetirosHoy = db.ExecuteScalar<decimal>(sqlRetiros, new { Inicio = inicioDia, Fin = finDia });

                // Productos Más Vendidos
                string sqlMasVendidos = @"
                    SELECT p.CodigoBarras, p.Nombre, SUM(vd.Cantidad) as CantidadTotal, SUM(vd.Subtotal) as TotalGenerado
                    FROM VentaDetalles vd
                    INNER JOIN Ventas v ON vd.VentaId = v.Id
                    INNER JOIN Productos p ON vd.ProductoId = p.Id
                    WHERE v.Fecha BETWEEN @Inicio AND @Fin AND v.Estado = 'CONFIRMADO'
                    GROUP BY p.CodigoBarras, p.Nombre
                    ORDER BY CantidadTotal DESC LIMIT 5;";
                metricas.ProductosMasVendidos = db.Query<momospos.Models.ArticuloVendidoDTO>(sqlMasVendidos, new { Inicio = inicioDia, Fin = finDia }).AsList();

                // Productos Menos Vendidos
                string sqlMenosVendidos = @"
                    SELECT p.CodigoBarras, p.Nombre, SUM(vd.Cantidad) as CantidadTotal, SUM(vd.Subtotal) as TotalGenerado
                    FROM VentaDetalles vd
                    INNER JOIN Ventas v ON vd.VentaId = v.Id
                    INNER JOIN Productos p ON vd.ProductoId = p.Id
                    WHERE v.Fecha BETWEEN @Inicio AND @Fin AND v.Estado = 'CONFIRMADO'
                    GROUP BY p.CodigoBarras, p.Nombre
                    ORDER BY CantidadTotal ASC LIMIT 5;";
                metricas.ProductosMenosVendidos = db.Query<momospos.Models.ArticuloVendidoDTO>(sqlMenosVendidos, new { Inicio = inicioDia, Fin = finDia }).AsList();

                // Productos con Stock Bajo
                string sqlStockBajo = @"
                    SELECT CodigoBarras, Nombre, StockActual, StockMinimo 
                    FROM Productos 
                    WHERE EsServicio = FALSE AND StockActual <= StockMinimo 
                    ORDER BY StockActual ASC;";
                metricas.ProductosStockBajo = db.Query<momospos.Models.ReporteExistenciasDTO>(sqlStockBajo).AsList();
            }
            return metricas;
        }
    }
}
