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
                // Trae citas que ocurrirán en los próximos 'X' minutos y que estén Programadas o Confirmadas
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente 
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    WHERE c.FechaHora BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + (@Minutos || ' minutes')::interval
                    AND c.Estado IN ('Programada', 'Confirmada')
                    ORDER BY c.FechaHora ASC";
                return db.Query<Cita>(sql, new { Minutos = minutosAnticipacion });
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
    }
}
