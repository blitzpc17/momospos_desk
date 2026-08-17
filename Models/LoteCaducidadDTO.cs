using System;

namespace momospos.Models
{
    public class LoteCaducidadDTO
    {
        public string CodigoBarras { get; set; }
        public string ProductoNombre { get; set; }
        public string NumeroLote { get; set; }
        public decimal StockLote { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public int DiasRestantes { get; set; }
    }
}
