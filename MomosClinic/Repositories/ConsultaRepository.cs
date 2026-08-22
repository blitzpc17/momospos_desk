using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using Npgsql;
using MomosClinic.Models;

namespace MomosClinic.Repositories
{
    public class ConsultaRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public IEnumerable<Consulta> ObtenerConsultasPorPaciente(int pacienteId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente 
                    FROM clinic.Consultas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    WHERE c.PacienteId = @PacienteId
                    ORDER BY c.CreadoEn DESC";
                return db.Query<Consulta>(sql, new { PacienteId = pacienteId });
            }
        }
        
        public Consulta ObtenerPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente 
                    FROM clinic.Consultas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    WHERE c.Id = @Id";
                return db.QueryFirstOrDefault<Consulta>(sql, new { Id = id });
            }
        }
        
        public IEnumerable<Consulta> BuscarRecientes(string query = "")
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    SELECT c.*, p.NombreCompleto as NombrePaciente 
                    FROM clinic.Consultas c
                    JOIN clinic.Pacientes p ON c.PacienteId = p.Id
                    WHERE p.NombreCompleto ILIKE @Query OR c.Diagnostico ILIKE @Query
                    ORDER BY c.CreadoEn DESC LIMIT 50";
                return db.Query<Consulta>(sql, new { Query = $"%{query}%" });
            }
        }

        public int Insertar(Consulta consulta)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    INSERT INTO clinic.Consultas (
                        CitaId, PacienteId, Peso, Talla, Temperatura, PresionArterial, 
                        FrecuenciaCardiaca, FrecuenciaRespiratoria, SaturacionOxigeno, IMC,
                        MotivoConsulta, ExploracionFisica, Analisis, Diagnostico, PlanTratamiento
                    ) VALUES (
                        @CitaId, @PacienteId, @Peso, @Talla, @Temperatura, @PresionArterial,
                        @FrecuenciaCardiaca, @FrecuenciaRespiratoria, @SaturacionOxigeno, @IMC,
                        @MotivoConsulta, @ExploracionFisica, @Analisis, @Diagnostico, @PlanTratamiento
                    ) RETURNING Id;";
                return db.ExecuteScalar<int>(sql, consulta);
            }
        }
        
        public void ActualizarCobro(int id, string folioCobro)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE clinic.Consultas SET CobroGenerado = TRUE, FolioCobro = @Folio WHERE Id = @Id", 
                    new { Folio = folioCobro, Id = id });
            }
        }
    }
}
