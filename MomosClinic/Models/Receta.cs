using System;
using System.Collections.Generic;

namespace MomosClinic.Models
{
    public class Receta
    {
        public int Id { get; set; }
        public int ConsultaId { get; set; }
        public int PacienteId { get; set; }
        public string IndicacionesGenerales { get; set; }
        public DateTime FechaEmision { get; set; }

        public List<RecetaDetalle> Detalles { get; set; } = new List<RecetaDetalle>();
    }

    public class RecetaDetalle
    {
        public int Id { get; set; }
        public int RecetaId { get; set; }
        public int? ProductoId { get; set; } // Opcional, si usa Farmacia
        public string NombreMedicamento { get; set; }
        public string Dosis { get; set; }
        public string Frecuencia { get; set; }
        public string Duracion { get; set; }
        public int Cantidad { get; set; }
        public string Instrucciones { get; set; }
    }
}
