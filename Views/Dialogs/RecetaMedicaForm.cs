using System;
using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views.Dialogs
{
    public class RecetaMedicaForm : Form
    {
        private TextBox txtNombreMedico;
        private TextBox txtCedula;
        private Button btnAceptar;
        private Button btnCancelar;

        public string NombreMedico { get; private set; }
        public string Cedula { get; private set; }

        public RecetaMedicaForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Datos de Receta Médica";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label
            {
                Text = "📝 Registro de Receta Médica",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(30, 20)
            };

            Label lblInfo = new Label
            {
                Text = "Al menos uno de los productos requiere receta médica obligatoria (e.g. Antibiótico). Por favor capture los datos del médico para poder continuar.",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 60),
                Size = new Size(420, 45)
            };

            Label lblNombre = new Label { Text = "Nombre del Médico:", Location = new Point(30, 115), AutoSize = true, Font = Theme.FontNormal };
            txtNombreMedico = new TextBox { Location = new Point(30, 140), Width = 420, Font = new Font("Segoe UI", 12) };

            Label lblCedula = new Label { Text = "Cédula Profesional:", Location = new Point(30, 185), AutoSize = true, Font = Theme.FontNormal };
            txtCedula = new TextBox { Location = new Point(30, 210), Width = 420, Font = new Font("Segoe UI", 12) };

            btnAceptar = new Button { Text = "Aceptar", Location = new Point(220, 260), Width = 110, Height = 40 };
            Theme.StyleButton(btnAceptar, Theme.PrimaryColor);
            btnAceptar.Click += BtnAceptar_Click;

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(340, 260), Width = 110, Height = 40 };
            Theme.StyleButton(btnCancelar, Theme.DangerColor);
            btnCancelar.Click += BtnCancelar_Click;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombreMedico);
            this.Controls.Add(lblCedula);
            this.Controls.Add(txtCedula);
            this.Controls.Add(btnAceptar);
            this.Controls.Add(btnCancelar);
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreMedico.Text))
            {
                CustomDialog.ShowWarning("El nombre del médico es obligatorio.");
                txtNombreMedico.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                CustomDialog.ShowWarning("La cédula profesional del médico es obligatoria.");
                txtCedula.Focus();
                return;
            }

            NombreMedico = txtNombreMedico.Text.Trim();
            Cedula = txtCedula.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
