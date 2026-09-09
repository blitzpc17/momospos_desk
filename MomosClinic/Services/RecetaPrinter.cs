using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MomosClinic.Models;
using MomosClinic.Repositories;
using momospos.Repositories;
using momospos.Views;

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
            try
            {
                var config = _configRepo.ObtenerTodas();
                string razonSocial = config.ContainsKey("RazonSocial") ? config["RazonSocial"] : "Nombre o Razón Social";
                string direccion = config.ContainsKey("Direccion") ? config["Direccion"] : "Dirección del consultorio";
                string telefonoClinica = config.ContainsKey("Telefono") ? config["Telefono"] : "";
                string logoBase64 = config.ContainsKey("LogoEmpresa") ? config["LogoEmpresa"] : null;
                
                // Color configuration
                bool aColor = true;
                if (config.ContainsKey("RecetaAColor") && config["RecetaAColor"].ToLower() == "false")
                {
                    aColor = false;
                }

                // Obtener Medico
                string nombreMedico = "Médico Tratante";
                string cedulaProfesional = "S/N";
                if (_consulta != null && _consulta.MedicoId.HasValue)
                {
                    var medicoRepo = new MedicoRepository();
                    var medico = medicoRepo.ObtenerPorId(_consulta.MedicoId.Value);
                    if (medico != null)
                    {
                        nombreMedico = medico.NombreCompleto;
                        cedulaProfesional = medico.CedulaProfesional ?? "S/N";
                        if (!string.IsNullOrEmpty(medico.Telefono)) telefonoClinica = medico.Telefono;
                    }
                }

                string tempPath = Path.Combine(Path.GetTempPath(), $"Receta_{_receta.Folio ?? "Temp"}.pdf");

                // Deteccion tamaño hoja
                bool requiresLetter = _receta.Detalles.Count > 4 || (!string.IsNullOrEmpty(_receta.IndicacionesGenerales) && _receta.IndicacionesGenerales.Length > 250);
                
                Rectangle mediaCarta = new Rectangle(396f, 612f); // 5.5 x 8.5
                Rectangle pageSize = requiresLetter ? PageSize.LETTER : mediaCarta;

                Document doc = new Document(pageSize, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(tempPath, FileMode.Create));

                // Marca de Agua
                if (!string.IsNullOrEmpty(logoBase64))
                {
                    writer.PageEvent = new MarcaDeAgua(logoBase64, aColor);
                }

                doc.Open();

                BaseColor mainColor = aColor ? BaseColor.BLUE : BaseColor.DARK_GRAY;
                BaseColor textColor = BaseColor.BLACK;

                Font fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, textColor);
                Font fSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, mainColor);
                Font fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9, textColor);
                Font fNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, textColor);
                Font fRx = FontFactory.GetFont(FontFactory.HELVETICA, 20, iTextSharp.text.Font.BOLDITALIC, mainColor);

                // Header
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1f, 3f });

                if (!string.IsNullOrEmpty(logoBase64))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(logoBase64);
                        Image logo = Image.GetInstance(imageBytes);
                        
                        // Si no es color y queremos en escala de grises, en iText es complejo
                        // Así que usamos la imagen original tal cual pero la configuracion está aplicada al texto.
                        
                        logo.ScaleAbsolute(70, 70);
                        PdfPCell cellLogo = new PdfPCell(logo);
                        cellLogo.Border = Rectangle.NO_BORDER;
                        cellLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerTable.AddCell(cellLogo);
                    }
                    catch
                    {
                        headerTable.AddCell(CreateCell(" ", fNormal));
                    }
                }
                else
                {
                    headerTable.AddCell(CreateCell(" ", fNormal));
                }

                PdfPCell textCell = new PdfPCell();
                textCell.Border = Rectangle.NO_BORDER;
                textCell.AddElement(new Paragraph(razonSocial, fTitulo));
                textCell.AddElement(new Paragraph($"Dr(a). {nombreMedico}", fSubtitulo));
                textCell.AddElement(new Paragraph($"Cédula Profesional: {cedulaProfesional}", fNegrita));
                textCell.AddElement(new Paragraph(direccion, fNormal));
                textCell.AddElement(new Paragraph($"Tel: {telefonoClinica}", fNormal));
                
                if (!string.IsNullOrEmpty(_receta.Folio))
                    textCell.AddElement(new Paragraph("Folio: " + _receta.Folio, fNegrita));
                
                headerTable.AddCell(textCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" ")); 
                doc.Add(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1));
                doc.Add(new Paragraph(" "));

                // Paciente y Signos
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;

                string pacienteNombre = _paciente != null ? _paciente.NombreCompleto : "Público en General";
                string edad = _paciente != null ? $"{_paciente.Edad} años" : "N/A";
                
                infoTable.AddCell(CreateCell("Datos del Paciente", fNegrita, 2));
                infoTable.AddCell(CreateCell($"Nombre: {pacienteNombre}", fNormal));
                infoTable.AddCell(CreateCell($"Fecha: {_receta.FechaEmision.ToString("dd/MM/yyyy HH:mm")}", fNormal));
                infoTable.AddCell(CreateCell($"Edad: {edad}", fNormal));
                
                string signos = "";
                if (_consulta != null)
                {
                    if (_consulta.Temperatura.HasValue) signos += $"Temp: {_consulta.Temperatura}°C  ";
                    if (!string.IsNullOrEmpty(_consulta.PresionArterial)) signos += $"PA: {_consulta.PresionArterial}  ";
                    if (_consulta.Peso.HasValue) signos += $"Peso: {_consulta.Peso}kg";
                }
                infoTable.AddCell(CreateCell($"Signos Vitales: {signos}", fNormal));
                
                doc.Add(infoTable);
                doc.Add(new Paragraph(" "));
                doc.Add(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -1));
                doc.Add(new Paragraph(" "));

                // Rx
                doc.Add(new Paragraph("Rx", fRx));
                doc.Add(new Paragraph(" "));

                // Medicamentos
                foreach (var det in _receta.Detalles)
                {
                    doc.Add(new Paragraph($"• {det.NombreMedicamento} ({det.Cantidad} pza)", fNegrita));
                    doc.Add(new Paragraph($"   Tomar {det.Dosis} cada {det.Frecuencia} por {det.Duracion}.", fNormal));
                    doc.Add(new Paragraph(" "));
                }

                if (!string.IsNullOrWhiteSpace(_receta.IndicacionesGenerales))
                {
                    doc.Add(new Paragraph("Indicaciones Generales:", fNegrita));
                    doc.Add(new Paragraph(_receta.IndicacionesGenerales, fNormal));
                }

                // Firma
                PdfPTable signTable = new PdfPTable(1);
                signTable.TotalWidth = 200f;
                signTable.LockedWidth = true;
                
                signTable.AddCell(CreateCell(" ", fNormal, 1, Element.ALIGN_CENTER, Rectangle.NO_BORDER));
                signTable.AddCell(CreateCell(" ", fNormal, 1, Element.ALIGN_CENTER, Rectangle.BOTTOM_BORDER)); // Linea
                signTable.AddCell(CreateCell("Firma del Médico", fNormal, 1, Element.ALIGN_CENTER));
                signTable.AddCell(CreateCell($"Dr(a). {nombreMedico}", fSubtitulo, 1, Element.ALIGN_CENTER));

                // Position signature table at absolute bottom
                signTable.WriteSelectedRows(0, -1, (doc.PageSize.Width - signTable.TotalWidth) / 2, doc.BottomMargin + 60, writer.DirectContent);

                doc.Close();

                if (mostrarVistaPrevia)
                {
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al generar la receta PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private PdfPCell CreateCell(string text, Font font, int colspan = 1, int alignment = Element.ALIGN_LEFT, int border = Rectangle.NO_BORDER)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Colspan = colspan;
            cell.HorizontalAlignment = alignment;
            cell.Border = border;
            cell.PaddingBottom = 5f;
            return cell;
        }
    }

    class MarcaDeAgua : PdfPageEventHelper
    {
        private string _base64Logo;
        private bool _color;

        public MarcaDeAgua(string base64Logo, bool color)
        {
            _base64Logo = base64Logo;
            _color = color;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(_base64Logo);
                Image img = Image.GetInstance(imageBytes);
                
                // Si no es color y tuviéramos un helper de grises, aquí se transformaría.
                
                // Aumentar escala y transparencia
                img.ScaleAbsolute(250, 250);
                img.SetAbsolutePosition((document.PageSize.Width - 250) / 2, (document.PageSize.Height - 250) / 2);
                
                PdfGState gstate = new PdfGState();
                gstate.FillOpacity = 0.15f; // Transparente (marca de agua)
                
                PdfContentByte cb = writer.DirectContentUnder;
                cb.SaveState();
                cb.SetGState(gstate);
                cb.AddImage(img);
                cb.RestoreState();
            }
            catch { }
        }
    }
}
