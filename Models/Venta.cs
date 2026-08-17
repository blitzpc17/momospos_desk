using System;
using System.Collections.Generic;

namespace momospos.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public int CajaSesionId { get; set; }
        public int? ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public decimal Pagado { get; set; }
        public decimal Cambio { get; set; }
        public string Estado { get; set; }
        public int UsuarioId { get; set; }
        
        // Farmacia / Receta Médica
        public string MedicoNombre { get; set; }
        public string MedicoCedula { get; set; }
        public bool RecetaRetenida { get; set; }
        public string RecetaRutaImagen { get; set; }
        
        public List<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
        public List<VentaPago> Pagos { get; set; } = new List<VentaPago>();
    }
}
