namespace momospos.Models
{
    public class Modulo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Clave { get; set; }
        public int? PadreId { get; set; }
        public int Orden { get; set; }
        public string Icono { get; set; }

        // Propiedad de navegación lógica
        public System.Collections.Generic.List<Modulo> Submodulos { get; set; } = new System.Collections.Generic.List<Modulo>();
    }
}
