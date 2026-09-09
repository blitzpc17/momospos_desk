using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using MomosClinic.Models;
using momospos.Helpers;

namespace MomosClinic.Repositories
{
    public class ProductoSugeridoRepository
    {
        private string GetConnectionString()
        {
            return ConfiguracionHelper.ObtenerCadenaConexion();
        }

        public void Sugerir(ProductoSugerido prod)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "INSERT INTO ProductosSugeridos (NombreProducto, CantidadSolicitada, SolicitadoPor) VALUES (@NombreProducto, @CantidadSolicitada, @SolicitadoPor)";
                db.Execute(sql, prod);
            }
        }

        public List<ProductoSugerido> ObtenerNoEvaluados()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT p.*, u.Nombre as UsuarioNombre 
                    FROM ProductosSugeridos p
                    LEFT JOIN Usuarios u ON p.SolicitadoPor = u.Id
                    WHERE p.Evaluado = FALSE 
                    ORDER BY p.FechaSolicitud DESC";
                return db.Query<ProductoSugerido>(sql).ToList();
            }
        }
        
        public void MarcarEvaluado(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE ProductosSugeridos SET Evaluado = TRUE WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
