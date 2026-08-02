using System;

namespace momospos.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; }
    }
}
