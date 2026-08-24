using System.Collections.Generic;
using System.Text;

namespace momospos.Helpers
{
    public class TicketEscPos
    {
        private readonly List<byte> datos = new List<byte>();
        private readonly Encoding encoding;

        public TicketEscPos()
        {
            // En .NET Framework 4.8 CodePage 850 está disponible por defecto, 
            // no es necesario registrar el CodePagesEncodingProvider.
            try
            {
                encoding = Encoding.GetEncoding(850);
            }
            catch
            {
                encoding = Encoding.ASCII;
            }

            Inicializar();
        }

        public void Inicializar()
        {
            // ESC @ - Inicializar impresora
            datos.AddRange(new byte[] { 0x1B, 0x40 });
        }

        public void AlinearIzquierda()
        {
            // ESC a 0
            datos.AddRange(new byte[] { 0x1B, 0x61, 0x00 });
        }

        public void Centrar()
        {
            // ESC a 1
            datos.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
        }

        public void AlinearDerecha()
        {
            // ESC a 2
            datos.AddRange(new byte[] { 0x1B, 0x61, 0x02 });
        }

        public void Negrita(bool activar)
        {
            // ESC E n
            datos.AddRange(new byte[] { 0x1B, 0x45, activar ? (byte)1 : (byte)0 });
        }

        public void DobleTamano(bool activar)
        {
            // GS ! n
            datos.AddRange(new byte[] { 0x1D, 0x21, activar ? (byte)0x11 : (byte)0x00 });
        }

        public void Texto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return;
            datos.AddRange(encoding.GetBytes(texto));
        }

        public void Linea(string texto = "")
        {
            Texto(texto + "\n");
        }

        public void Separador()
        {
            Linea("------------------------------------------");
        }

        public void Logo(string ruta)
        {
            if (!System.IO.File.Exists(ruta)) return;

            Centrar();
            byte[] escPosData = EscPosImage.ImageToEscPos(ruta, 250); // Ajustar max width para que se vea profesional y no gigante
            if (escPosData != null && escPosData.Length > 0)
            {
                datos.AddRange(escPosData);
                Linea();
            }
        }

        public void Avanzar(int lineas = 3)
        {
            for (int i = 0; i < lineas; i++)
                Linea();
        }

        public void CorteCompleto()
        {
            // GS V 0
            datos.AddRange(new byte[] { 0x1D, 0x56, 0x00 });
        }

        public void CorteParcial()
        {
            // GS V 1
            datos.AddRange(new byte[] { 0x1D, 0x56, 0x01 });
        }

        public byte[] ObtenerDatos()
        {
            return datos.ToArray();
        }
    }
}
