using System;

namespace MomosClinic.Models
{
    public class ProductoSugerido
    {
        public int Id { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadSolicitada { get; set; }
        public int? SolicitadoPor { get; set; }
        public string UsuarioNombre { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public bool Evaluado { get; set; }
    }
}
