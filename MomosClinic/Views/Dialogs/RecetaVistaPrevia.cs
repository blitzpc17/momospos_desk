using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using MomosClinic.Models;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class RecetaVistaPrevia : Form
    {
        private readonly Receta _receta;
        private readonly string _nombrePaciente;
        private readonly string _diagnostico;
        private Panel _previewPanel;

        public RecetaVistaPrevia(Receta receta, string nombrePaciente, string diagnostico = "")
        {
            _receta = receta;
            _nombrePaciente = nombrePaciente;
            _diagnostico = diagnostico;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Vista Previa de Receta";
            this.Size = new Size(700, 820);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.MinimumSize = new Size(600, 600);

            // --- Top Bar ---
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Theme.PrimaryColor };
            Label lblTitle = new Label
            {
                Text = "\U0001F4CB Vista Previa de Receta M\u00e9dica",
                Font = Theme.FontTitle,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 13)
            };
            topBar.Controls.Add(lblTitle);
            this.Controls.Add(topBar);

            // --- Button bar ---
            Panel btnBar = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            Panel divider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(220, 225, 235) };
            btnBar.Controls.Add(divider);

            Button btnImprimir = new Button
            {
                Text = "\U0001F5A8 Imprimir",
                Location = new Point(20, 13),
                Width = 130,
                Height = 36
            };
            Theme.StyleButton(btnImprimir, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnImprimir.Click += BtnImprimir_Click;

            Button btnCerrar = new Button
            {
                Text = "\u274C Cerrar",
                Location = new Point(165, 13),
                Width = 110,
                Height = 36
            };
            Theme.StyleButton(btnCerrar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCerrar.Click += (s, e) => this.Close();

            btnBar.Controls.Add(btnImprimir);
            btnBar.Controls.Add(btnCerrar);
            this.Controls.Add(btnBar);

            // --- Scroll container ---
            Panel scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30, 20, 30, 20),
                BackColor = Color.FromArgb(240, 243, 248)
            };
            this.Controls.Add(scrollContainer);

            // --- Recipe Card ---
            _previewPanel = BuildRecetaCard();
            scrollContainer.Controls.Add(_previewPanel);

            // Z-order fix
            topBar.BringToFront();
            scrollContainer.BringToFront();
            btnBar.SendToBack();
        }

        private Panel BuildRecetaCard()
        {
            Panel card = new Panel
            {
                BackColor = Color.White,
                Width = 600,
                Location = new Point(20, 20),
                AutoSize = true,
                Padding = new Padding(35, 30, 35, 30),
                BorderStyle = BorderStyle.None
            };

            // Give card a rounded-ish look via paint
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var pen = new Pen(Color.FromArgb(210, 218, 235), 1))
                    g.DrawRectangle(pen, rect);
            };

            int y = 30;
            int leftX = 35;
            int cardWidth = 530;

            // ── HEADER ──
            Panel headerStripe = new Panel
            {
                BackColor = Theme.PrimaryColor,
                Location = new Point(0, 0),
                Width = card.Width,
                Height = 8
            };
            card.Controls.Add(headerStripe);

            // Clinic name
            Label lblClinic = new Label
            {
                Text = "CLINICA MOMOS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(leftX, y)
            };
            card.Controls.Add(lblClinic);
            y += 32;

            Label lblSubClinic = new Label
            {
                Text = "Receta M\u00e9dica Oficial",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(leftX, y)
            };
            card.Controls.Add(lblSubClinic);

            // Date (top right)
            Label lblFecha = new Label
            {
                Text = "Fecha: " + (_receta.FechaEmision == default ? DateTime.Now : _receta.FechaEmision).ToString("dd/MM/yyyy"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(leftX + cardWidth - 160, y)
            };
            card.Controls.Add(lblFecha);
            y += 40;

            // Divider
            y = AddDivider(card, y, leftX, cardWidth);

            // ── PATIENT INFO ──
            y = AddSectionHeader(card, "\U0001F464 Datos del Paciente", y, leftX);
            y = AddInfoRow(card, "Paciente:", _nombrePaciente, y, leftX);

            if (!string.IsNullOrWhiteSpace(_diagnostico))
                y = AddInfoRow(card, "Diagn\u00f3stico:", _diagnostico, y, leftX);

            y += 15;
            y = AddDivider(card, y, leftX, cardWidth);

            // ── MEDICATIONS ──
            y = AddSectionHeader(card, "\U0001F48A Medicamentos Prescritos", y, leftX);

            if (_receta.Detalles == null || _receta.Detalles.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "  (Sin medicamentos agregados)",
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(leftX, y)
                };
                card.Controls.Add(lblEmpty);
                y += 28;
            }
            else
            {
                int num = 1;
                foreach (var detalle in _receta.Detalles)
                {
                    // Medicine number badge
                    Panel badgePanel = new Panel
                    {
                        BackColor = Color.FromArgb(235, 243, 255),
                        Location = new Point(leftX, y),
                        Width = cardWidth,
                        Height = 80,
                        Padding = new Padding(12, 10, 12, 10)
                    };

                    Label lblNum = new Label
                    {
                        Text = num.ToString("00"),
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = Theme.PrimaryColor,
                        AutoSize = true,
                        Location = new Point(10, 22)
                    };
                    badgePanel.Controls.Add(lblNum);

                    Label lblMedName = new Label
                    {
                        Text = detalle.NombreMedicamento ?? "",
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.FromArgb(30, 40, 60),
                        AutoSize = true,
                        Location = new Point(45, 8)
                    };
                    badgePanel.Controls.Add(lblMedName);

                    string details = BuildMedDetails(detalle);
                    Label lblMedDetail = new Label
                    {
                        Text = details,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(80, 90, 110),
                        AutoSize = true,
                        Location = new Point(45, 32)
                    };
                    badgePanel.Controls.Add(lblMedDetail);

                    // Qty badge
                    Label lblQty = new Label
                    {
                        Text = "x" + detalle.Cantidad,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Theme.PrimaryColor,
                        AutoSize = true,
                        Location = new Point(cardWidth - 50, 28)
                    };
                    badgePanel.Controls.Add(lblQty);

                    card.Controls.Add(badgePanel);
                    y += 90;
                    num++;
                }
            }

            y += 10;
            y = AddDivider(card, y, leftX, cardWidth);

            // ── GENERAL INSTRUCTIONS ──
            y = AddSectionHeader(card, "\U0001F4DD Indicaciones Generales", y, leftX);

            string indText = string.IsNullOrWhiteSpace(_receta.IndicacionesGenerales)
                ? "Sin indicaciones adicionales."
                : _receta.IndicacionesGenerales;

            Label lblIndicaciones = new Label
            {
                Text = indText,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(60, 70, 90),
                Location = new Point(leftX, y),
                Width = cardWidth,
                AutoSize = true
            };
            card.Controls.Add(lblIndicaciones);
            y += lblIndicaciones.PreferredHeight + 20;

            y = AddDivider(card, y, leftX, cardWidth);

            // ── SIGNATURE AREA ──
            y += 15;
            Panel sigArea = new Panel
            {
                Location = new Point(leftX + (cardWidth / 2) - 10, y),
                Width = cardWidth / 2,
                Height = 70
            };

            Panel sigLine = new Panel
            {
                Location = new Point(15, 0),
                Width = sigArea.Width - 30,
                Height = 1,
                BackColor = Color.FromArgb(180, 190, 210)
            };
            sigArea.Controls.Add(sigLine);

            Label lblSig = new Label
            {
                Text = "Firma del M\u00e9dico",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(50, 8)
            };
            sigArea.Controls.Add(lblSig);
            card.Controls.Add(sigArea);
            y += 90;

            // Footer stripe
            Panel footerStripe = new Panel
            {
                BackColor = Theme.PrimaryColor,
                Location = new Point(0, y),
                Width = card.Width,
                Height = 6
            };
            card.Controls.Add(footerStripe);
            y += 20;

            card.Height = y + 10;
            return card;
        }

        private string BuildMedDetails(RecetaDetalle d)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(d.Dosis)) parts.Add("Dosis: " + d.Dosis);
            if (!string.IsNullOrWhiteSpace(d.Frecuencia)) parts.Add("Frecuencia: " + d.Frecuencia);
            if (!string.IsNullOrWhiteSpace(d.Duracion)) parts.Add("Duraci\u00f3n: " + d.Duracion);
            return parts.Count > 0 ? string.Join("  |  ", parts) : "Sin especificaciones";
        }

        private int AddSectionHeader(Panel card, string text, int y, int leftX)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(leftX, y)
            };
            card.Controls.Add(lbl);
            return y + 32;
        }

        private int AddInfoRow(Panel card, string label, string value, int y, int leftX)
        {
            Label lblKey = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 90, 110),
                AutoSize = true,
                Location = new Point(leftX, y)
            };
            Label lblVal = new Label
            {
                Text = value ?? "-",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(30, 40, 60),
                AutoSize = true,
                Location = new Point(leftX + 120, y)
            };
            card.Controls.Add(lblKey);
            card.Controls.Add(lblVal);
            return y + 26;
        }

        private int AddDivider(Panel card, int y, int leftX, int width)
        {
            Panel div = new Panel
            {
                BackColor = Color.FromArgb(225, 230, 242),
                Location = new Point(leftX, y),
                Width = width,
                Height = 1
            };
            card.Controls.Add(div);
            return y + 18;
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (pSender, pe) =>
            {
                // Render the preview panel as a bitmap and print it
                Bitmap bmp = new Bitmap(_previewPanel.Width, _previewPanel.Height);
                _previewPanel.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

                float scale = Math.Min(
                    (float)pe.MarginBounds.Width / bmp.Width,
                    (float)pe.MarginBounds.Height / bmp.Height);

                int drawW = (int)(bmp.Width * scale);
                int drawH = (int)(bmp.Height * scale);
                int drawX = pe.MarginBounds.X + (pe.MarginBounds.Width - drawW) / 2;
                int drawY = pe.MarginBounds.Y;

                pe.Graphics.DrawImage(bmp, drawX, drawY, drawW, drawH);
            };

            PrintPreviewDialog ppd = new PrintPreviewDialog
            {
                Document = pd,
                WindowState = FormWindowState.Maximized
            };
            ppd.ShowDialog();
        }
    }
}
