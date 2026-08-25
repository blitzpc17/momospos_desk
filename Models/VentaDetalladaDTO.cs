using System;

namespace momospos.Models
{
    public class VentaDetalladaDTO
    {
        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string UnidadMedida { get; set; }
        public string Servicio { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal TotalCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal TotalVenta { get; set; }
    }
}
