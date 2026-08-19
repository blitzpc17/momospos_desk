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

        public void AbrirCajon()
        {
            try
            {
                if (_configs.ContainsKey("AbrirCajon") && _configs["AbrirCajon"] == "True")
                {
                    if (_configs.ContainsKey("ImpresoraTicket") && !string.IsNullOrEmpty(_configs["ImpresoraTicket"]))
                    {
                        momospos.Helpers.RawPrinterHelper.OpenCashDrawer(_configs["ImpresoraTicket"]);
                    }
                }
            }
            catch { }
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

        public void ImprimirComoPdf(string filePath)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = filePath;
            
            pd.PrintPage += Pd_PrintPage;
            
            try
            {
                pd.Print();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al generar PDF: " + ex.Message + "\n(Asegúrese de tener 'Microsoft Print to PDF' habilitado en Windows).", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            bool is80mm = _configs.ContainsKey("TamanoTicket") && _configs["TamanoTicket"] == "80mm";
            
            float fontSize = is80mm ? 9f : 8f;
            Font fontHeader = new Font("Courier New", fontSize + 2f, FontStyle.Bold);
            Font fontNormal = new Font("Courier New", fontSize, FontStyle.Regular);
            Font fontBold = new Font("Courier New", fontSize, FontStyle.Bold);
            
            int yPos = 10;
            int offset = is80mm ? 20 : 15;

            string nombreNegocio = _configs.ContainsKey("NombreNegocio") ? _configs["NombreNegocio"] : "MomosPOS";
            string rfc = _configs.ContainsKey("RFC") ? _configs["RFC"] : "XAXX010101000";
            string direccion = _configs.ContainsKey("Direccion") ? _configs["Direccion"] : "Direccion no configurada";
            string mensaje = _configs.ContainsKey("MensajeTicket") ? _configs["MensajeTicket"] : "¡Gracias por su compra!";
            int maxChars = is80mm ? 48 : 32;
            string divisor = new string('-', maxChars);
            
            // El ancho real de los guiones divisorios.
            int ticketWidth = (int)g.MeasureString(divisor, fontNormal).Width;
            
            // Mantenemos startX en 0. Los drivers genéricos de POS tienen un bug 
            // donde si startX es negativo, envían el primer caracter al final de la línea anterior.
            // Para centrar los textos correctamente, usamos el ticketWidth dinámico.
            int startX = 0;

            // 1. Cabecera
            CentrarTexto(g, nombreNegocio, fontHeader, yPos, ticketWidth, startX); yPos += offset;
            CentrarTexto(g, "RFC: " + rfc, fontNormal, yPos, ticketWidth, startX); yPos += offset;
            CentrarTexto(g, direccion, fontNormal, yPos, ticketWidth, startX); yPos += offset + 10;

            g.DrawString($"Fecha: {_venta.Fecha:dd/MM/yyyy HH:mm:ss}", fontNormal, Brushes.Black, startX, yPos);
            yPos += 15;
            g.DrawString($"Folio: {_venta.Folio}", fontNormal, Brushes.Black, startX, yPos);
            yPos += 20;

            if (!string.IsNullOrEmpty(_venta.MedicoNombre))
            {
                CentrarTexto(g, "=== RECETA MÉDICA ===", fontBold, yPos, ticketWidth, startX); yPos += offset;
                CentrarTexto(g, $"Médico: {_venta.MedicoNombre}", fontNormal, yPos, ticketWidth, startX); yPos += offset;
                CentrarTexto(g, $"Cédula: {_venta.MedicoCedula}", fontNormal, yPos, ticketWidth, startX); yPos += offset;

                if (_venta.RecetaRetenida)
                {
                    CentrarTexto(g, "*** RECETA RETENIDA - ANEXAR AL TICKET ***", fontBold, yPos, ticketWidth, startX); yPos += offset;
                }
                yPos += 10;
            }
            
            g.DrawString(divisor, fontNormal, Brushes.Black, startX, yPos);
            yPos += 15;
            
            int cantLen = is80mm ? 6 : 5;
            int descLen = is80mm ? 29 : 16;
            int subtotalLen = is80mm ? 11 : 9;

            string headerCant = "CANT".PadRight(cantLen);
            string headerDesc = "DESCRIPCION".PadRight(descLen);
            string headerImp = "IMPORTE".PadLeft(subtotalLen);
            
            g.DrawString($"{headerCant} {headerDesc} {headerImp}", fontBold, Brushes.Black, startX, yPos);
            
            yPos += 15;
            g.DrawString(divisor, fontNormal, Brushes.Black, startX, yPos);
            yPos += 20;

            // 2. Detalles
            foreach (var det in _venta.Detalles) { 
                string cant = det.Cantidad.ToString("0.##");
                if (cant.Length > cantLen) cant = cant.Substring(0, cantLen);
                cant = cant.PadRight(cantLen);

                string desc = det.Descripcion;
                if (desc.Length > descLen) desc = desc.Substring(0, descLen);
                desc = desc.PadRight(descLen);

                string subtotal = det.Subtotal.ToString("C");
                if (subtotal.Length > subtotalLen) subtotal = subtotal.Substring(0, subtotalLen);
                subtotal = subtotal.PadLeft(subtotalLen);
                
                string linea = $"{cant} {desc} {subtotal}";
                g.DrawString(linea, fontNormal, Brushes.Black, startX, yPos);
                yPos += 15;
                
                if (det.DescuentoPromo > 0)
                {
                    string promoName = string.IsNullOrEmpty(det.NombrePromo) ? "Promoción" : det.NombrePromo;
                    string ahorromsg = $"Ahorro {promoName}:";
                    if (ahorromsg.Length > (cantLen + descLen - 1)) ahorromsg = ahorromsg.Substring(0, cantLen + descLen - 1);
                    
                    string ahorromsgPadded = ahorromsg.PadLeft(cantLen + descLen + 1);
                    string ahorroSub = "-" + det.DescuentoPromo.ToString("C");
                    ahorroSub = ahorroSub.PadLeft(subtotalLen);
                    
                    string lineaPromo = $"{ahorromsgPadded}{ahorroSub}";
                    g.DrawString(lineaPromo, fontNormal, Brushes.Black, startX, yPos);
                    yPos += 15;
                }
                if (det.DescuentoManual > 0)
                {
                    string ahorromsg = "Cortesía:";
                    if (ahorromsg.Length > (cantLen + descLen - 1)) ahorromsg = ahorromsg.Substring(0, cantLen + descLen - 1);
                    
                    string ahorromsgPadded = ahorromsg.PadLeft(cantLen + descLen + 1);
                    string ahorroSub = "-" + det.DescuentoManual.ToString("C");
                    ahorroSub = ahorroSub.PadLeft(subtotalLen);
                    
                    string lineaPromo = $"{ahorromsgPadded}{ahorroSub}";
                    g.DrawString(lineaPromo, fontNormal, Brushes.Black, startX, yPos);
                    yPos += 15;
                }
            }

            yPos += 5;
            g.DrawString(divisor, fontNormal, Brushes.Black, startX, yPos);
            yPos += 20;

            // 3. Totales
            decimal totalAhorro = _venta.Detalles.Sum(d => d.DescuentoPromo + d.DescuentoManual);
            if (totalAhorro > 0)
            {
                string ahorroStr = $"SU AHORRO: {totalAhorro.ToString("C")}".PadLeft(maxChars);
                g.DrawString(ahorroStr, fontNormal, Brushes.Black, startX, yPos);
                yPos += 15;
            }
            string totalStr = $"TOTAL: {_venta.Total.ToString("C")}".PadLeft(maxChars);
            g.DrawString(totalStr, fontBold, Brushes.Black, startX, yPos);
            yPos += 20;

            string pagadoStr = $"PAGADO: {_venta.Pagado.ToString("C")}".PadLeft(maxChars);
            g.DrawString(pagadoStr, fontNormal, Brushes.Black, startX, yPos);
            yPos += 15;

            string cambioStr = $"CAMBIO: {_venta.Cambio.ToString("C")}".PadLeft(maxChars);
            g.DrawString(cambioStr, fontNormal, Brushes.Black, startX, yPos);
            yPos += 30;

            // 4. Pie de Ticket
            CentrarTexto(g, mensaje, fontNormal, yPos, ticketWidth, startX); yPos += offset;
            CentrarTexto(g, "Le atendió: Cajero ID " + _venta.UsuarioId, fontNormal, yPos, ticketWidth, startX);
        }

        private void CentrarTexto(Graphics g, string texto, Font font, int y, int ticketWidth, int startX)
        {
            SizeF size = g.MeasureString(texto, font);
            float x = startX + (ticketWidth - size.Width) / 2;
            g.DrawString(texto, font, Brushes.Black, x, y);
        }
    }
}
