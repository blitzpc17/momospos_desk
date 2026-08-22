using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using System.Linq;

namespace momospos.Views.Dialogs
{
    public class RolForm : Form
    {
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private CheckBox chkActivo;
        private Button btnGuardar;
        private Button btnCancelar;

        public Rol RolActual { get; private set; }

        public RolForm(Rol rol = null)
        {
            RolActual = rol ?? new Rol { Activo = true };
            BuildUI();
            if (rol != null) CargarDatos();
        }

        private void BuildUI()
        {
            this.Text = RolActual.Id == 0 ? "Nuevo Rol" : "Editar Rol";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            int y = 20;

            Label lblNombre = new Label { Text = "Nombre:", Location = new Point(20, y), AutoSize = true };
            txtNombre = new TextBox { Location = new Point(120, y), Width = 230 };
            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            y += 40;

            Label lblDesc = new Label { Text = "Descripción:", Location = new Point(20, y), AutoSize = true };
            txtDescripcion = new TextBox { Location = new Point(120, y), Width = 230, Multiline = true, Height = 60 };
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescripcion);
            y += 80;

            chkActivo = new CheckBox { Text = "Activo", Location = new Point(120, y), AutoSize = true };
            this.Controls.Add(chkActivo);
            y += 50;

            btnGuardar = new Button { Text = "💾 Guardar", Location = new Point(120, y), Width = 100, Height = 35 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor);
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(230, y), Width = 100, Height = 35 };
            Theme.StyleButton(btnCancelar, Theme.SecondaryColor);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void CargarDatos()
        {
            txtNombre.Text = RolActual.Nombre;
            txtDescripcion.Text = RolActual.Descripcion;
            chkActivo.Checked = RolActual.Activo;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es requerido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RolActual.Nombre = txtNombre.Text.Trim();
            RolActual.Descripcion = txtDescripcion.Text.Trim();
            RolActual.Activo = chkActivo.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
