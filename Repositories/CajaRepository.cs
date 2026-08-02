using System;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using momospos.Models;

namespace momospos.Repositories
{
    public class CajaRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public CajaSesion ObtenerSesionAbierta()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<CajaSesion>(
                    "SELECT * FROM CajaSesiones WHERE Estado = 'ABIERTA' ORDER BY FechaApertura DESC LIMIT 1");
            }
        }

        public void AbrirCaja(CajaSesion sesion)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO CajaSesiones (CajaId, UsuarioAperturaId, FondoInicial, EfectivoEsperado, Estado)
                               VALUES (1, @UsuarioAperturaId, @FondoInicial, @FondoInicial, 'ABIERTA')"; // Asumimos CajaId=1 por defecto
                db.Execute(sql, sesion);
            }
        }

        public void CerrarCaja(CajaSesion sesion)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"UPDATE CajaSesiones SET 
                               UsuarioCierreId = @UsuarioCierreId, FechaCierre = @FechaCierre, 
                               EfectivoContado = @EfectivoContado, Diferencia = @Diferencia, 
                               Estado = 'CERRADA'
                               WHERE Id = @Id";
                db.Execute(sql, sesion);
            }
        }
        
        public void RegistrarMovimientoCaja(CajaMovimiento mov, IDbTransaction transaction = null)
        {
            string sql = @"INSERT INTO CajaMovimientos (CajaSesionId, Tipo, Importe, Concepto, UsuarioId, Fecha)
                           VALUES (@CajaSesionId, @Tipo, @Importe, @Concepto, @UsuarioId, @Fecha)";
            
            if (transaction != null)
            {
                transaction.Connection.Execute(sql, mov, transaction);
            }
            else
            {
                using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
                {
                    db.Execute(sql, mov);
                }
            }
        }
        
        public void ActualizarEfectivoEsperado(int cajaSesionId, decimal importe)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE CajaSesiones SET EfectivoEsperado = EfectivoEsperado + @Importe WHERE Id = @Id", 
                           new { Importe = importe, Id = cajaSesionId });
            }
        }

        public System.Collections.Generic.IEnumerable<CajaMovimiento> ObtenerMovimientosSesion(int cajaSesionId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<CajaMovimiento>("SELECT * FROM CajaMovimientos WHERE CajaSesionId = @Id ORDER BY Fecha DESC", new { Id = cajaSesionId });
            }
        }
    }
}
