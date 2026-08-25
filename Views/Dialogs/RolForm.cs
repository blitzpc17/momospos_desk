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
            this.Size = new Size(450, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = this.Text, Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            int y = 80;

            Label lblNombre = new Label { Text = "Nombre:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormalBold };
            txtNombre = new TextBox { Location = new Point(30, y + 25), Width = 370, Font = Theme.FontNormal };
            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            y += 70;

            Label lblDesc = new Label { Text = "Descripción:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormalBold };
            txtDescripcion = new TextBox { Location = new Point(30, y + 25), Width = 370, Multiline = true, Height = 80, Font = Theme.FontNormal };
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescripcion);
            y += 120;

            chkActivo = new CheckBox { Text = "Activo", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal };
            this.Controls.Add(chkActivo);
            y += 40;

            btnGuardar = new Button { Text = "💾 Guardar", Location = new Point(140, y), Width = 120, Height = 40 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor);
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(280, y), Width = 120, Height = 40 };
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
                CustomMessageBox.Show("El nombre es requerido.", "Error de Validación");
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
