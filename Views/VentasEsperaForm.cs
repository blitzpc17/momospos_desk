using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using System.Linq;

namespace momospos.Views
{
    public class VentasEsperaForm : Form
    {
        private ListBox lstVentasEspera;
        private Button btnRecuperar;
        private Button btnCancelar;
        private Dictionary<string, List<VentaDetalle>> _ventasPausadas;

        public string VentaSeleccionadaId { get; private set; }

        public VentasEsperaForm(Dictionary<string, List<VentaDetalle>> ventasPausadas)
        {
            _ventasPausadas = ventasPausadas;
            BuildUI();
            Theme.SetIcon(this);
            CargarVentas();
        }

        private void BuildUI()
        {
            this.Text = "Recuperar Venta en Espera";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label { Text = "Ventas Pausadas", Font = Theme.FontTitle, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            lstVentasEspera = new ListBox { Location = new Point(20, 60), Width = 440, Height = 250, Font = Theme.FontNormal };
            this.Controls.Add(lstVentasEspera);

            btnRecuperar = new Button { Text = "Recuperar Venta", Location = new Point(20, 330), Width = 200, Height = 40 };
            Theme.StyleButton(btnRecuperar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnRecuperar.Click += BtnRecuperar_Click;
            this.Controls.Add(btnRecuperar);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(240, 330), Width = 220, Height = 40 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancelar);
        }

        private void CargarVentas()
        {
            lstVentasEspera.Items.Clear();
            foreach (var kvp in _ventasPausadas)
            {
                decimal total = kvp.Value.Sum(x => x.Subtotal);
                lstVentasEspera.Items.Add($"{kvp.Key} - {kvp.Value.Count} arts - Total: {total:C}");
            }
        }

        private void BtnRecuperar_Click(object sender, EventArgs e)
        {
            if (lstVentasEspera.SelectedItem != null)
            {
                string text = lstVentasEspera.SelectedItem.ToString();
                VentaSeleccionadaId = text.Split('-')[0].Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Seleccione una venta para recuperar.");
            }
        }
    }
}
