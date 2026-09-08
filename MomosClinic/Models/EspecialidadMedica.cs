using System;

namespace MomosClinic.Models
{
    public class EspecialidadMedica
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
