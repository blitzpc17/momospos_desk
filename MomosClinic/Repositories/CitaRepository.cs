using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using Npgsql;
using MomosClinic.Models;

namespace MomosClinic.Repositories
{
    public class CitaRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public IEnumerable<Cita> ObtenerCitasDelDia(DateTime fecha)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente 
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    WHERE DATE(c.FechaHora) = DATE(@Fecha)
                    ORDER BY c.FechaHora ASC";
                return db.Query<Cita>(sql, new { Fecha = fecha });
            }
        }

        public IEnumerable<Cita> ObtenerProximasCitas(int minutosAnticipacion)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Pasamos la hora exacta desde C# para evitar problemas de zona horaria con la base de datos
                DateTime ahora = DateTime.Now;
                DateTime limite = ahora.AddMinutes(minutosAnticipacion);

                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente 
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    WHERE c.FechaHora BETWEEN @Ahora AND @Limite
                    AND c.Estado IN ('Programada', 'Confirmada')
                    ORDER BY c.FechaHora ASC";
                return db.Query<Cita>(sql, new { Ahora = ahora, Limite = limite });
            }
        }

        public void Insertar(Cita cita)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    INSERT INTO clinic.Citas (PacienteId, FechaHora, Motivo, Estado, Notas) 
                    VALUES (@PacienteId, @FechaHora, @Motivo, @Estado, @Notas)";
                db.Execute(sql, cita);
            }
        }

        public void ActualizarEstado(int id, string estado)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE clinic.Citas SET Estado = @Estado WHERE Id = @Id", new { Estado = estado, Id = id });
            }
        }

        public bool ExisteCitaEnFechaHora(DateTime fechaHora)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "SELECT COUNT(1) FROM clinic.Citas WHERE FechaHora = @FechaHora AND Estado NOT IN ('Cancelada')";
                int count = db.ExecuteScalar<int>(sql, new { FechaHora = fechaHora });
                return count > 0;
            }
        }
    }
}
