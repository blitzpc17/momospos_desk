using System;

namespace momospos.Models
{
    public class UnidadMedida
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Abreviatura { get; set; }
        public bool PermiteFraccion { get; set; }
    }
}
