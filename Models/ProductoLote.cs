using System;

namespace momospos.Models
{
    public class ProductoLote
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NumeroLote { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public decimal StockActual { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
