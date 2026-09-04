using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;

namespace momospos.Views.Dialogs
{
    public class DesgloseEfectivoForm : Form
    {
        private Label lblTotalCalculado;
        private Button btnAceptar;
        public decimal TotalEfectivo { get; private set; } = 0;

        // Billetes
        private TextBox txtB1000, txtB500, txtB200, txtB100, txtB50, txtB20;
        // Monedas
        private TextBox txtM10, txtM5, txtM2, txtM1, txtM05;

        public DesgloseEfectivoForm()
        {
            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "Desglose de Efectivo";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblHeader = new Label { Text = "🧮 Calculadora de Denominaciones", Font = Theme.FontTitle, ForeColor = Theme.TextLight, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblHeader);

            Panel contentPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            int yBilletes = 20;
            Label lblTitBilletes = new Label { Text = "Billetes", Font = Theme.FontSubtitle, Location = new Point(20, yBilletes), AutoSize = true };
            contentPanel.Controls.Add(lblTitBilletes);

            yBilletes += 40;
            txtB1000 = CrearFilaDenominacion(contentPanel, "$1,000", yBilletes); yBilletes += 35;
            txtB500 = CrearFilaDenominacion(contentPanel, "$500", yBilletes); yBilletes += 35;
            txtB200 = CrearFilaDenominacion(contentPanel, "$200", yBilletes); yBilletes += 35;
            txtB100 = CrearFilaDenominacion(contentPanel, "$100", yBilletes); yBilletes += 35;
            txtB50 = CrearFilaDenominacion(contentPanel, "$50", yBilletes); yBilletes += 35;
            txtB20 = CrearFilaDenominacion(contentPanel, "$20", yBilletes);

            int yMonedas = 20;
            int xCol2 = 250;
            Label lblTitMonedas = new Label { Text = "Monedas", Font = Theme.FontSubtitle, Location = new Point(xCol2, yMonedas), AutoSize = true };
            contentPanel.Controls.Add(lblTitMonedas);

            yMonedas += 40;
            txtM10 = CrearFilaDenominacion(contentPanel, "$10", yMonedas, xCol2); yMonedas += 35;
            txtM5 = CrearFilaDenominacion(contentPanel, "$5", yMonedas, xCol2); yMonedas += 35;
            txtM2 = CrearFilaDenominacion(contentPanel, "$2", yMonedas, xCol2); yMonedas += 35;
            txtM1 = CrearFilaDenominacion(contentPanel, "$1", yMonedas, xCol2); yMonedas += 35;
            txtM05 = CrearFilaDenominacion(contentPanel, "$0.50", yMonedas, xCol2);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 120, Padding = new Padding(20) };
            
            lblTotalCalculado = new Label { Text = "Total: $0.00", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.SuccessColor, AutoSize = true, Location = new Point(20, 10) };
            
            btnAceptar = new Button { Text = "ACEPTAR CONTEO", Location = new Point(20, 60), Width = 440, Height = 45 };
            Theme.StyleButton(btnAceptar, Theme.PrimaryColor, Theme.TextLight, Theme.FontTitle);
            btnAceptar.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            bottomPanel.Controls.Add(lblTotalCalculado);
            bottomPanel.Controls.Add(btnAceptar);

            this.Controls.Add(contentPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private TextBox CrearFilaDenominacion(Panel p, string labelText, int y, int x = 20)
        {
            Label lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 12), Location = new Point(x, y), Width = 70, TextAlign = ContentAlignment.MiddleRight };
            Label lblX = new Label { Text = "x", Font = new Font("Segoe UI", 12), Location = new Point(x + 75, y), Width = 20 };
            TextBox txt = new TextBox { Location = new Point(x + 100, y), Width = 80, Font = new Font("Segoe UI", 12), Text = "0", TextAlign = HorizontalAlignment.Center };
            
            txt.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            txt.TextChanged += Txt_TextChanged;
            txt.Click += (s, e) => { txt.SelectAll(); };
            txt.Enter += (s, e) => { txt.SelectAll(); };

            p.Controls.Add(lbl);
            p.Controls.Add(lblX);
            p.Controls.Add(txt);

            return txt;
        }

        private void Txt_TextChanged(object sender, EventArgs e)
        {
            CalcularTotal();
        }

        private void CalcularTotal()
        {
            decimal total = 0;
            total += ObtenerValor(txtB1000) * 1000m;
            total += ObtenerValor(txtB500) * 500m;
            total += ObtenerValor(txtB200) * 200m;
            total += ObtenerValor(txtB100) * 100m;
            total += ObtenerValor(txtB50) * 50m;
            total += ObtenerValor(txtB20) * 20m;

            total += ObtenerValor(txtM10) * 10m;
            total += ObtenerValor(txtM5) * 5m;
            total += ObtenerValor(txtM2) * 2m;
            total += ObtenerValor(txtM1) * 1m;
            total += ObtenerValor(txtM05) * 0.5m;

            TotalEfectivo = total;
            lblTotalCalculado.Text = $"Total: {TotalEfectivo:C}";
        }

        private int ObtenerValor(TextBox txt)
        {
            if (string.IsNullOrWhiteSpace(txt.Text)) return 0;
            if (int.TryParse(txt.Text, out int result)) return result;
            return 0;
        }
    }
}
