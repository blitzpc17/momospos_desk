using System;

namespace momospos.Models
{
    public class CorteHistorialDTO
    {
        public int SesionId { get; set; }
        public int CajaId { get; set; }
        public string NombreCajero { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal FondoInicial { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal? EfectivoContado { get; set; }
        public decimal? Diferencia { get; set; }
        public string Estado { get; set; }
    }
}
