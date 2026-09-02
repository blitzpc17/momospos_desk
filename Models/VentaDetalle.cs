using System;

namespace momospos.Models
{
    public class VentaDetalle
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DescuentoManual { get; set; }
        public string LoteInfo { get; set; }
        public decimal DescuentoPromo { get; set; }
        public string NombrePromo { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
    }
}
