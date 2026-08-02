using System;

namespace momospos.Models
{
    public class ArticuloVendidoDTO
    {
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public decimal CantidadTotal { get; set; }
        public decimal TotalGenerado { get; set; }
    }
}
