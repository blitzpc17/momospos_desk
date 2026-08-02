using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using momospos.Models;
using momospos.Repositories;
using System.Collections.Generic;

namespace momospos.Views
{
    public class TicketPrinter
    {
        private Venta _venta;
        private ConfiguracionRepository _configRepo;
        private Dictionary<string, string> _configs;

        public TicketPrinter(Venta venta)
        {
            _venta = venta;
            _configRepo = new ConfiguracionRepository();
            _configs = _configRepo.ObtenerTodas();
        }

        public void Imprimir()
        {
            PrintDocument pd = new PrintDocument();
            
            if (_configs.ContainsKey("ImpresoraTicket") && !string.IsNullOrEmpty(_configs["ImpresoraTicket"]))
            {
                pd.PrinterSettings.PrinterName = _configs["ImpresoraTicket"];
            }
            
            pd.PrintPage += Pd_PrintPage;
            
            try
            {
                pd.Print();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al imprimir el ticket: " + ex.Message, "Error Impresión", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font fontHeader = new Font("Courier New", 12, FontStyle.Bold);
            Font fontNormal = new Font("Courier New", 9, FontStyle.Regular);
            Font fontBold = new Font("Courier New", 9, FontStyle.Bold);
            
            int startX = 10;
            int yPos = 10;
            int offset = 20;

            string nombreNegocio = _configs.ContainsKey("NombreNegocio") ? _configs["NombreNegocio"] : "MomosPOS";
            string rfc = _configs.ContainsKey("RFC") ? _configs["RFC"] : "XAXX010101000";
            string direccion = _configs.ContainsKey("Direccion") ? _configs["Direccion"] : "Direccion no configurada";
            string mensaje = _configs.ContainsKey("MensajeTicket") ? _configs["MensajeTicket"] : "¡Gracias por su compra!";

            // 1. Cabecera
            CentrarTexto(g, nombreNegocio, fontHeader, yPos); yPos += offset;
            CentrarTexto(g, "RFC: " + rfc, fontNormal, yPos); yPos += offset;
            CentrarTexto(g, direccion, fontNormal, yPos); yPos += offset + 10;

            g.DrawString($"Fecha: {_venta.Fecha:dd/MM/yyyy HH:mm:ss}", fontNormal, Brushes.Black, startX, yPos);
            yPos += 15;
            g.DrawString($"Folio: {_venta.Folio}", fontNormal, Brushes.Black, startX, yPos);
            yPos += 20;
            
            g.DrawString("--------------------------------", fontNormal, Brushes.Black, startX, yPos);
            yPos += 15;
            g.DrawString("CANT DESCRIPCION        IMPORTE", fontBold, Brushes.Black, startX, yPos);
            yPos += 15;
            g.DrawString("--------------------------------", fontNormal, Brushes.Black, startX, yPos);
            yPos += 20;

            // 2. Detalles
            foreach (var det in _venta.Detalles)
            {
                string desc = det.Descripcion.Length > 15 ? det.Descripcion.Substring(0, 15) : det.Descripcion.PadRight(15);
                string cant = det.Cantidad.ToString("0.##").PadRight(4);
                string subtotal = det.Subtotal.ToString("C").PadLeft(10);
                
                string linea = $"{cant} {desc} {subtotal}";
                g.DrawString(linea, fontNormal, Brushes.Black, startX, yPos);
                yPos += 15;
            }

            yPos += 5;
            g.DrawString("--------------------------------", fontNormal, Brushes.Black, startX, yPos);
            yPos += 20;

            // 3. Totales
            g.DrawString($"TOTAL: {_venta.Total.ToString("C").PadLeft(20)}", fontBold, Brushes.Black, startX, yPos);
            yPos += 20;
            g.DrawString($"PAGADO: {_venta.Pagado.ToString("C").PadLeft(19)}", fontNormal, Brushes.Black, startX, yPos);
            yPos += 15;
            g.DrawString($"CAMBIO: {_venta.Cambio.ToString("C").PadLeft(19)}", fontNormal, Brushes.Black, startX, yPos);
            yPos += 30;

            // 4. Pie de Ticket
            CentrarTexto(g, mensaje, fontNormal, yPos); yPos += offset;
            CentrarTexto(g, "Le atendió: Cajero ID " + _venta.UsuarioId, fontNormal, yPos);
        }

        private void CentrarTexto(Graphics g, string texto, Font font, int y)
        {
            SizeF size = g.MeasureString(texto, font);
            float x = (280 - size.Width) / 2;
            g.DrawString(texto, font, Brushes.Black, x, y);
        }
    }
}
