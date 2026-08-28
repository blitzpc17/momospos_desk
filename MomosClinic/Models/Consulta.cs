using System;

namespace MomosClinic.Models
{
    public class Consulta
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public int? CitaId { get; set; }
        public int PacienteId { get; set; }
        
        // Joined Data
        public string NombrePaciente { get; set; }

        // Signos Vitales
        public decimal? Peso { get; set; } // kg
        public decimal? Talla { get; set; } // m
        public decimal? Temperatura { get; set; } // C
        public string PresionArterial { get; set; }
        public int? FrecuenciaCardiaca { get; set; }
        public int? FrecuenciaRespiratoria { get; set; }
        public int? SaturacionOxigeno { get; set; }
        
        public decimal? IMC 
        {
            get
            {
                if (Peso.HasValue && Talla.HasValue && Talla.Value > 0)
                {
                    return Math.Round(Peso.Value / (Talla.Value * Talla.Value), 2);
                }
                return null;
            }
        }

        // SOAP
        public string MotivoConsulta { get; set; }
        public string ExploracionFisica { get; set; }
        public string Analisis { get; set; }
        public string Diagnostico { get; set; }
        public string PlanTratamiento { get; set; }

        // Finanzas
        public bool CobroGenerado { get; set; }
        public string FolioCobro { get; set; }

        public DateTime CreadoEn { get; set; }
    }
}
