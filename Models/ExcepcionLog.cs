using System;

namespace momospos.Models
{
    public class ExcepcionLog
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } // Obtenido via JOIN
        public string Modulo { get; set; }
        public string Mensaje { get; set; }
        public string StackTrace { get; set; }
    }
}
