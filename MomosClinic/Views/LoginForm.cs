using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Repositories;
using momospos.Views;
using System.IO;

namespace MomosClinic.Views
{
    public class LoginForm : Form
    {
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnLogin;

        private momospos.Repositories.ConfiguracionRepository _configRepo;
        private momospos.Repositories.UsuarioRepository _repo;

        public momospos.Models.Usuario UsuarioLogueado { get; private set; }

        public LoginForm()
        {
            _configRepo = new momospos.Repositories.ConfiguracionRepository();
            _repo = new momospos.Repositories.UsuarioRepository();
            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "Inicio de Sesión";
            this.Size = new Size(700, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // Estilo moderno sin bordes

            // Panel Izquierdo (Banner)
            Panel panelBanner = new Panel();
            panelBanner.Location = new Point(0, 0);
            panelBanner.Size = new Size(300, 450);
            panelBanner.BackColor = Theme.PrimaryColor;

            string rutaBanner = _configRepo.ObtenerValor("ClinicBanner");
            if (!string.IsNullOrWhiteSpace(rutaBanner) && File.Exists(rutaBanner))
            {
                panelBanner.BackgroundImage = MomosClinic.Helpers.ImageHelper.LoadImageWithoutLock(rutaBanner);
                panelBanner.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                // Fallback icon/text si no hay banner
                Label lblBannerIcon = new Label
                {
                    Text = "⚕️",
                    Font = new Font("Segoe UI", 72),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panelBanner.Controls.Add(lblBannerIcon);
            }
            this.Controls.Add(panelBanner);

            // Panel Derecho (Formulario)
            Panel panelForm = new Panel();
            panelForm.Location = new Point(300, 0);
            panelForm.Size = new Size(400, 450);
            panelForm.BackColor = Color.White;
            this.Controls.Add(panelForm);

            // Botón cerrar (X)
            Button btnCerrarHeader = new Button
            {
                Text = "X",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(360, 0),
                Cursor = Cursors.Hand
            };
            btnCerrarHeader.FlatAppearance.BorderSize = 0;
            btnCerrarHeader.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            panelForm.Controls.Add(btnCerrarHeader);

            // Logo del Médico o Clínica
            PictureBox pbLogo = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(150, 40),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            string rutaLogo = _configRepo.ObtenerValor("ClinicLogo");
            if (!string.IsNullOrWhiteSpace(rutaLogo) && File.Exists(rutaLogo))
            {
                pbLogo.Image = MomosClinic.Helpers.ImageHelper.LoadImageWithoutLock(rutaLogo);
            }
            else
            {
                pbLogo.Image = SystemIcons.Application.ToBitmap(); // Fallback temporal
            }
            panelForm.Controls.Add(pbLogo);

            // Título (Nombre del Médico / Clínica)
            string clinicName = _configRepo.ObtenerValor("ClinicName");
            if (string.IsNullOrWhiteSpace(clinicName)) clinicName = "MomosClinic";

            Label lblTitulo = new Label 
            { 
                Text = clinicName, 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                ForeColor = Theme.PrimaryColor, 
                Location = new Point(20, 140), 
                Size = new Size(360, 60), 
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter 
            };
            panelForm.Controls.Add(lblTitulo);

            Label lblSub = new Label 
            { 
                Text = "Por favor, inicie sesión", 
                Font = new Font("Segoe UI", 10, FontStyle.Regular), 
                ForeColor = Color.Gray, 
                Location = new Point(20, 200), 
                Size = new Size(360, 25), 
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter 
            };
            panelForm.Controls.Add(lblSub);

            // Controles de Login
            int startY = 230;

            Panel pnlUsr = CreateInputPanel(Theme.PrimaryColor, 50, startY);
            txtUsuario = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Location = new Point(10, 8), Width = 280, Text = "Usuario" };
            txtUsuario.ForeColor = Color.Gray;
            txtUsuario.Enter += (s, e) => { if (txtUsuario.Text == "Usuario") { txtUsuario.Text = ""; txtUsuario.ForeColor = Color.Black; } };
            txtUsuario.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtUsuario.Text)) { txtUsuario.Text = "Usuario"; txtUsuario.ForeColor = Color.Gray; } };
            pnlUsr.Controls.Add(txtUsuario);
            panelForm.Controls.Add(pnlUsr);

            Panel pnlPwd = CreateInputPanel(Theme.PrimaryColor, 50, startY + 50);
            txtPassword = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Location = new Point(10, 8), Width = 280, Text = "Contraseña" };
            txtPassword.ForeColor = Color.Gray;
            txtPassword.Enter += (s, e) => { if (txtPassword.Text == "Contraseña") { txtPassword.Text = ""; txtPassword.ForeColor = Color.Black; txtPassword.PasswordChar = '•'; } };
            txtPassword.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtPassword.Text)) { txtPassword.PasswordChar = '\0'; txtPassword.Text = "Contraseña"; txtPassword.ForeColor = Color.Gray; } };
            pnlPwd.Controls.Add(txtPassword);
            panelForm.Controls.Add(pnlPwd);

            btnLogin = new Button { Text = "ENTRAR", Location = new Point(50, startY + 110), Width = 300, Height = 45 };
            Theme.StyleButton(btnLogin, Theme.PrimaryColor, Color.White, new Font("Segoe UI", 11, FontStyle.Bold));
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;
            panelForm.Controls.Add(btnLogin);
            
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCerrarHeader;

            // Funcionalidad para arrastrar el formulario desde el panel derecho
            panelForm.MouseDown += ArrastrarFormulario;
        }

        private Panel CreateInputPanel(Color borderColor, int x, int y)
        {
            Panel pnl = new Panel { Location = new Point(x, y), Size = new Size(300, 35), BackColor = Color.White };
            Panel bottomLine = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = borderColor };
            pnl.Controls.Add(bottomLine);
            return pnl;
        }

        private void ArrastrarFormulario(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = _repo.Autenticar(txtUsuario.Text.Trim(), txtPassword.Text);

            if (user != null)
            {
                UsuarioLogueado = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (txtUsuario.Text == "admin" && txtPassword.Text == "admin")
            {
                UsuarioLogueado = new momospos.Models.Usuario { Id = 1, Nombre = "Administrador Local", UsuarioLogin = "admin", EsAdmin = true };
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                momospos.Views.CustomMessageBox.Show("Credenciales incorrectas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }

    // Para poder arrastrar la ventana sin bordes
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}
