using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using momospos.Models;

namespace momospos.Repositories
{
    public class ProductoRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public List<Producto> ObtenerTodos()
        {
            var configRepo = new ConfiguracionRepository();
            bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";
            string orderBy = isFarmacia ? "p.SustanciaActiva ASC, p.Nombre ASC" : "p.Nombre ASC";

            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Producto>($"SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id ORDER BY {orderBy}").ToList();
            }
        }

        public Producto ObtenerPorCodigo(string codigoBarras)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<Producto>(
                    "SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id WHERE p.CodigoBarras = @CodigoBarras", 
                    new { CodigoBarras = codigoBarras });
            }
        }

        public Producto ObtenerPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<Producto>(
                    "SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id WHERE p.Id = @Id", 
                    new { Id = id });
            }
        }

        public List<Producto> BuscarPorNombre(string nombre)
        {
            var configRepo = new ConfiguracionRepository();
            bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";
            string orderBy = isFarmacia ? "p.SustanciaActiva ASC, p.Nombre ASC, p.Descripcion ASC" : "p.Nombre ASC, p.Descripcion ASC";

            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return db.Query<Producto>(
                        $"SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id ORDER BY {orderBy}").ToList();
                }
                else
                {
                    if (isFarmacia)
                    {
                        return db.Query<Producto>(
                            $"SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id WHERE p.Nombre ILIKE @Nombre OR p.CodigoBarras ILIKE @Nombre OR p.SustanciaActiva ILIKE @Nombre OR p.Descripcion ILIKE @Nombre OR p.ClaveProducto ILIKE @Nombre OR p.CodigoProveedor ILIKE @Nombre ORDER BY {orderBy}", 
                            new { Nombre = "%" + nombre + "%" }).ToList();
                    }
                    else
                    {
                        return db.Query<Producto>(
                            $"SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id WHERE p.Nombre ILIKE @Nombre OR p.CodigoBarras ILIKE @Nombre OR p.Descripcion ILIKE @Nombre OR p.ClaveProducto ILIKE @Nombre OR p.CodigoProveedor ILIKE @Nombre ORDER BY {orderBy}", 
                            new { Nombre = "%" + nombre + "%" }).ToList();
                    }
                }
            }
        }

        public void Guardar(Producto producto)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (producto.Id == 0)
                {
                    string sql = @"INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, UnidadMedidaId, PrecioCompra, PrecioVenta, PrecioMayoreo, CantidadMayoreo, StockActual, StockMinimo, EsServicio, PrecioFijo, AplicaCaducidad, RequiereReceta, SustanciaActiva, ClaveProducto, CodigoProveedor, RutaImagen) 
                                   VALUES (@CodigoBarras, @Nombre, @Descripcion, @CategoriaId, @UnidadMedidaId, @PrecioCompra, @PrecioVenta, @PrecioMayoreo, @CantidadMayoreo, @StockActual, @StockMinimo, @EsServicio, @PrecioFijo, @AplicaCaducidad, @RequiereReceta, @SustanciaActiva, @ClaveProducto, @CodigoProveedor, @RutaImagen) RETURNING Id;";
                    producto.Id = db.QuerySingle<int>(sql, producto);
                }
                else
                {
                    string sql = @"UPDATE Productos SET 
                                   CodigoBarras = @CodigoBarras, Nombre = @Nombre, Descripcion = @Descripcion, 
                                   CategoriaId = @CategoriaId, UnidadMedidaId = @UnidadMedidaId, 
                                   PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta, PrecioMayoreo = @PrecioMayoreo, CantidadMayoreo = @CantidadMayoreo, 
                                   StockActual = @StockActual, StockMinimo = @StockMinimo,
                                   EsServicio = @EsServicio, PrecioFijo = @PrecioFijo,
                                   AplicaCaducidad = @AplicaCaducidad, RequiereReceta = @RequiereReceta, SustanciaActiva = @SustanciaActiva,
                                   ClaveProducto = @ClaveProducto, CodigoProveedor = @CodigoProveedor, RutaImagen = @RutaImagen
                                   WHERE Id = @Id";
                    db.Execute(sql, producto);
                }
            }
        }

        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("DELETE FROM Productos WHERE Id = @Id", new { Id = id });
            }
        }

        public void AgregarStock(int productoId, decimal cantidad, decimal costoUnitario)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "UPDATE Productos SET StockActual = StockActual + @Cantidad, PrecioCompra = @CostoUnitario WHERE Id = @ProductoId;";
                db.Execute(sql, new { Cantidad = cantidad, CostoUnitario = costoUnitario, ProductoId = productoId });
            }
        }

        public List<ProductoLote> ObtenerLotesPorProducto(int productoId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<ProductoLote>("SELECT * FROM ProductoLotes WHERE ProductoId = @ProductoId ORDER BY FechaCaducidad ASC", new { ProductoId = productoId }).ToList();
            }
        }

        public void GuardarLote(ProductoLote lote)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (lote.Id == 0)
                {
                    db.Execute("INSERT INTO ProductoLotes (ProductoId, NumeroLote, FechaCaducidad, StockActual) VALUES (@ProductoId, @NumeroLote, @FechaCaducidad, @StockActual)", lote);
                }
                else
                {
                    db.Execute("UPDATE ProductoLotes SET NumeroLote = @NumeroLote, FechaCaducidad = @FechaCaducidad, StockActual = @StockActual WHERE Id = @Id", lote);
                }
                // Sincronizar el stock total del producto
                db.Execute("UPDATE Productos SET StockActual = (SELECT COALESCE(SUM(StockActual), 0) FROM ProductoLotes WHERE ProductoId = @ProductoId) WHERE Id = @ProductoId", new { ProductoId = lote.ProductoId });
            }
        }

        public void EliminarLote(int id, int productoId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("DELETE FROM ProductoLotes WHERE Id = @Id", new { Id = id });
                db.Execute("UPDATE Productos SET StockActual = (SELECT COALESCE(SUM(StockActual), 0) FROM ProductoLotes WHERE ProductoId = @ProductoId) WHERE Id = @ProductoId", new { ProductoId = productoId });
            }
        }

        public List<string> ImportarMasivo(List<Producto> productos, IProgress<int> progress = null)
        {
            var errores = new List<string>();
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Open();
                try
                {
                    var categoriasCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    int count = 0;

                    foreach (var prod in productos)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(prod.CategoriaNombreTemporal))
                            {
                                string catName = prod.CategoriaNombreTemporal.Trim();
                                if (categoriasCache.ContainsKey(catName))
                                {
                                    prod.CategoriaId = categoriasCache[catName];
                                }
                                else
                                {
                                    var existingCat = db.QueryFirstOrDefault<int?>("SELECT Id FROM Categorias WHERE Nombre ILIKE @Nombre", new { Nombre = catName });
                                    if (existingCat.HasValue)
                                    {
                                        prod.CategoriaId = existingCat.Value;
                                        categoriasCache[catName] = existingCat.Value;
                                    }
                                    else
                                    {
                                        int newCatId = db.QuerySingle<int>("INSERT INTO Categorias (Nombre) VALUES (@Nombre) RETURNING Id", new { Nombre = catName });
                                        prod.CategoriaId = newCatId;
                                        categoriasCache[catName] = newCatId;
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(prod.CodigoBarras))
                            {
                                var existing = db.QueryFirstOrDefault<Producto>("SELECT * FROM Productos WHERE CodigoBarras = @CodigoBarras", new { CodigoBarras = prod.CodigoBarras });
                                if (existing != null)
                                {
                                    prod.Id = existing.Id;
                                    string sqlUpdate = @"UPDATE Productos SET 
                                        Nombre = @Nombre, Descripcion = @Descripcion, 
                                        PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta, 
                                        PrecioMayoreo = @PrecioMayoreo,
                                        StockActual = @StockActual,
                                        StockMinimo = @StockMinimo,
                                        CategoriaId = COALESCE(@CategoriaId, CategoriaId)
                                        WHERE Id = @Id";
                                    db.Execute(sqlUpdate, prod);
                                    continue;
                                }
                            }
                            
                            string sqlInsert = @"INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, PrecioCompra, PrecioVenta, PrecioMayoreo, StockActual, StockMinimo, CategoriaId, EsServicio, PrecioFijo) 
                                           VALUES (@CodigoBarras, @Nombre, @Descripcion, @PrecioCompra, @PrecioVenta, @PrecioMayoreo, @StockActual, @StockMinimo, @CategoriaId, FALSE, TRUE)";
                            db.Execute(sqlInsert, prod);
                        }
                        catch (Exception ex)
                        {
                            errores.Add($"Producto '{prod.Nombre}' (Código: {prod.CodigoBarras}): {ex.Message}");
                        }
                        
                        count++;
                        progress?.Report(count);
                    }
                }
                catch (Exception ex)
                {
                    errores.Add($"Error general de importación: {ex.Message}");
                }
            }
            return errores;
        }

        public List<ReporteExistenciasDTO> ObtenerReporteExistencias()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT 
                        p.CodigoBarras, 
                        p.Nombre, 
                        p.Descripcion,
                        p.SustanciaActiva,
                        COALESCE(c.Nombre, 'Sin Categoría') AS Categoria, 
                        COALESCE(pl.StockActual, p.StockActual) AS StockActual, 
                        p.StockMinimo, 
                        (COALESCE(pl.StockActual, p.StockActual) * p.PrecioCompra) AS CostoInvertido, 
                        (COALESCE(pl.StockActual, p.StockActual) * (p.PrecioVenta - p.PrecioCompra)) AS GananciaProyectada,
                        pl.NumeroLote,
                        pl.FechaCaducidad,
                        CASE 
                            WHEN pl.FechaCaducidad IS NOT NULL AND pl.FechaCaducidad < CURRENT_DATE THEN 'Caducado'
                            WHEN pl.FechaCaducidad IS NOT NULL AND pl.FechaCaducidad < CURRENT_DATE + INTERVAL '30 days' THEN 'Por Caducar'
                            WHEN COALESCE(pl.StockActual, p.StockActual) <= 0 THEN 'Sin Stock'
                            WHEN COALESCE(pl.StockActual, p.StockActual) <= p.StockMinimo THEN 'Bajo Stock'
                            ELSE 'Suficiente'
                        END AS Estado
                    FROM Productos p
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id
                    LEFT JOIN ProductoLotes pl ON p.Id = pl.ProductoId AND p.AplicaCaducidad = TRUE
                    WHERE p.EsServicio = FALSE
                    ORDER BY 
                        CASE 
                            WHEN pl.FechaCaducidad IS NOT NULL AND pl.FechaCaducidad < CURRENT_DATE THEN 1
                            WHEN pl.FechaCaducidad IS NOT NULL AND pl.FechaCaducidad < CURRENT_DATE + INTERVAL '30 days' THEN 2
                            WHEN COALESCE(pl.StockActual, p.StockActual) <= 0 THEN 3
                            WHEN COALESCE(pl.StockActual, p.StockActual) <= p.StockMinimo THEN 4
                            ELSE 5
                        END ASC,
                        p.Nombre ASC;";
                return db.Query<ReporteExistenciasDTO>(sql).ToList();
            }
        }

        public List<ReporteExistenciasDTO> ObtenerReporteCaducidades()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Solo lotes, ignorar productos sin lote, ordenado por fecha de caducidad.
                string sql = @"
                    SELECT 
                        p.CodigoBarras, 
                        p.Nombre, 
                        p.Descripcion,
                        p.SustanciaActiva,
                        COALESCE(c.Nombre, 'Sin Categoría') AS Categoria, 
                        pl.StockActual, 
                        p.StockMinimo, 
                        (pl.StockActual * p.PrecioCompra) AS CostoInvertido, 
                        (pl.StockActual * (p.PrecioVenta - p.PrecioCompra)) AS GananciaProyectada,
                        pl.NumeroLote,
                        pl.FechaCaducidad,
                        CASE 
                            WHEN pl.FechaCaducidad < CURRENT_DATE THEN 'Caducado'
                            WHEN pl.FechaCaducidad < CURRENT_DATE + INTERVAL '90 days' THEN 'Por Caducar'
                            ELSE 'Vigente'
                        END AS Estado
                    FROM ProductoLotes pl
                    INNER JOIN Productos p ON pl.ProductoId = p.Id
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id
                    WHERE p.EsServicio = FALSE AND pl.StockActual > 0
                    ORDER BY pl.FechaCaducidad ASC, p.Nombre ASC;";
                return db.Query<ReporteExistenciasDTO>(sql).ToList();
            }
        }
    }
}
