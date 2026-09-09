using System;

namespace MomosClinic.Models
{
    public class MotivoCancelacionCita
    {
        public int Id { get; set; }
        public string Motivo { get; set; }
        public bool Activo { get; set; }
    }
}
