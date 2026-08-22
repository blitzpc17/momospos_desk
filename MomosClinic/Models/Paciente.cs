using System;

namespace MomosClinic.Models
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Clave => Id.ToString("D6");
        public string NombreCompleto { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Genero { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Alergias { get; set; }
        public string AntecedentesFamiliares { get; set; }
        public string AntecedentesPatologicos { get; set; }
        public string TipoSangre { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public string CreadoPor { get; set; }
        public string ModificadoPor { get; set; }
        public string MotivoBaja { get; set; }
        public string BajaPor { get; set; }

        public int Edad 
        {
            get 
            {
                if (!FechaNacimiento.HasValue) return 0;
                var today = DateTime.Today;
                var age = today.Year - FechaNacimiento.Value.Year;
                if (FechaNacimiento.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
