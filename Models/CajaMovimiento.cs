using System;

namespace momospos.Models
{
    public class CajaMovimiento
    {
        public int Id { get; set; }
        public int CajaSesionId { get; set; }
        public string Tipo { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public string Concepto { get; set; }
        public int UsuarioId { get; set; }
    }
}
