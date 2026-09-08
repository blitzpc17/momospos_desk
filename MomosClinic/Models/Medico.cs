using System;

namespace MomosClinic.Models
{
    public class Medico
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        
        public int? EspecialidadId { get; set; }
        public string Especialidad { get; set; } // Obtenido mediante JOIN

        public string Telefono { get; set; }
        public string Correo { get; set; }
        public bool Activo { get; set; }
        
        public string RutaImagen { get; set; }
        
        public DateTime CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}
