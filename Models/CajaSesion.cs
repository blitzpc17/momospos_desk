using System;

namespace momospos.Models
{
    public class CajaSesion
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public int UsuarioAperturaId { get; set; }
        public int? UsuarioCierreId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal FondoInicial { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal? EfectivoContado { get; set; }
        public decimal? Diferencia { get; set; }
        public string Estado { get; set; } // ABIERTA, CERRADA
        public string Observaciones { get; set; } // Para registrar ajustes
    }
}
