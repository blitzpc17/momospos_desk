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
    public class OrdenesCobroRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public int Insertar(OrdenCobro orden)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"
                    INSERT INTO OrdenesCobro (Referencia, ModuloOrigen, Estado, JsonDetalles) 
                    VALUES (@Referencia, @ModuloOrigen, 'PENDIENTE', @JsonDetalles) RETURNING Id;";
                return db.QuerySingle<int>(sql, orden);
            }
        }

        public IEnumerable<OrdenCobro> ObtenerPendientes()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Solo pendientes de hoy para no saturar
                string sql = "SELECT * FROM OrdenesCobro WHERE Estado = 'PENDIENTE' AND Fecha::date = CURRENT_DATE ORDER BY Fecha DESC";
                return db.Query<OrdenCobro>(sql).ToList();
            }
        }

        public void ActualizarEstado(int id, string nuevoEstado)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE OrdenesCobro SET Estado = @Estado WHERE Id = @Id", new { Estado = nuevoEstado, Id = id });
            }
        }
    }
}
