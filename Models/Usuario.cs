using System;

namespace momospos.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string UsuarioLogin { get; set; }
        public string PasswordHash { get; set; }
        public bool EsAdmin { get; set; }
        public string Estado { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
