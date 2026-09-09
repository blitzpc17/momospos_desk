using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using momospos.Views;
using momospos.Models;
using momospos.Repositories;

namespace MomosClinic.Views.Dialogs
{
    public class SugerirProductoForm : Form
    {
        private TextBox txtNombre;
        private NumericUpDown numCantidad;
        private Button btnGuardar;
        private Button btnCancelar;

        public SugerirProductoForm(string nombreSugerido = "")
        {
            this.Text = "Sugerir Nuevo Producto";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            int y = 20;
            Label lbl1 = new Label { Text = "Nombre del producto solicitado:", AutoSize = true, Location = new Point(20, y), Font = Theme.FontNormal };
            this.Controls.Add(lbl1);
            
            y += 30;
            txtNombre = new TextBox { Location = new Point(20, y), Width = 340, Font = new Font("Segoe UI", 12), Text = nombreSugerido };
            this.Controls.Add(txtNombre);

            y += 40;
            Label lbl2 = new Label { Text = "Cantidad solicitada:", AutoSize = true, Location = new Point(20, y), Font = Theme.FontNormal };
            this.Controls.Add(lbl2);

            numCantidad = new NumericUpDown { Location = new Point(180, y-2), Width = 100, Font = new Font("Segoe UI", 12), Minimum = 1, Value = 1 };
            this.Controls.Add(numCantidad);

            y += 60;
            btnGuardar = new Button { Text = "Registrar Sugerencia", Location = new Point(180, y), Width = 180, Height = 40 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor);
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(20, y), Width = 120, Height = 40 };
            Theme.StyleButton(btnCancelar, Theme.SecondaryColor);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomMessageBox.Show("Ingrese el nombre del producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var repo = new ProductoSugeridoRepository();
            repo.Sugerir(new ProductoSugerido {
                NombreProducto = txtNombre.Text.Trim(),
                CantidadSolicitada = (int)numCantidad.Value,
                SolicitadoPor = 1 // TODO: Usuario Actual
            });

            CustomMessageBox.Show("Producto registrado como sugerencia. El administrador podrá revisarlo.", "Sugerencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
