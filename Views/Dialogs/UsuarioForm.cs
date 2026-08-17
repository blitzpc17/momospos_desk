using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;

namespace momospos.Views.Dialogs
{
    public class UsuarioForm : Form
    {
        public Usuario Usuario { get; private set; }
        private bool _esEdicion;

        private TextBox txtNombre;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private ComboBox cbRol;

        public UsuarioForm(Usuario usuario = null)
        {
            if (usuario == null)
            {
                Usuario = new Usuario();
                _esEdicion = false;
            }
            else
            {
                Usuario = usuario;
                _esEdicion = true;
            }
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Text = _esEdicion ? "Editar Usuario" : "Nuevo Usuario";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label { Text = _esEdicion ? "✏️ Editar Usuario" : "👤 Nuevo Usuario", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.PrimaryColor, AutoSize = true, Location = new Point(30, 20) };

            Label lblNombre = new Label { Text = "Nombre Completo:", Location = new Point(30, 70), AutoSize = true, Font = Theme.FontNormal };
            txtNombre = new TextBox { Location = new Point(30, 95), Width = 320, Font = new Font("Segoe UI", 12) };

            Label lblLogin = new Label { Text = "Usuario (Login):", Location = new Point(30, 140), AutoSize = true, Font = Theme.FontNormal };
            txtLogin = new TextBox { Location = new Point(30, 165), Width = 320, Font = new Font("Segoe UI", 12) };
            if (_esEdicion) txtLogin.Enabled = false; // El login no se cambia, o lo dejamos editable. Mejor editable para flexibilidad.
            txtLogin.Enabled = true;

            Label lblPass = new Label { Text = _esEdicion ? "Contraseña (dejar en blanco para no cambiar):" : "Contraseña:", Location = new Point(30, 210), AutoSize = true, Font = Theme.FontNormal };
            txtPassword = new TextBox { Location = new Point(30, 235), Width = 320, Font = new Font("Segoe UI", 12), UseSystemPasswordChar = true };

            Button btnTogglePass = new Button { Text = "👁", Width = 30, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Cursor = Cursors.Hand, BackColor = Color.White };
            btnTogglePass.Height = txtPassword.ClientSize.Height;
            btnTogglePass.FlatAppearance.BorderSize = 0;
            btnTogglePass.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnTogglePass.Location = new Point(txtPassword.ClientSize.Width - btnTogglePass.Width, 0);
            btnTogglePass.Click += (s, e) => {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                btnTogglePass.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
            };
            txtPassword.Controls.Add(btnTogglePass);

            Label lblRol = new Label { Text = "Rol:", Location = new Point(30, 280), AutoSize = true, Font = Theme.FontNormal };
            cbRol = new ComboBox { Location = new Point(30, 305), Width = 320, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbRol.Items.Add("Cajero");
            cbRol.Items.Add("Administrador");
            cbRol.SelectedIndex = 0;

            Button btnGuardar = new Button { Text = "Guardar", Location = new Point(130, 355), Width = 100, Height = 40 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "Cancelar", Location = new Point(240, 355), Width = 100, Height = 40 };
            Theme.StyleButton(btnCancelar, Theme.SecondaryColor);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            this.Controls.Add(lblLogin);
            this.Controls.Add(txtLogin);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblRol);
            this.Controls.Add(cbRol);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void CargarDatos()
        {
            if (_esEdicion)
            {
                txtNombre.Text = Usuario.Nombre;
                txtLogin.Text = Usuario.UsuarioLogin;
                cbRol.SelectedIndex = Usuario.EsAdmin ? 1 : 0;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                CustomDialog.ShowWarning("El nombre y usuario son obligatorios.");
                return;
            }

            if (!_esEdicion && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                CustomDialog.ShowWarning("La contraseña es obligatoria para un usuario nuevo.");
                return;
            }

            Usuario.Nombre = txtNombre.Text.Trim();
            Usuario.UsuarioLogin = txtLogin.Text.Trim();
            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                Usuario.PasswordHash = txtPassword.Text.Trim();
            }
            Usuario.EsAdmin = (cbRol.SelectedIndex == 1);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
