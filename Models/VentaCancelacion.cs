using System;

namespace momospos.Models
{
    public class VentaCancelacion
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string Motivo { get; set; }
        public int UsuarioSolicitaId { get; set; }
        public int? UsuarioAutorizaId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaAutorizacion { get; set; }
        public string Estado { get; set; } // PENDIENTE, APROBADA, RECHAZADA
        
        // Propiedades de navegación para la UI
        public string VentaFolio { get; set; }
        public decimal VentaTotal { get; set; }
        public string NombreSolicitante { get; set; }
        public string NombreAutoriza { get; set; }
    }
}
