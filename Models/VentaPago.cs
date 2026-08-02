using System;

namespace momospos.Models
{
    public class VentaPago
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string MetodoPago { get; set; } // EFECTIVO, TARJETA, CREDITO
        public decimal Importe { get; set; }
        public DateTime Fecha { get; set; }
    }
}
