using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;

namespace momospos.Views
{
    public class GastosForm : Form
    {
        private TextBox txtMonto;
        private TextBox txtConcepto;
        private Button btnGuardar;
        private Button btnCancelar;

        private CajaRepository _cajaRepo;
        private CajaSesion _sesionActual;
        private Usuario _usuarioActual;

        public GastosForm(CajaSesion sesion, Usuario usuario)
        {
            _sesionActual = sesion;
            _usuarioActual = usuario;
            _cajaRepo = new CajaRepository();
            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "Retiro de Efectivo / Gasto";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.DangerColor };
            Label lblHeader = new Label { 
                Text = "💸 REGISTRAR RETIRO DE CAJA", 
                Font = Theme.FontTitle, 
                ForeColor = Theme.TextLight, 
                AutoSize = true, 
                Location = new Point(20, 15) 
            };
            topPanel.Controls.Add(lblHeader);

            int startY = 80;
            int marginY = 70;

            this.Controls.Add(new Label { Text = "Monto a retirar ($):", Font = Theme.FontNormal, Location = new Point(30, startY), AutoSize = true });
            txtMonto = new TextBox { Location = new Point(30, startY + 25), Width = 150, Font = new Font("Segoe UI", 16, FontStyle.Bold), TextAlign = HorizontalAlignment.Right };
            txtMonto.KeyPress += ValidarNumeros;
            this.Controls.Add(txtMonto);

            startY += marginY;

            this.Controls.Add(new Label { Text = "Concepto / Razón del retiro:", Font = Theme.FontNormal, Location = new Point(30, startY), AutoSize = true });
            txtConcepto = new TextBox { Location = new Point(30, startY + 25), Width = 320, Font = Theme.FontNormal };
            this.Controls.Add(txtConcepto);

            startY += marginY + 10;

            btnGuardar = new Button { Text = "Confirmar Retiro", Location = new Point(30, startY), Width = 200, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.DangerColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(240, startY), Width = 110, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancelar);

            this.Controls.Add(topPanel);
        }

        private void ValidarNumeros(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                momospos.Views.CustomMessageBox.Show("Ingrese un monto válido mayor a cero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string concepto = txtConcepto.Text.Trim();
            if (string.IsNullOrEmpty(concepto))
            {
                momospos.Views.CustomMessageBox.Show("Debe ingresar un concepto para el retiro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Disminuir efectivo esperado (Pasamos negativo)
                _cajaRepo.ActualizarEfectivoEsperado(_sesionActual.Id, -monto);
                
                // Registrar el movimiento
                _cajaRepo.RegistrarMovimientoCaja(new CajaMovimiento
                {
                    CajaSesionId = _sesionActual.Id,
                    Tipo = "RETIRO",
                    Importe = monto,
                    Concepto = concepto,
                    UsuarioId = _usuarioActual.Id,
                    Fecha = DateTime.Now
                });

                CustomMessageBox.Show("Retiro registrado exitosamente. El efectivo esperado en caja ha disminuido.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al registrar retiro: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtMonto.Focus();
        }
    }
}
