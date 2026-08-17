using System;

namespace momospos.Models
{
    public class MedicamentoControladoDTO
    {
        public string FolioVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public string MedicoNombre { get; set; }
        public string MedicoCedula { get; set; }
        public string ClienteNombre { get; set; }
        
        public string CodigoBarras { get; set; }
        public string NombreProducto { get; set; }
        public string SustanciaActiva { get; set; }
        
        public decimal Cantidad { get; set; }
        public string NumerosLote { get; set; }
    }
}
