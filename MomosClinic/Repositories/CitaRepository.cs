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

        public IEnumerable<Cita> ObtenerCitasPorRango(DateTime inicio, DateTime fin, string query = "")
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente, m.NombreCompleto as NombreMedico 
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    LEFT JOIN clinic.Medicos m ON c.MedicoId = m.Id
                    WHERE DATE(c.FechaHora) >= DATE(@Inicio) AND DATE(c.FechaHora) <= DATE(@Fin)";
                
                if (!string.IsNullOrWhiteSpace(query))
                {
                    sql += " AND p.NombreCompleto ILIKE @Query";
                }
                
                sql += " ORDER BY c.FechaHora ASC";
                
                return db.Query<Cita>(sql, new { Inicio = inicio, Fin = fin, Query = $"%{query}%" });
            }
        }

        public IEnumerable<Cita> ObtenerCitasDelDia(DateTime fecha)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente, m.NombreCompleto as NombreMedico 
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    LEFT JOIN clinic.Medicos m ON c.MedicoId = m.Id
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
                    SELECT c.*, p.NombreCompleto as NombrePaciente, m.NombreCompleto as NombreMedico
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    LEFT JOIN clinic.Medicos m ON c.MedicoId = m.Id
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
                    INSERT INTO clinic.Citas (PacienteId, MedicoId, FechaHora, Motivo, Estado, Notas) 
                    VALUES (@PacienteId, @MedicoId, @FechaHora, @Motivo, @Estado, @Notas) RETURNING Id;";
                int id = db.ExecuteScalar<int>(sql, cita);
                
                string folio = "CIT-" + cita.FechaHora.ToString("yyyyMM") + "-" + id.ToString("D4");
                db.Execute("UPDATE clinic.Citas SET Folio = @Folio WHERE Id = @Id", new { Folio = folio, Id = id });
                
                cita.Id = id;
                cita.Folio = folio;
            }
        }

        public void ActualizarEstado(int id, string estado)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE clinic.Citas SET Estado = @Estado WHERE Id = @Id", new { Estado = estado, Id = id });
            }
        }

        public void ActualizarEstado(int id, string estado, string motivoCancelacion, string notasExtras)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string notasStr = "MOTIVO CANCELACIÓN: " + motivoCancelacion;
                if (!string.IsNullOrWhiteSpace(notasExtras)) notasStr += " - " + notasExtras;

                db.Execute("UPDATE clinic.Citas SET Estado = @Estado, Notas = CONCAT(Notas, '\n', @NotasExtras) WHERE Id = @Id", 
                    new { Estado = estado, NotasExtras = notasStr, Id = id });
            }
        }

        public bool ExisteCitaEnFechaHora(DateTime fechaHora, int? medicoId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "SELECT COUNT(1) FROM clinic.Citas WHERE FechaHora = @FechaHora AND Estado NOT IN ('Cancelada')";
                
                if (medicoId.HasValue && medicoId.Value > 0)
                {
                    sql += " AND (MedicoId = @MedicoId OR MedicoId IS NULL)";
                }

                int count = db.ExecuteScalar<int>(sql, new { FechaHora = fechaHora, MedicoId = medicoId });
                return count > 0;
            }
        }

        public Cita ObtenerCitaActivaPaciente(int pacienteId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT * FROM clinic.Citas 
                    WHERE PacienteId = @PacienteId 
                    AND FechaHora >= @Ahora 
                    AND Estado IN ('Programada', 'Confirmada') 
                    ORDER BY FechaHora ASC
                    LIMIT 1";
                return db.QueryFirstOrDefault<Cita>(sql, new { PacienteId = pacienteId, Ahora = DateTime.Now });
            }
        }
        
        public void ActualizarFechaHora(int citaId, DateTime fechaHora, int? medicoId, string motivo, string notas)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE clinic.Citas 
                    SET FechaHora = @FechaHora, MedicoId = @MedicoId, Motivo = @Motivo, Notas = @Notas
                    WHERE Id = @Id";
                db.Execute(sql, new { Id = citaId, FechaHora = fechaHora, MedicoId = medicoId, Motivo = motivo, Notas = notas });
            }
        }

        public Cita ObtenerPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente, m.NombreCompleto as NombreMedico 
                    FROM clinic.Citas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    LEFT JOIN clinic.Medicos m ON c.MedicoId = m.Id
                    WHERE c.Id = @Id";
                return db.QueryFirstOrDefault<Cita>(sql, new { Id = id });
            }
        }

        public void ActualizarCita(Cita cita)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE clinic.Citas 
                    SET PacienteId = @PacienteId, MedicoId = @MedicoId, FechaHora = @FechaHora, 
                        Motivo = @Motivo, Notas = @Notas, Estado = @Estado
                    WHERE Id = @Id";
                db.Execute(sql, cita);
            }
        }
    }
}
