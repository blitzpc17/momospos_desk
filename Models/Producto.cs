using System;

namespace momospos.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int? CategoriaId { get; set; }
        public int? UnidadMedidaId { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public bool EsServicio { get; set; }
        public bool PrecioFijo { get; set; }
        public bool Activo { get; set; }
        public bool AplicaCaducidad { get; set; }
        public bool RequiereReceta { get; set; }
        public string SustanciaActiva { get; set; }
        
        public decimal PrecioMayoreo { get; set; }
        public decimal CantidadMayoreo { get; set; }
        public string ClaveProducto { get; set; }
        public string CodigoProveedor { get; set; }
        public string RutaImagen { get; set; }
        
        public DateTime CreadoEn { get; set; }
        
        // Propiedades de navegación para la UI
        public bool PermiteFraccion { get; set; }
        public string UnidadMedidaAbreviatura { get; set; }
    }
}
