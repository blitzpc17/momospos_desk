using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Views;
using MomosClinic.Services;

namespace MomosClinic.Views.Dialogs
{
    public class RecetaPrintOptionsDialog : Form
    {
        private ComboBox cbTamanoReceta;
        private WebBrowser wbPreview;
        private string currentPdfPath;
        private RecetaPrinter _printer;
        
        public string SelectedSize { get; private set; }
        public string TempPdfPath { get; private set; }

        public RecetaPrintOptionsDialog(string defaultSize, RecetaPrinter printer = null)
        {
            _printer = printer;
            
            this.Text = "Opciones de Impresión de Receta";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            Panel pnlOpciones = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(20) };
            Panel pnlPreview = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            this.Controls.Add(pnlPreview);
            this.Controls.Add(pnlOpciones);

            Label lbl = new Label
            {
                Text = "Formato de hoja:",
                Font = Theme.FontSubtitle,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlOpciones.Controls.Add(lbl);

            cbTamanoReceta = new ComboBox
            {
                Location = new Point(20, 50),
                Width = 260,
                Font = Theme.FontNormal,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbTamanoReceta.Items.AddRange(new string[] { "Automático (Depende contenido)", "Media Carta Horizontal", "Carta Completa" });
            
            if (cbTamanoReceta.Items.Contains(defaultSize))
                cbTamanoReceta.SelectedItem = defaultSize;
            else
                cbTamanoReceta.SelectedIndex = 0;
                
            cbTamanoReceta.SelectedIndexChanged += (s, e) => { GeneratePreview(); };
            pnlOpciones.Controls.Add(cbTamanoReceta);

            Button btnImprimir = new Button
            {
                Text = "Imprimir / Guardar PDF",
                Location = new Point(20, 110),
                Width = 260,
                Height = 40
            };
            Theme.StyleButton(btnImprimir, Theme.PrimaryColor, Color.White, Theme.FontNormalBold);
            btnImprimir.Click += (s, e) =>
            {
                SelectedSize = cbTamanoReceta.SelectedItem.ToString();
                TempPdfPath = currentPdfPath;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            pnlOpciones.Controls.Add(btnImprimir);

            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(20, 160),
                Width = 260,
                Height = 40
            };
            Theme.StyleButton(btnCancelar, Theme.DangerColor, Color.White, Theme.FontNormalBold);
            btnCancelar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            pnlOpciones.Controls.Add(btnCancelar);

            // Preview Browser
            wbPreview = new WebBrowser { Dock = DockStyle.Fill };
            pnlPreview.Controls.Add(wbPreview);
            
            // Generar primer preview
            this.Shown += (s, e) => { GeneratePreview(); };
        }

        private void GeneratePreview()
        {
            if (_printer != null)
            {
                string size = cbTamanoReceta.SelectedItem?.ToString() ?? "Automático (Depende contenido)";
                // Call printer to generate PDF, but DO NOT open it in external app
                currentPdfPath = _printer.Imprimir(overrideSize: size, soloGenerar: true);
                if (currentPdfPath != null)
                {
                    wbPreview.Navigate(new Uri(currentPdfPath));
                }
            }
        }
    }
}
