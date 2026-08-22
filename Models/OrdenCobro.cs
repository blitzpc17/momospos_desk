using System;

namespace momospos.Models
{
    public class OrdenCobro
    {
        public int Id { get; set; }
        public string Referencia { get; set; }
        public string ModuloOrigen { get; set; } // 'MomosPOS', 'MomosClinic'
        public string Estado { get; set; } // PENDIENTE, COBRADA, CANCELADA
        public string JsonDetalles { get; set; }
        public DateTime Fecha { get; set; }
    }
}
