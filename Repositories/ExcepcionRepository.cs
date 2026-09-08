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
    public class ExcepcionRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString ?? "";
        }

        public void Insertar(Exception ex, string modulo, int? usuarioId = null)
        {
            try
            {
                string connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString)) return;

                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    string sql = @"
                        INSERT INTO Excepciones (UsuarioId, Modulo, Mensaje, StackTrace) 
                        VALUES (@UsuarioId, @Modulo, @Mensaje, @StackTrace)";
                        
                    db.Execute(sql, new 
                    { 
                        UsuarioId = usuarioId, 
                        Modulo = modulo ?? "Desconocido", 
                        Mensaje = ex.Message, 
                        StackTrace = ex.StackTrace 
                    });
                }
            }
            catch 
            {
                // Fallback de último recurso: No podemos loguear una excepción al intentar loguear una excepción.
            }
        }

        public List<ExcepcionLog> ObtenerPorFecha(DateTime fecha)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT 
                        e.*, 
                        u.Nombre AS UsuarioNombre 
                    FROM Excepciones e
                    LEFT JOIN Usuarios u ON e.UsuarioId = u.Id
                    WHERE e.FechaHora::date = @Fecha::date
                    ORDER BY e.FechaHora DESC";
                    
                return db.Query<ExcepcionLog>(sql, new { Fecha = fecha }).ToList();
            }
        }
    }
}
