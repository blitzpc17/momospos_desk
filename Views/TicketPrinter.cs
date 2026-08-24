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

        public void CortarPapel()
        {
            try
            {
                if (_configs.ContainsKey("ImpresoraTicket") && !string.IsNullOrEmpty(_configs["ImpresoraTicket"]))
                {
                    momospos.Helpers.RawPrinterHelper.CutPaper(_configs["ImpresoraTicket"]);
                }
            }
            catch { }
        }

        private int CalcularAlturaEstimada()
        {
            int yPos = 10;
            bool is80mm = _configs.ContainsKey("TamanoTicket") && _configs["TamanoTicket"] == "80mm";
            int offset = is80mm ? 20 : 15;
            
            if (_configs.ContainsKey("RutaLogo") && !string.IsNullOrEmpty(_configs["RutaLogo"]) && System.IO.File.Exists(_configs["RutaLogo"]))
            {
                try
                {
                    using (Image logo = Image.FromFile(_configs["RutaLogo"]))
                    {
                        int maxLogoWidth = is80mm ? 200 : 150;
                        int logoWidth = logo.Width;
                        int logoHeight = logo.Height;
                        if (logoWidth > maxLogoWidth)
                        {
                            float scale = (float)maxLogoWidth / logoWidth;
                            logoWidth = maxLogoWidth;
                            logoHeight = (int)(logoHeight * scale);
                        }
                        yPos += logoHeight + 10;
                    }
                }
                catch { }
            }

            yPos += offset * 3 + 10;
            yPos += 15;
            yPos += 20;

            if (!string.IsNullOrEmpty(_venta.MedicoNombre))
            {
                yPos += offset * 3;
                if (_venta.RecetaRetenida) yPos += offset;
                yPos += 10;
            }
            
            yPos += 15;
            yPos += 15;
            yPos += 20;

            foreach (var det in _venta.Detalles)
            {
                yPos += 15;
                if (det.DescuentoPromo > 0) yPos += 15;
                if (det.DescuentoManual > 0) yPos += 15;
            }

            yPos += 25;

            decimal totalAhorro = _venta.Detalles.Sum(d => d.DescuentoPromo + d.DescuentoManual);
            if (totalAhorro > 0) yPos += 15;

            yPos += 20;
            yPos += 15;
            yPos += 30;

            yPos += offset;
            yPos += offset; // For the last text line

            // Add margin for paper cut
            yPos += 80;

            return yPos;
        }

        public void Imprimir()
        {
            if (!_configs.ContainsKey("ImpresoraTicket") || string.IsNullOrEmpty(_configs["ImpresoraTicket"]))
            {
                System.Windows.Forms.MessageBox.Show("No hay impresora configurada para emitir el ticket.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            string impresora = _configs["ImpresoraTicket"];
            momospos.Helpers.TicketEscPos ticket = new momospos.Helpers.TicketEscPos();
            
            bool is80mm = _configs.ContainsKey("TamanoTicket") && _configs["TamanoTicket"] == "80mm";

            // 1. Logo
            if (_configs.ContainsKey("RutaLogoTicket") && !string.IsNullOrEmpty(_configs["RutaLogoTicket"]) && System.IO.File.Exists(_configs["RutaLogoTicket"]))
            {
                ticket.Logo(_configs["RutaLogoTicket"]);
            }

            // 2. Cabecera
            ticket.Centrar();
            ticket.Negrita(true);
            ticket.DobleTamano(true);
            
            string nombreNegocio = _configs.ContainsKey("NombreNegocio") ? _configs["NombreNegocio"] : "MomosPOS";
            ticket.Linea(nombreNegocio);
            
            ticket.DobleTamano(false);
            ticket.Negrita(false);
            
            string rfc = _configs.ContainsKey("RFC") ? _configs["RFC"] : "XAXX010101000";
            string direccion = _configs.ContainsKey("Direccion") ? _configs["Direccion"] : "Direccion no configurada";
            ticket.Linea("RFC: " + rfc);
            ticket.Linea(direccion);
            ticket.Linea();

            // 3. Info Venta
            ticket.AlinearIzquierda();
            ticket.Linea($"Folio:      {_venta.Folio}");
            ticket.Linea($"Fecha:      {_venta.Fecha:dd/MM/yyyy HH:mm:ss}");
            ticket.Linea($"Cajero:     ID {_venta.UsuarioId}");

            if (!string.IsNullOrEmpty(_venta.MedicoNombre))
            {
                ticket.Linea();
                ticket.Centrar();
                ticket.Negrita(true);
                ticket.Linea("=== RECETA MEDICA ===");
                ticket.Negrita(false);
                ticket.Linea($"Medico: {_venta.MedicoNombre}");
                ticket.Linea($"Cedula: {_venta.MedicoCedula}");
                if (_venta.RecetaRetenida)
                {
                    ticket.Negrita(true);
                    ticket.Linea("*** RECETA RETENIDA - ANEXAR ***");
                    ticket.Negrita(false);
                }
            }

            ticket.Separador();
            
            if (is80mm)
                ticket.Linea("CANT  DESCRIPCION                 IMPORTE");
            else
                ticket.Linea("CANT DESCRIPCION        IMPORTE");
                
            ticket.Separador();

            // 4. Detalles
            foreach (var det in _venta.Detalles)
            {
                string cant = det.Cantidad.ToString("0.##");
                string desc = det.Descripcion;
                string subtotal = det.Subtotal.ToString("C");

                if (is80mm)
                {
                    cant = cant.PadRight(5);
                    if (desc.Length > 27) desc = desc.Substring(0, 27);
                    desc = desc.PadRight(27);
                    subtotal = subtotal.PadLeft(9);
                }
                else
                {
                    cant = cant.PadRight(4);
                    if (desc.Length > 16) desc = desc.Substring(0, 16);
                    desc = desc.PadRight(16);
                    subtotal = subtotal.PadLeft(9);
                }

                ticket.Linea($"{cant} {desc} {subtotal}");
                
                if (det.DescuentoPromo > 0)
                {
                    string promoName = string.IsNullOrEmpty(det.NombrePromo) ? "Promo" : det.NombrePromo;
                    if (promoName.Length > 15) promoName = promoName.Substring(0, 15);
                    ticket.Linea($"  Ahorro {promoName}: -{det.DescuentoPromo:C}");
                }
                if (det.DescuentoManual > 0)
                {
                    ticket.Linea($"  Cortesia: -{det.DescuentoManual:C}");
                }
            }

            ticket.Separador();

            // 5. Totales
            ticket.AlinearDerecha();
            
            decimal totalAhorro = _venta.Detalles.Sum(d => d.DescuentoPromo + d.DescuentoManual);
            if (totalAhorro > 0)
            {
                ticket.Linea($"SU AHORRO: {totalAhorro:C}");
            }

            ticket.Negrita(true);
            ticket.DobleTamano(true);
            ticket.Linea($"TOTAL: {_venta.Total:C}");
            ticket.DobleTamano(false);
            ticket.Negrita(false);

            ticket.Linea($"PAGADO: {_venta.Pagado:C}");
            ticket.Linea($"CAMBIO: {_venta.Cambio:C}");
            ticket.Linea();

            // 6. Pie
            ticket.Centrar();
            string mensaje = _configs.ContainsKey("MensajeTicket") ? _configs["MensajeTicket"] : "¡Gracias por su compra!";
            ticket.Linea(mensaje);

            ticket.Avanzar(4);
            ticket.CorteCompleto();

            bool resultado = momospos.Helpers.RawPrinterHelper.SendBytesToPrinter(impresora, ticket.ObtenerDatos());
            if (!resultado)
            {
                System.Windows.Forms.MessageBox.Show("Error enviando datos RAW a la impresora.", "Error Impresión", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public void ImprimirComoPdf(string filePath)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = filePath;
            
            bool is80mm = _configs.ContainsKey("TamanoTicket") && _configs["TamanoTicket"] == "80mm";
            int paperWidth = is80mm ? 314 : 228;
            pd.DefaultPageSettings.PaperSize = new PaperSize("Custom", paperWidth, CalcularAlturaEstimada());

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
            if (_configs.ContainsKey("RutaLogo") && !string.IsNullOrEmpty(_configs["RutaLogo"]))
            {
                string rutaLogo = _configs["RutaLogo"];
                if (System.IO.File.Exists(rutaLogo))
                {
                    try
                    {
                        // Evitamos bloqueos de archivo leyendo los bytes primero, y manteniendo el MemoryStream vivo
                        byte[] imgBytes = System.IO.File.ReadAllBytes(rutaLogo);
                        using (var ms = new System.IO.MemoryStream(imgBytes))
                        using (Image logo = Image.FromStream(ms))
                        {
                            int maxLogoWidth = is80mm ? 200 : 150;
                            int logoWidth = logo.Width;
                            int logoHeight = logo.Height;
                            if (logoWidth > maxLogoWidth)
                            {
                                float scale = (float)maxLogoWidth / logoWidth;
                                logoWidth = maxLogoWidth;
                                logoHeight = (int)(logoHeight * scale);
                            }
                            int logoX = startX + (ticketWidth - logoWidth) / 2;
                            
                            Bitmap bmp1bpp = new Bitmap(logoWidth, logoHeight, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                            System.Drawing.Imaging.BitmapData destData = bmp1bpp.LockBits(new Rectangle(0, 0, logoWidth, logoHeight), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                            
                            byte[] buffer = new byte[destData.Stride * logoHeight];
                            for (int i = 0; i < buffer.Length; i++) buffer[i] = 255; 
                            
                            using (Bitmap resizedLogo = new Bitmap(logo, logoWidth, logoHeight))
                            {
                                for (int y = 0; y < logoHeight; y++)
                                {
                                    for (int x = 0; x < logoWidth; x++)
                                    {
                                        Color c = resizedLogo.GetPixel(x, y);
                                        int rgb = (c.R + c.G + c.B) / 3;
                                        bool isBlack = c.A >= 128 && rgb < 200;
                                        
                                        if (isBlack)
                                        {
                                            int index = (y * destData.Stride) + (x / 8);
                                            buffer[index] &= (byte)~(0x80 >> (x % 8)); 
                                        }
                                    }
                                }
                            }
                            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, destData.Scan0, buffer.Length);
                            bmp1bpp.UnlockBits(destData);
                            
                            g.DrawImage(bmp1bpp, new Rectangle(logoX, yPos, logoWidth, logoHeight));
                            bmp1bpp.Dispose();
                            
                            yPos += logoHeight + 10;
                        }
                    }
                    catch (Exception ex)
                    {
                        g.DrawString("Err Logo Carga: " + ex.Message, fontNormal, Brushes.Black, startX, yPos);
                        yPos += 15;
                    }
                }
                else
                {
                    g.DrawString("Err Logo: Archivo no encontrado en:", fontNormal, Brushes.Black, startX, yPos); yPos += 15;
                    g.DrawString(rutaLogo, new Font("Courier New", 6f, FontStyle.Regular), Brushes.Black, startX, yPos); yPos += 15;
                }
            }
            else
            {
                // Solo para debug si no hay configuración
                // g.DrawString("RutaLogo no configurada", fontNormal, Brushes.Black, startX, yPos); yPos += 15;
            }

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
