using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;

namespace momospos.Views.Dialogs
{
    public class AutorizacionForm : Form
    {
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnAutorizar;
        private Button btnCancelar;
        private string _accion;

        public bool Autorizado { get; private set; }
        public Usuario UsuarioAutoriza { get; private set; }

        public AutorizacionForm(string accionDesc = "Realizar Acción")
        {
            _accion = accionDesc;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Autorización Requerida";
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label { Text = "🔒 Se requiere permiso", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.DangerColor, AutoSize = true, Location = new Point(30, 20) };
            
            Label lblInfo = new Label { Text = $"Acción: {_accion}\nSolo un SUPERVISOR o ADMIN puede autorizar esto.", Font = new Font("Segoe UI", 10), Location = new Point(30, 60), Size = new Size(320, 40) };

            Label lblUser = new Label { Text = "Usuario:", Location = new Point(30, 110), AutoSize = true, Font = Theme.FontNormal };
            txtUsuario = new TextBox { Location = new Point(30, 135), Width = 320, Font = new Font("Segoe UI", 12) };

            Label lblPass = new Label { Text = "Contraseña:", Location = new Point(30, 175), AutoSize = true, Font = Theme.FontNormal };
            txtPassword = new TextBox { Location = new Point(30, 200), Width = 320, Font = new Font("Segoe UI", 12), PasswordChar = '*' };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { btnAutorizar.PerformClick(); } };

            btnAutorizar = new Button { Text = "Autorizar", Location = new Point(130, 240), Width = 100, Height = 40 };
            Theme.StyleButton(btnAutorizar, Theme.PrimaryColor);
            btnAutorizar.Click += BtnAutorizar_Click;

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(240, 240), Width = 100, Height = 40 };
            Theme.StyleButton(btnCancelar, Theme.SecondaryColor);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnAutorizar);
            this.Controls.Add(btnCancelar);
        }

        private void BtnAutorizar_Click(object sender, EventArgs e)
        {
            var repo = new UsuarioRepository();
            var user = repo.Autenticar(txtUsuario.Text.Trim(), txtPassword.Text.Trim());

            if (user != null && user.EsAdmin)
            {
                Autorizado = true;
                UsuarioAutoriza = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas o el usuario no tiene permisos de administrador.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
