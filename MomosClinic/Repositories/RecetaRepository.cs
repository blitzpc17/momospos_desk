using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using Npgsql;
using MomosClinic.Models;

namespace MomosClinic.Repositories
{
    public class RecetaRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public void Insertar(Receta receta)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        string sqlReceta = @"
                            INSERT INTO clinic.Recetas (ConsultaId, PacienteId, IndicacionesGenerales) 
                            VALUES (@ConsultaId, @PacienteId, @IndicacionesGenerales) RETURNING Id;";
                        receta.Id = db.ExecuteScalar<int>(sqlReceta, receta, tran);

                        foreach (var det in receta.Detalles)
                        {
                            det.RecetaId = receta.Id;
                            string sqlDet = @"
                                INSERT INTO clinic.RecetaDetalles (
                                    RecetaId, ProductoId, NombreMedicamento, Dosis, 
                                    Frecuencia, Duracion, Cantidad, Instrucciones
                                ) VALUES (
                                    @RecetaId, @ProductoId, @NombreMedicamento, @Dosis, 
                                    @Frecuencia, @Duracion, @Cantidad, @Instrucciones
                                );";
                            db.Execute(sqlDet, det, tran);
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
        public IEnumerable<dynamic> BuscarRecientes(string query = "")
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT r.Id, r.ConsultaId, r.PacienteId, r.FechaEmision, p.NombreCompleto as PacienteNombre, r.IndicacionesGenerales 
                    FROM clinic.Recetas r 
                    JOIN clinic.Pacientes p ON r.PacienteId = p.Id 
                    WHERE p.NombreCompleto ILIKE @Query
                    ORDER BY r.FechaEmision DESC LIMIT 50";
                return db.Query(sql, new { Query = $"%{query}%" });
            }
        }

        public IEnumerable<dynamic> ObtenerPorPaciente(int pacienteId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT r.Id, r.ConsultaId, r.PacienteId, r.FechaEmision, p.NombreCompleto as PacienteNombre, r.IndicacionesGenerales 
                    FROM clinic.Recetas r 
                    JOIN clinic.Pacientes p ON r.PacienteId = p.Id 
                    WHERE r.PacienteId = @PacienteId
                    ORDER BY r.FechaEmision DESC";
                return db.Query(sql, new { PacienteId = pacienteId });
            }
        }

        public Receta ObtenerCompleta(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                var receta = db.QueryFirstOrDefault<Receta>("SELECT * FROM clinic.Recetas WHERE Id = @Id", new { Id = id });
                if (receta != null)
                {
                    receta.Detalles = db.Query<RecetaDetalle>("SELECT * FROM clinic.RecetaDetalles WHERE RecetaId = @Id", new { Id = id }).AsList();
                }
                return receta;
            }
        }
        // This method interacts with MomosPOS public.Productos table IF configured.
        public IEnumerable<dynamic> BuscarProductosFarmacia(string query)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Verifica si la tabla public.Productos existe
                int count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'productos'");
                if (count == 0) return new List<dynamic>();

                string sql = @"
                    SELECT Id, CodigoBarras, Nombre, PrecioVenta 
                    FROM public.Productos 
                    WHERE Activo = TRUE AND (Nombre ILIKE @Query OR CodigoBarras = @Exact)
                    ORDER BY Nombre LIMIT 50";
                return db.Query(sql, new { Query = $"%{query}%", Exact = query });
            }
        }
    }
}
