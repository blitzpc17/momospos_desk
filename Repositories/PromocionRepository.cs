using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using momospos.Models;
using System;

namespace momospos.Repositories
{
    public class PromocionRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public IEnumerable<Promocion> ObtenerTodas()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT 
                        pr.*, 
                        p.Nombre as ProductoNombre, 
                        p.CodigoBarras as ProductoCodigo
                    FROM Promociones pr
                    LEFT JOIN Productos p ON pr.ProductoId = p.Id
                    ORDER BY pr.CreadoEn DESC";
                return db.Query<Promocion>(sql).ToList();
            }
        }

        public IEnumerable<Promocion> ObtenerActivasPorProducto(int productoId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT * FROM Promociones 
                    WHERE ProductoId = @ProductoId 
                      AND Activo = TRUE 
                      AND FechaInicio <= CURRENT_TIMESTAMP 
                      AND FechaFin >= CURRENT_TIMESTAMP";
                return db.Query<Promocion>(sql, new { ProductoId = productoId }).ToList();
            }
        }

        public void Registrar(Promocion promocion)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO Promociones (ProductoId, Nombre, Tipo, CantidadRequerida, CantidadRegalo, DescuentoPorcentaje, AplicaTotalVenta, MontoMinimoVenta, FechaInicio, FechaFin, Activo) 
                               VALUES (@ProductoId, @Nombre, @Tipo, @CantidadRequerida, @CantidadRegalo, @DescuentoPorcentaje, @AplicaTotalVenta, @MontoMinimoVenta, @FechaInicio, @FechaFin, @Activo)";
                db.Execute(sql, promocion);
            }
        }

        public void Actualizar(Promocion promocion)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"UPDATE Promociones SET 
                               ProductoId = @ProductoId, Nombre = @Nombre, Tipo = @Tipo, 
                               CantidadRequerida = @CantidadRequerida, CantidadRegalo = @CantidadRegalo, DescuentoPorcentaje = @DescuentoPorcentaje, 
                               AplicaTotalVenta = @AplicaTotalVenta, MontoMinimoVenta = @MontoMinimoVenta, FechaInicio = @FechaInicio, FechaFin = @FechaFin, Activo = @Activo 
                               WHERE Id = @Id";
                db.Execute(sql, promocion);
            }
        }

        public void CambiarEstado(int id, bool activo)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "UPDATE Promociones SET Activo = @Activo WHERE Id = @Id";
                db.Execute(sql, new { Id = id, Activo = activo });
            }
        }
        
        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "DELETE FROM Promociones WHERE Id = @Id";
                db.Execute(sql, new { Id = id });
            }
        }
    }
}
