using System;

namespace momospos.Models
{
    public class ArticuloVendidoDTO
    {
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string SustanciaActiva { get; set; }
        public string Categoria { get; set; }
        public decimal CantidadTotal { get; set; }
        public decimal PrecioCompraUnitario { get; set; }
        public decimal PrecioVentaUnitario { get; set; }
        public decimal TotalGenerado { get; set; }
        public decimal Ganancia { get; set; }
    }
}
