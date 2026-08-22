using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using Npgsql;

namespace MomosClinic.Repositories
{
    public class MetricasDashboardDTO
    {
        public int ConsultasMesActual { get; set; }
        public int PacientesNuevosMes { get; set; }
        public decimal IngresoEstimadoMes { get; set; }
        public int RecetasEmitidasMes { get; set; }
    }

    public class GraficaMensualDTO
    {
        public int Dia { get; set; }
        public int CantidadConsultas { get; set; }
    }

    public class DashboardClinicRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString ?? "";
        }

        public MetricasDashboardDTO ObtenerMetricasMesActual()
        {
            var metricas = new MetricasDashboardDTO();
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Consultas del mes
                metricas.ConsultasMesActual = db.ExecuteScalar<int>(@"
                    SELECT COUNT(*) FROM clinic.Consultas 
                    WHERE date_trunc('month', CreadoEn) = date_trunc('month', CURRENT_DATE)");

                // Pacientes nuevos del mes
                metricas.PacientesNuevosMes = db.ExecuteScalar<int>(@"
                    SELECT COUNT(*) FROM clinic.Pacientes 
                    WHERE date_trunc('month', CreadoEn) = date_trunc('month', CURRENT_DATE)");

                // Recetas del mes
                metricas.RecetasEmitidasMes = db.ExecuteScalar<int>(@"
                    SELECT COUNT(*) FROM clinic.Recetas 
                    WHERE date_trunc('month', FechaEmision) = date_trunc('month', CURRENT_DATE)");

                // Ingreso estimado basado en Órdenes de Cobro pagadas de este mes
                // (Requiere que MomosPOS las marque como COBRADA)
                // Vamos a usar un enfoque simple: extraer la suma de cantidades * precio en las OrdenesCobro si queremos ser precisos,
                // o si tenemos la tabla VentaDetalles con el origen. Por ahora, extraemos de OrdenesCobro directamente si existe la columna de total.
                // Como JSON no es fácil de sumar en versiones viejas de PG sin sintaxis específica, si no, lo calculamos en C#.
                
                try
                {
                    var jsons = db.Query<string>(@"
                        SELECT JsonDetalles FROM public.OrdenesCobro 
                        WHERE ModuloOrigen = 'MomosClinic' AND Estado = 'COBRADA'
                        AND date_trunc('month', Fecha) = date_trunc('month', CURRENT_DATE)");

                    decimal totalIngreso = 0;
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    foreach (var json in jsons)
                    {
                        var detalles = serializer.Deserialize<List<momospos.Models.VentaDetalle>>(json);
                        if (detalles != null)
                        {
                            foreach (var det in detalles)
                            {
                                totalIngreso += det.Subtotal;
                            }
                        }
                    }
                    metricas.IngresoEstimadoMes = totalIngreso;
                }
                catch
                {
                    metricas.IngresoEstimadoMes = 0;
                }
            }
            return metricas;
        }

        public List<GraficaMensualDTO> ObtenerConsultasPorDiaMesActual()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<GraficaMensualDTO>(@"
                    SELECT EXTRACT(DAY FROM CreadoEn) as Dia, COUNT(*) as CantidadConsultas
                    FROM clinic.Consultas
                    WHERE date_trunc('month', CreadoEn) = date_trunc('month', CURRENT_DATE)
                    GROUP BY EXTRACT(DAY FROM CreadoEn)
                    ORDER BY Dia ASC
                ").AsList();
            }
        }
    }
}
