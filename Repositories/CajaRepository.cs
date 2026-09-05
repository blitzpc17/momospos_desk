using System;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using momospos.Models;
using System.Collections.Generic;

namespace momospos.Repositories
{
    public class CajaRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public CajaSesion ObtenerSesionAbierta(int cajaId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<CajaSesion>(
                    "SELECT * FROM CajaSesiones WHERE Estado = 'ABIERTA' AND CajaId = @CajaId ORDER BY FechaApertura DESC LIMIT 1",
                    new { CajaId = cajaId });
            }
        }

        public void AbrirCaja(CajaSesion sesion)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO CajaSesiones (CajaId, UsuarioAperturaId, FondoInicial, EfectivoEsperado, Estado)
                               VALUES (@CajaId, @UsuarioAperturaId, @FondoInicial, @FondoInicial, 'ABIERTA')";
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

        public CajaSesion ObtenerSesionPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<CajaSesion>("SELECT * FROM CajaSesiones WHERE Id = @Id", new { Id = id });
            }
        }

        public System.Collections.Generic.List<CorteHistorialDTO> ObtenerReporteCortes(DateTime inicio, DateTime fin)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                inicio = inicio.Date;
                fin = fin.Date.AddDays(1).AddTicks(-1);

                string sql = @"
                    SELECT 
                        c.Id AS SesionId,
                        c.CajaId,
                        u.Nombre AS NombreCajero,
                        c.FechaApertura,
                        c.FechaCierre,
                        c.FondoInicial,
                        c.EfectivoEsperado,
                        c.EfectivoContado,
                        c.Diferencia,
                        c.Estado,
                        c.Observaciones
                    FROM CajaSesiones c
                    LEFT JOIN Usuarios u ON c.UsuarioAperturaId = u.Id
                    WHERE c.FechaApertura BETWEEN @Inicio AND @Fin
                      AND c.Estado = 'CERRADA'
                    ORDER BY c.FechaCierre DESC;";
                
                return db.Query<CorteHistorialDTO>(sql, new { Inicio = inicio, Fin = fin }).ToList();
            }
        }

        public (int TotalCortes, decimal SumaEsperada, decimal SumaContada, decimal SumaDiferencia, decimal FondoTotal) ObtenerResumenCorteDia(DateTime fecha)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                var inicio = fecha.Date;
                var fin = fecha.Date.AddDays(1).AddTicks(-1);
                
                string sql = @"
                    SELECT 
                        COUNT(Id) AS TotalCortes,
                        COALESCE(SUM(EfectivoEsperado), 0) AS SumaEsperada,
                        COALESCE(SUM(EfectivoContado), 0) AS SumaContada,
                        COALESCE(SUM(Diferencia), 0) AS SumaDiferencia,
                        COALESCE(SUM(FondoInicial), 0) AS FondoTotal
                    FROM CajaSesiones
                    WHERE FechaCierre BETWEEN @Inicio AND @Fin
                      AND Estado = 'CERRADA'";
                
                return db.QueryFirstOrDefault<(int, decimal, decimal, decimal, decimal)>(sql, new { Inicio = inicio, Fin = fin });
            }
        }

        public void ActualizarObservacionesSesion(int cajaSesionId, string observaciones)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE CajaSesiones SET Observaciones = @Observaciones WHERE Id = @Id", 
                           new { Observaciones = observaciones, Id = cajaSesionId });
            }
        }

        public class ResumenCorteDiaDTO
        {
            public DateTime Fecha { get; set; }
            public int NumeroTurnos { get; set; }
            public decimal FondoTotal { get; set; }
            public decimal SumaEsperada { get; set; }
            public decimal SumaContada { get; set; }
            public decimal Diferencia { get; set; }
        }

        public List<ResumenCorteDiaDTO> ObtenerResumenCortesPorDias(DateTime inicio, DateTime fin)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Agrupar por la fecha de cierre sin hora
                string sql = @"
                    SELECT 
                        DATE(FechaCierre) AS Fecha,
                        COUNT(Id) AS NumeroTurnos,
                        COALESCE(SUM(FondoInicial), 0) AS FondoTotal,
                        COALESCE(SUM(EfectivoEsperado), 0) AS SumaEsperada,
                        COALESCE(SUM(EfectivoContado), 0) AS SumaContada,
                        COALESCE(SUM(Diferencia), 0) AS Diferencia
                    FROM CajaSesiones
                    WHERE FechaCierre BETWEEN @Inicio AND @Fin
                      AND Estado = 'CERRADA'
                    GROUP BY DATE(FechaCierre)
                    ORDER BY DATE(FechaCierre) DESC";

                return db.Query<ResumenCorteDiaDTO>(sql, new { Inicio = inicio, Fin = fin }).ToList();
            }
        }
    }
}
