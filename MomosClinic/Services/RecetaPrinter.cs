using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using MomosClinic.Models;
using momospos.Repositories;
using System.IO;

namespace MomosClinic.Services
{
    public class RecetaPrinter
    {
        private Paciente _paciente;
        private Consulta _consulta;
        private Receta _receta;
        private ConfiguracionRepository _configRepo;

        public RecetaPrinter(Paciente paciente, Consulta consulta, Receta receta)
        {
            _paciente = paciente;
            _consulta = consulta;
            _receta = receta;
            _configRepo = new ConfiguracionRepository();
        }

        public void Imprimir(bool mostrarVistaPrevia = true)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += Pd_PrintPage;

            if (mostrarVistaPrevia)
            {
                PrintPreviewDialog ppd = new PrintPreviewDialog();
                ppd.Document = pd;
                ppd.WindowState = FormWindowState.Maximized;
                ppd.ShowDialog();
            }
            else
            {
                pd.Print();
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int startX = 50;
            int startY = 50;
            int offset = 0;
            int pageWidth = e.PageBounds.Width;

            var config = _configRepo.ObtenerTodas();
            string nombreClinica = config.ContainsKey("NombreNegocio") ? config["NombreNegocio"] : "MomosClinic";
            string direccion = config.ContainsKey("Direccion") ? config["Direccion"] : "Dirección no configurada";
            string logoBase64 = config.ContainsKey("LogoEmpresa") ? config["LogoEmpresa"] : null;

            Font fontTitulo = new Font("Arial", 18, FontStyle.Bold);
            Font fontSubtitulo = new Font("Arial", 12, FontStyle.Regular);
            Font fontNegrita = new Font("Arial", 10, FontStyle.Bold);
            Font fontNormal = new Font("Arial", 10, FontStyle.Regular);

            // Header - Logo
            if (!string.IsNullOrEmpty(logoBase64))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(logoBase64);
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        Image logo = Image.FromStream(ms);
                        g.DrawImage(logo, startX, startY, 100, 100);
                        offset += 120; // Si hay logo, bajamos el texto
                    }
                }
                catch { }
            }

            // Header - Datos Clínica
            int textX = string.IsNullOrEmpty(logoBase64) ? startX : startX + 120;
            g.DrawString(nombreClinica, fontTitulo, Brushes.Black, textX, startY);
            g.DrawString("Receta Médica", fontSubtitulo, Brushes.DarkBlue, textX, startY + 30);
            g.DrawString(direccion, fontNormal, Brushes.Gray, textX, startY + 55);

            offset = Math.Max(offset, 100);
            startY += offset + 20;

            // Línea separadora
            g.DrawLine(Pens.Black, startX, startY, pageWidth - startX, startY);
            startY += 20;

            // Datos del Paciente
            g.DrawString("Datos del Paciente:", fontNegrita, Brushes.Black, startX, startY);
            startY += 25;
            g.DrawString($"Nombre: {_paciente.NombreCompleto}", fontNormal, Brushes.Black, startX, startY);
            g.DrawString($"Fecha: {DateTime.Now.ToString("dd/MM/yyyy")}", fontNormal, Brushes.Black, pageWidth - 250, startY);
            startY += 20;
            g.DrawString($"Edad: {_paciente.Edad} años", fontNormal, Brushes.Black, startX, startY);
            if (_consulta.Temperatura.HasValue)
                g.DrawString($"Temp: {_consulta.Temperatura}°C", fontNormal, Brushes.Black, 200, startY);
            if (!string.IsNullOrEmpty(_consulta.PresionArterial))
                g.DrawString($"PA: {_consulta.PresionArterial}", fontNormal, Brushes.Black, 350, startY);
            if (_consulta.Peso.HasValue)
                g.DrawString($"Peso: {_consulta.Peso}kg", fontNormal, Brushes.Black, 500, startY);

            startY += 30;
            g.DrawLine(Pens.LightGray, startX, startY, pageWidth - startX, startY);
            startY += 20;

            // Rx
            Font fontRx = new Font("Arial", 24, FontStyle.Bold | FontStyle.Italic);
            g.DrawString("Rx", fontRx, Brushes.DarkBlue, startX, startY);
            startY += 40;

            // Medicamentos
            foreach (var det in _receta.Detalles)
            {
                g.DrawString($"• {det.NombreMedicamento} ({det.Cantidad} pza)", fontNegrita, Brushes.Black, startX, startY);
                startY += 20;
                string indicaciones = $"Tomar {det.Dosis} cada {det.Frecuencia} por {det.Duracion}.";
                g.DrawString(indicaciones, fontNormal, Brushes.Black, startX + 20, startY);
                startY += 30;
            }

            // Indicaciones Generales
            if (!string.IsNullOrWhiteSpace(_receta.IndicacionesGenerales))
            {
                startY += 10;
                g.DrawString("Indicaciones Generales:", fontNegrita, Brushes.Black, startX, startY);
                startY += 20;
                var rect = new RectangleF(startX, startY, pageWidth - (startX * 2), 150);
                g.DrawString(_receta.IndicacionesGenerales, fontNormal, Brushes.Black, rect);
                startY += (int)g.MeasureString(_receta.IndicacionesGenerales, fontNormal, (int)rect.Width).Height + 20;
            }

            // Firma Médico
            int bottomY = e.PageBounds.Height - 150;
            g.DrawLine(Pens.Black, pageWidth / 2 - 100, bottomY, pageWidth / 2 + 100, bottomY);
            g.DrawString("Firma del Médico", fontNormal, Brushes.Black, pageWidth / 2 - 50, bottomY + 10);
            g.DrawString($"Cédula: (Configurar en Ajustes)", fontNormal, Brushes.Gray, pageWidth / 2 - 80, bottomY + 30);
        }
    }
}
