using System;
using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views
{
    public class AbonarCreditoForm : Form
    {
        public decimal Abono { get; private set; }

        private TextBox txtMonto;

        public AbonarCreditoForm(string clienteNombre, decimal deudaActual)
        {
            BuildUI(clienteNombre, deudaActual);
        }

        private void BuildUI(string clienteNombre, decimal deudaActual)
        {
            this.Text = "Abonar a Crédito";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;
            Theme.SetIcon(this);

            Label lblTitulo = new Label
            {
                Text = "Registrar Abono",
                Font = Theme.FontTitle,
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            Label lblCliente = new Label
            {
                Text = $"Cliente: {clienteNombre}",
                Font = Theme.FontNormalBold,
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(20, 60)
            };

            Label lblDeuda = new Label
            {
                Text = $"Deuda Actual: {deudaActual:C}",
                Font = Theme.FontNormal,
                ForeColor = Theme.WarningColor,
                AutoSize = true,
                Location = new Point(20, 85)
            };

            Label lblMonto = new Label
            {
                Text = "Monto a abonar:",
                Font = Theme.FontNormal,
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(20, 125)
            };

            txtMonto = new TextBox
            {
                Location = new Point(20, 150),
                Width = 340,
                Font = Theme.FontSubtitle,
                Text = "0"
            };
            txtMonto.KeyPress += TxtMonto_KeyPress;
            txtMonto.Click += (s, e) => txtMonto.SelectAll();

            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(140, 200),
                Width = 100,
                Height = 35
            };
            Theme.StyleButton(btnCancelar, Theme.DangerColor);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            Button btnAceptar = new Button
            {
                Text = "Aceptar",
                Location = new Point(250, 200),
                Width = 110,
                Height = 35
            };
            Theme.StyleButton(btnAceptar, Theme.SuccessColor);
            btnAceptar.Click += BtnAceptar_Click;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblCliente);
            this.Controls.Add(lblDeuda);
            this.Controls.Add(lblMonto);
            this.Controls.Add(txtMonto);
            this.Controls.Add(btnCancelar);
            this.Controls.Add(btnAceptar);

            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;
        }

        private void TxtMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && (((TextBox)sender).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal monto) && monto > 0)
            {
                Abono = monto;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                momospos.Views.CustomMessageBox.Show("Ingrese un monto válido mayor a cero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
