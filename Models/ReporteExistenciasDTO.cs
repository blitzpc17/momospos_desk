using System;

namespace momospos.Models
{
    public class ReporteExistenciasDTO
    {
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal CostoInvertido { get; set; }
        public decimal GananciaProyectada { get; set; }
        public string Estado { get; set; }
    }
}
