using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using System.Linq;

namespace momospos.Views
{
    public class VentasEsperaForm : Form
    {
        private ListBox lstVentasEspera;
        private Button btnRecuperar;
        private Button btnCancelar;
        private OrdenesCobroRepository _repo;
        private List<OrdenCobro> _pendientes;

        public int OrdenSeleccionadaId { get; private set; }

        public VentasEsperaForm()
        {
            _repo = new OrdenesCobroRepository();
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

            Label lblTitulo = new Label { Text = "Órdenes y Ventas Pausadas", Font = Theme.FontTitle, Location = new Point(20, 20), AutoSize = true };
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
            _pendientes = _repo.ObtenerPendientes().ToList();

            foreach (var orden in _pendientes)
            {
                string origen = orden.ModuloOrigen == "MomosClinic" ? "🩺 Clínica" : "🛒 POS";
                lstVentasEspera.Items.Add($"{orden.Id} - [{origen}] {orden.Referencia}");
            }
        }

        private void BtnRecuperar_Click(object sender, EventArgs e)
        {
            if (lstVentasEspera.SelectedItem != null)
            {
                string text = lstVentasEspera.SelectedItem.ToString();
                string idString = text.Split('-')[0].Trim();
                
                if (int.TryParse(idString, out int id))
                {
                    OrdenSeleccionadaId = id;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Seleccione una orden para recuperar.");
            }
        }
    }
}
