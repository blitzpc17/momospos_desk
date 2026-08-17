namespace momospos.Models
{
    public class VentaDetalleLote
    {
        public int Id { get; set; }
        public int VentaDetalleId { get; set; }
        public int ProductoLoteId { get; set; }
        public decimal Cantidad { get; set; }
    }
}
