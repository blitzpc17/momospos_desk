using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class CancelarCitaForm : Form
    {
        private ComboBox cbMotivos;
        private TextBox txtNotas;
        private Button btnAceptar;
        private Button btnCancelar;
        
        public string MotivoSeleccionado { get; private set; }
        public string NotasExtra { get; private set; }

        public CancelarCitaForm(string accionDesc)
        {
            this.Text = accionDesc;
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblInfo = new Label { Text = "Por favor, indique el motivo de la " + accionDesc.ToLower() + ":", Location = new Point(20, 20), AutoSize = true, Font = Theme.FontNormal };
            this.Controls.Add(lblInfo);

            cbMotivos = new ComboBox { Location = new Point(20, 50), Width = 340, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            
            var motivosRepo = new CatalogoMotivosRepository();
            var lista = motivosRepo.ObtenerTodos();
            cbMotivos.DataSource = lista;
            cbMotivos.DisplayMember = "Motivo";
            cbMotivos.ValueMember = "Motivo";
            this.Controls.Add(cbMotivos);

            Label lblNotas = new Label { Text = "Notas adicionales (Opcional):", Location = new Point(20, 90), AutoSize = true, Font = Theme.FontNormal };
            this.Controls.Add(lblNotas);

            txtNotas = new TextBox { Location = new Point(20, 120), Width = 340, Height = 100, Multiline = true, Font = new Font("Segoe UI", 11) };
            this.Controls.Add(txtNotas);

            btnAceptar = new Button { Text = "Aceptar", Location = new Point(200, 240), Width = 160, Height = 40 };
            Theme.StyleButton(btnAceptar, Theme.PrimaryColor);
            btnAceptar.Click += BtnAceptar_Click;
            this.Controls.Add(btnAceptar);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(20, 240), Width = 160, Height = 40 };
            Theme.StyleButton(btnCancelar, Theme.SecondaryColor);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (cbMotivos.SelectedItem == null)
            {
                CustomMessageBox.Show("Debe seleccionar un motivo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MotivoSeleccionado = cbMotivos.SelectedValue.ToString();
            NotasExtra = txtNotas.Text.Trim();
            this.DialogResult = DialogResult.OK;
        }
    }
}
