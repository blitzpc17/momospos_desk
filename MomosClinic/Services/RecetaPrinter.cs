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

        public string Imprimir(string overrideSize = null, bool soloGenerar = false)
        {
            try
            {
                var config = _configRepo.ObtenerTodas();
                string razonSocial = config.ContainsKey("RazonSocial") ? config["RazonSocial"] : "Nombre o Razón Social";
                string direccion = config.ContainsKey("Direccion") ? config["Direccion"] : "Dirección del consultorio";
                string telefonoClinica = config.ContainsKey("Telefono") ? config["Telefono"] : "";
                string logoBase64 = config.ContainsKey("RecetaLogoBase64") ? config["RecetaLogoBase64"] : (config.ContainsKey("LogoEmpresa") ? config["LogoEmpresa"] : null);
                string marcaAguaBase64 = config.ContainsKey("RecetaMarcaAguaBase64") ? config["RecetaMarcaAguaBase64"] : null;

                // Color configuration
                bool aColor = true;
                if (config.ContainsKey("RecetaAColor") && config["RecetaAColor"].ToLower() == "false")
                {
                    aColor = false;
                }

                // Obtener Medico
                string nombreMedico = "Médico Tratante";
                string cedulaProfesional = "S/N";
                string especialidad = "Médico General";
                
                int? medicoIdResolver = _consulta?.MedicoId;
                if (!medicoIdResolver.HasValue)
                {
                    string defaultMed = _configRepo.ObtenerValor("MedicoPorDefectoId");
                    if (!string.IsNullOrEmpty(defaultMed) && int.TryParse(defaultMed, out int medId))
                    {
                        medicoIdResolver = medId;
                    }
                }

                if (medicoIdResolver.HasValue)
                {
                    var medicoRepo = new MedicoRepository();
                    var medico = medicoRepo.ObtenerPorId(medicoIdResolver.Value);
                    if (medico != null)
                    {
                        nombreMedico = medico.NombreCompleto;
                        cedulaProfesional = medico.CedulaProfesional ?? "S/N";
                        if (!string.IsNullOrEmpty(medico.Especialidad)) especialidad = medico.Especialidad;
                        if (!string.IsNullOrEmpty(medico.Telefono)) telefonoClinica = medico.Telefono;
                    }
                }

                string tempPath = Path.Combine(Path.GetTempPath(), $"Receta_{_receta.Folio ?? "Temp"}.pdf");

                // Deteccion tamaño hoja
                string tamanoConf = overrideSize ?? _configRepo.ObtenerValor("TamanoReceta");
                Rectangle pageSize = PageSize.LETTER;

                if (tamanoConf == "Media Carta Horizontal")
                {
                    pageSize = new Rectangle(612f, 396f); // 8.5 x 5.5 
                }
                else if (tamanoConf == "Carta Completa")
                {
                    pageSize = PageSize.LETTER;
                }
                else
                {
                    bool requiresLetter = _receta.Detalles.Count > 4 || (!string.IsNullOrEmpty(_receta.IndicacionesGenerales) && _receta.IndicacionesGenerales.Length > 250);
                    pageSize = requiresLetter ? PageSize.LETTER : new Rectangle(612f, 396f);
                }

                Document doc = new Document(pageSize, 20, 20, 20, 20); 
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(tempPath, FileMode.Create));

                // Marca de Agua
                if (!string.IsNullOrEmpty(marcaAguaBase64))
                {
                    writer.PageEvent = new MarcaDeAgua(marcaAguaBase64, aColor);
                }
                else if (!string.IsNullOrEmpty(logoBase64))
                {
                    writer.PageEvent = new MarcaDeAgua(logoBase64, aColor);
                }

                doc.Open();

                BaseColor mainColor = aColor ? BaseColor.BLUE : BaseColor.DARK_GRAY;
                if (aColor && config.ContainsKey("RecetaColorBase"))
                {
                    try
                    {
                        var col = System.Drawing.ColorTranslator.FromHtml(config["RecetaColorBase"]);
                        mainColor = new BaseColor(col.R, col.G, col.B);
                    }
                    catch { }
                }
                BaseColor textColor = BaseColor.BLACK;

                Font fTitBase = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, mainColor);
                Font fSubBase = FontFactory.GetFont(FontFactory.HELVETICA, 10, mainColor);
                Font fBoldBase = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, mainColor);
                Font fNormalBase = FontFactory.GetFont(FontFactory.HELVETICA, 9, mainColor);
                
                Font fLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, mainColor);
                Font fVal = FontFactory.GetFont(FontFactory.HELVETICA, 9, textColor);
                Font fText = FontFactory.GetFont(FontFactory.HELVETICA, 9, textColor);
                Font fTextBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, textColor);
                
                Font fRx = FontFactory.GetFont(FontFactory.HELVETICA, 16, iTextSharp.text.Font.BOLDITALIC, mainColor);

                // --- HEADER ---
                PdfPTable headerTable = new PdfPTable(3);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1.5f, 3f, 2.5f });

                // Celda 1: Logo
                if (!string.IsNullOrEmpty(logoBase64))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(logoBase64);
                        Image logo = Image.GetInstance(imageBytes);
                        logo.ScaleToFit(80, 80);
                        PdfPCell cellLogo = new PdfPCell(logo);
                        cellLogo.Border = Rectangle.NO_BORDER;
                        cellLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                        cellLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerTable.AddCell(cellLogo);
                    }
                    catch
                    {
                        headerTable.AddCell(CreateCell(" ", fNormalBase));
                    }
                }
                else
                {
                    headerTable.AddCell(CreateCell(" ", fNormalBase));
                }

                // Celda 2: Nombre del Medico y Especialidad
                PdfPCell centerCell = new PdfPCell();
                centerCell.Border = Rectangle.NO_BORDER;
                centerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                centerCell.HorizontalAlignment = Element.ALIGN_LEFT;
                centerCell.PaddingLeft = 10f;
                Paragraph pTit = new Paragraph($"Dr(a). {nombreMedico}", fTitBase);
                pTit.SpacingAfter = 2f;
                centerCell.AddElement(pTit);
                Paragraph pSpec = new Paragraph(especialidad, fSubBase);
                centerCell.AddElement(pSpec);
                headerTable.AddCell(centerCell);

                // Celda 3: Detalles Egresado, Cedula
                PdfPCell rightCell = new PdfPCell();
                rightCell.Border = Rectangle.LEFT_BORDER;
                rightCell.BorderColorLeft = mainColor;
                rightCell.BorderWidthLeft = 1f;
                rightCell.PaddingLeft = 10f;
                rightCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                rightCell.HorizontalAlignment = Element.ALIGN_LEFT;
                rightCell.AddElement(new Paragraph($"Ced. Profesional: {cedulaProfesional}", fBoldBase));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                
                // Linea separadora encabezado
                doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));
                doc.Add(new iTextSharp.text.pdf.draw.LineSeparator(1.5f, 100f, mainColor, Element.ALIGN_CENTER, -1));
                doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));

                // --- DATOS PACIENTE ---
                string pacienteNombre = _paciente != null ? _paciente.NombreCompleto : "Público en General";
                string edad = _paciente != null ? $"{_paciente.Edad} años" : "";
                
                Paragraph pPat1 = new Paragraph();
                pPat1.Add(new Chunk("Paciente: ", fLabel));
                pPat1.Add(new Chunk(pacienteNombre.PadRight(40, '_'), fVal));
                pPat1.Add(new Chunk("   Edad: ", fLabel));
                pPat1.Add(new Chunk(edad.PadRight(15, '_'), fVal));
                pPat1.Add(new Chunk("   Fecha: ", fLabel));
                pPat1.Add(new Chunk(_receta.FechaEmision.ToString("dd/MM/yyyy").PadRight(15, '_'), fVal));
                if (!string.IsNullOrEmpty(_receta.Folio))
                {
                    pPat1.Add(new Chunk("   Folio: ", fLabel));
                    pPat1.Add(new Chunk(_receta.Folio, fVal));
                }
                doc.Add(pPat1);

                Paragraph pPat2 = new Paragraph();
                string peso = _consulta?.Peso != null ? $"{_consulta.Peso} kg" : "";
                string talla = _consulta?.Talla != null ? $"{_consulta.Talla} m" : "";
                string ta = _consulta?.PresionArterial ?? "";
                string fa = _consulta?.FrecuenciaCardiaca != null ? $"{_consulta.FrecuenciaCardiaca} bpm" : "";
                string temp = _consulta?.Temperatura != null ? $"{_consulta.Temperatura} °C" : "";
                
                pPat2.Add(new Chunk("Peso: ", fLabel));
                pPat2.Add(new Chunk(peso.PadRight(10, '_'), fVal));
                pPat2.Add(new Chunk("   Talla: ", fLabel));
                pPat2.Add(new Chunk(talla.PadRight(10, '_'), fVal));
                pPat2.Add(new Chunk("   TA: ", fLabel));
                pPat2.Add(new Chunk(ta.PadRight(10, '_'), fVal));
                pPat2.Add(new Chunk("   FA: ", fLabel));
                pPat2.Add(new Chunk(fa.PadRight(10, '_'), fVal));
                pPat2.Add(new Chunk("   Temperatura: ", fLabel));
                pPat2.Add(new Chunk(temp.PadRight(10, '_'), fVal));
                doc.Add(pPat2);

                string idx = _consulta?.Diagnostico ?? "";
                Paragraph pPat3 = new Paragraph();
                pPat3.Add(new Chunk("IDX: ", fLabel));
                pPat3.Add(new Chunk(idx, fVal));
                doc.Add(pPat3);

                // Linea separadora paciente
                doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));
                doc.Add(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, mainColor, Element.ALIGN_CENTER, -1));
                doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));

                // --- CONTENIDO (Rx) ---
                doc.Add(new Paragraph("Rx", fRx));
                doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));

                foreach (var det in _receta.Detalles)
                {
                    doc.Add(new Paragraph($"• {det.NombreMedicamento} ({det.Cantidad} pza)", fTextBold));
                    doc.Add(new Paragraph($"   Tomar {det.Dosis} cada {det.Frecuencia} por {det.Duracion}.", fText));
                    doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));
                }

                if (!string.IsNullOrWhiteSpace(_receta.IndicacionesGenerales))
                {
                    doc.Add(new Paragraph("Indicaciones Generales:", fTextBold));
                    doc.Add(new Paragraph(_receta.IndicacionesGenerales, fText));
                }

                // --- FIRMA Y FOOTER ---
                PdfPTable footerTable = new PdfPTable(2);
                footerTable.TotalWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
                footerTable.LockedWidth = true;
                footerTable.SetWidths(new float[] { 1f, 1f });

                // Footer Bar (Top line separator for footer)
                PdfPCell footLineCell = new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 4)));
                footLineCell.Colspan = 2;
                footLineCell.Border = Rectangle.BOTTOM_BORDER;
                footLineCell.BorderColorBottom = mainColor;
                footLineCell.BorderWidthBottom = 1.5f;
                footerTable.AddCell(footLineCell);

                PdfPCell footLeft = new PdfPCell();
                footLeft.Border = Rectangle.NO_BORDER;
                footLeft.PaddingTop = 5f;
                footLeft.VerticalAlignment = Element.ALIGN_BOTTOM;
                footLeft.AddElement(new Paragraph(direccion, fNormalBase));
                footLeft.AddElement(new Paragraph($"Tel: {telefonoClinica}", fNormalBase));
                footerTable.AddCell(footLeft);

                PdfPCell footRight = new PdfPCell();
                footRight.Border = Rectangle.NO_BORDER;
                footRight.PaddingTop = 5f;
                footRight.HorizontalAlignment = Element.ALIGN_RIGHT;
                
                // Signature inside footer right
                PdfPTable signTable = new PdfPTable(1);
                signTable.TotalWidth = 200f;
                signTable.LockedWidth = true;
                signTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                PdfPCell signLine = CreateCell(" ", fText, 1, Element.ALIGN_CENTER, Rectangle.BOTTOM_BORDER);
                signLine.BorderColorBottom = mainColor;
                signLine.BorderWidthBottom = 1f;
                signTable.AddCell(signLine);
                signTable.AddCell(CreateCell($"Dr(a). {nombreMedico}", fTextBold, 1, Element.ALIGN_CENTER, Rectangle.NO_BORDER));
                footRight.AddElement(signTable);

                Paragraph pr2 = new Paragraph($"C.P.: {cedulaProfesional}", fBoldBase); 
                pr2.Alignment = Element.ALIGN_RIGHT;
                footRight.AddElement(pr2);
                footerTable.AddCell(footRight);

                footerTable.WriteSelectedRows(0, -1, doc.LeftMargin, doc.BottomMargin + 40, writer.DirectContent);

                doc.Close();

                if (!soloGenerar)
                {
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                }
                
                return tempPath;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al generar la receta PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
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
