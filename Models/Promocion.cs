using System;

namespace momospos.Models
{
    public class Promocion
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } // 'NxM' o 'Porcentaje'
        public decimal CantidadRequerida { get; set; }
        public decimal CantidadRegalo { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public bool AplicaTotalVenta { get; set; }
        public decimal MontoMinimoVenta { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        
        // Joined Properties para la vista
        public string ProductoNombre { get; set; }
        public string ProductoCodigo { get; set; }
    }
}
