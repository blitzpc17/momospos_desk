using System;

namespace MomosClinic.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public string NombrePaciente { get; set; } // Join from Pacientes
        public DateTime FechaHora { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; } // Programada, Confirmada, Completada, Cancelada
        public string Notas { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
