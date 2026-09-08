using Dapper;
using MomosClinic.Models;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using Npgsql;

namespace MomosClinic.Repositories
{
    public class MedicoRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public IEnumerable<Medico> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT m.*, e.Nombre as Especialidad 
                    FROM clinic.Medicos m 
                    LEFT JOIN clinic.EspecialidadesMedicas e ON m.EspecialidadId = e.Id 
                    ORDER BY m.NombreCompleto";
                return db.Query<Medico>(sql);
            }
        }

        public IEnumerable<Medico> ObtenerActivos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT m.*, e.Nombre as Especialidad 
                    FROM clinic.Medicos m 
                    LEFT JOIN clinic.EspecialidadesMedicas e ON m.EspecialidadId = e.Id 
                    WHERE m.Activo = true 
                    ORDER BY m.NombreCompleto";
                return db.Query<Medico>(sql);
            }
        }

        public Medico ObtenerPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT m.*, e.Nombre as Especialidad 
                    FROM clinic.Medicos m 
                    LEFT JOIN clinic.EspecialidadesMedicas e ON m.EspecialidadId = e.Id 
                    WHERE m.Id = @Id";
                return db.QueryFirstOrDefault<Medico>(sql, new { Id = id });
            }
        }

        public void Insertar(Medico medico)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO clinic.Medicos (NombreCompleto, EspecialidadId, Telefono, Correo, Activo, RutaImagen) 
                               VALUES (@NombreCompleto, @EspecialidadId, @Telefono, @Correo, @Activo, @RutaImagen)";
                db.Execute(sql, medico);
            }
        }

        public void Actualizar(Medico medico)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"UPDATE clinic.Medicos 
                               SET NombreCompleto = @NombreCompleto, EspecialidadId = @EspecialidadId, 
                                   Telefono = @Telefono, Correo = @Correo, Activo = @Activo,
                                   RutaImagen = @RutaImagen, ActualizadoEn = CURRENT_TIMESTAMP,
                                   FechaBaja = CASE WHEN @Activo = false AND Activo = true THEN CURRENT_TIMESTAMP ELSE FechaBaja END
                               WHERE Id = @Id";
                db.Execute(sql, medico);
            }
        }

        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("DELETE FROM clinic.Medicos WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
