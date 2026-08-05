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
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Producto>("SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id ORDER BY p.Nombre").ToList();
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
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return db.Query<Producto>(
                        "SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id ORDER BY p.Nombre, p.Descripcion").ToList();
                }
                else
                {
                    return db.Query<Producto>(
                        "SELECT p.*, u.PermiteFraccion, u.Abreviatura AS UnidadMedidaAbreviatura FROM Productos p LEFT JOIN UnidadesMedida u ON p.UnidadMedidaId = u.Id WHERE p.Nombre ILIKE @Nombre OR p.CodigoBarras ILIKE @Nombre OR p.Descripcion ILIKE @Nombre ORDER BY p.Nombre, p.Descripcion", 
                        new { Nombre = "%" + nombre + "%" }).ToList();
                }
            }
        }

        public void Guardar(Producto producto)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (producto.Id == 0)
                {
                    string sql = @"INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, UnidadMedidaId, PrecioCompra, PrecioVenta, StockActual, StockMinimo, EsServicio, PrecioFijo) 
                                   VALUES (@CodigoBarras, @Nombre, @Descripcion, @CategoriaId, @UnidadMedidaId, @PrecioCompra, @PrecioVenta, @StockActual, @StockMinimo, @EsServicio, @PrecioFijo)";
                    db.Execute(sql, producto);
                }
                else
                {
                    string sql = @"UPDATE Productos SET 
                                   CodigoBarras = @CodigoBarras, Nombre = @Nombre, Descripcion = @Descripcion, 
                                   CategoriaId = @CategoriaId, UnidadMedidaId = @UnidadMedidaId, 
                                   PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta, 
                                   StockActual = @StockActual, StockMinimo = @StockMinimo,
                                   EsServicio = @EsServicio, PrecioFijo = @PrecioFijo
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

        public void ImportarMasivo(List<Producto> productos)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Open();
                using (var tx = db.BeginTransaction())
                {
                    try
                    {
                        foreach (var prod in productos)
                        {
                            if (!string.IsNullOrWhiteSpace(prod.CodigoBarras))
                            {
                                var existing = db.QueryFirstOrDefault<Producto>("SELECT * FROM Productos WHERE CodigoBarras = @CodigoBarras", new { CodigoBarras = prod.CodigoBarras }, tx);
                                if (existing != null)
                                {
                                    prod.Id = existing.Id;
                                    string sqlUpdate = @"UPDATE Productos SET 
                                        Nombre = @Nombre, Descripcion = @Descripcion, 
                                        PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta, 
                                        StockActual = @StockActual
                                        WHERE Id = @Id";
                                    db.Execute(sqlUpdate, prod, tx);
                                    continue;
                                }
                            }
                            
                            string sqlInsert = @"INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, PrecioCompra, PrecioVenta, StockActual, StockMinimo, EsServicio, PrecioFijo) 
                                           VALUES (@CodigoBarras, @Nombre, @Descripcion, @PrecioCompra, @PrecioVenta, @StockActual, 0, FALSE, TRUE)";
                            db.Execute(sqlInsert, prod, tx);
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
