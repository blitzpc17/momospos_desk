using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using Npgsql;
using MomosClinic.Models;

namespace MomosClinic.Repositories
{
    public class PacienteRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public IEnumerable<Paciente> ObtenerTodos(bool mostrarInactivos = false)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = mostrarInactivos ? "SELECT * FROM clinic.Pacientes ORDER BY Id ASC" : "SELECT * FROM clinic.Pacientes WHERE Activo = TRUE ORDER BY Id ASC";
                return db.Query<Paciente>(sql);
            }
        }
        
        public IEnumerable<Paciente> Buscar(string query, bool mostrarInactivos = false)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string condicionEstatus = mostrarInactivos ? "" : "Activo = TRUE AND ";

                if (int.TryParse(query, out int id)) 
                {
                    string sql = $"SELECT * FROM clinic.Pacientes WHERE {condicionEstatus}(Id = @Id OR NombreCompleto ILIKE @Query OR Telefono ILIKE @Query) ORDER BY Id ASC LIMIT 50";
                    return db.Query<Paciente>(sql, new { Id = id, Query = $"%{query}%" });
                }
                else
                {
                    string sql = $"SELECT * FROM clinic.Pacientes WHERE {condicionEstatus}(NombreCompleto ILIKE @Query OR Telefono ILIKE @Query) ORDER BY Id ASC LIMIT 50";
                    return db.Query<Paciente>(sql, new { Query = $"%{query}%" });
                }
            }
        }

        public Paciente ObtenerPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<Paciente>("SELECT * FROM clinic.Pacientes WHERE Id = @Id", new { Id = id });
            }
        }

        public void Insertar(Paciente paciente)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    INSERT INTO clinic.Pacientes (
                        NombreCompleto, FechaNacimiento, Genero, Telefono, Email, Direccion,
                        Alergias, AntecedentesFamiliares, AntecedentesPatologicos, TipoSangre, CreadoPor
                    ) VALUES (
                        @NombreCompleto, @FechaNacimiento, @Genero, @Telefono, @Email, @Direccion,
                        @Alergias, @AntecedentesFamiliares, @AntecedentesPatologicos, @TipoSangre, @CreadoPor
                    ) RETURNING Id";
                paciente.Id = db.ExecuteScalar<int>(sql, paciente);
            }
        }

        public void Actualizar(Paciente paciente)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE clinic.Pacientes SET 
                        NombreCompleto = @NombreCompleto, 
                        FechaNacimiento = @FechaNacimiento, 
                        Genero = @Genero, 
                        Telefono = @Telefono, 
                        Email = @Email, 
                        Direccion = @Direccion,
                        Alergias = @Alergias, 
                        AntecedentesFamiliares = @AntecedentesFamiliares, 
                        AntecedentesPatologicos = @AntecedentesPatologicos, 
                        TipoSangre = @TipoSangre,
                        ModificadoPor = @ModificadoPor,
                        Activo = @Activo,
                        MotivoBaja = @MotivoBaja,
                        BajaPor = @BajaPor
                    WHERE Id = @Id";
                db.Execute(sql, paciente);
            }
        }

        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE clinic.Pacientes SET Activo = FALSE WHERE Id = @Id", new { Id = id });
            }
        }

        public bool ExistePacienteDuplicado(string nombreCompleto, string telefono, int idExcluir = 0)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto)) return false;

            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (string.IsNullOrWhiteSpace(telefono))
                {
                    string sql = "SELECT COUNT(1) FROM clinic.Pacientes WHERE NombreCompleto ILIKE @Nombre AND Id != @Id";
                    return db.ExecuteScalar<int>(sql, new { Nombre = nombreCompleto.Trim(), Id = idExcluir }) > 0;
                }
                else
                {
                    string sql = "SELECT COUNT(1) FROM clinic.Pacientes WHERE NombreCompleto ILIKE @Nombre AND Telefono = @Telefono AND Id != @Id";
                    return db.ExecuteScalar<int>(sql, new { Nombre = nombreCompleto.Trim(), Telefono = telefono.Trim(), Id = idExcluir }) > 0;
                }
            }
        }
    }
}
