using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using momospos.Views.Dialogs;

namespace momospos.Views
{
    public class LoginForm : Form
    {
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnIngresar;
        private Button btnSalir;

        private UsuarioRepository _usuarioRepo;
        public Usuario UsuarioAutenticado { get; private set; }

        public LoginForm()
        {
            _usuarioRepo = new UsuarioRepository();
            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "MomosPOS - Iniciar Sesión";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // Sin bordes de Windows
            this.BackColor = Theme.BackgroundColor;

            try { this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, "Resources", "logo2.ico")); } catch { }

            // Panel Izquierdo (Branding)
            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.PrimaryColor };
            
            PictureBox picLogo = new PictureBox {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 240,
                Height = 70,
                Location = new Point(5, 160)
            };
            try 
            { 
                string loginPngPath = Theme.GetLoginLogoPath();
                if (!string.IsNullOrEmpty(loginPngPath) && System.IO.File.Exists(loginPngPath))
                {
                    picLogo.Image = Image.FromFile(loginPngPath);
                }
            } 
            catch { }
            
            leftPanel.Controls.Add(picLogo);

            // Panel Derecho (Login)
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };

            Label lblTitulo = new Label { Text = "Bienvenido de nuevo", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(40, 40) };
            Label lblSub = new Label { Text = "Por favor, ingresa tus credenciales", Font = Theme.FontSmall, ForeColor = Color.Gray, AutoSize = true, Location = new Point(40, 70) };
            
            Label lblUser = new Label { Text = "Usuario", Font = Theme.FontNormal, ForeColor = Theme.TextDark, Location = new Point(40, 130), AutoSize = true };
            txtUsuario = new TextBox { Location = new Point(40, 155), Width = 250, Font = new Font("Segoe UI", 12) };
            
            Label lblPass = new Label { Text = "Contraseña", Font = Theme.FontNormal, ForeColor = Theme.TextDark, Location = new Point(40, 200), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(40, 225), Width = 250, Font = new Font("Segoe UI", 12), UseSystemPasswordChar = true };

            btnIngresar = new Button { Text = "Iniciar Sesión", Location = new Point(40, 280), Width = 250, Height = 45 };
            Theme.StyleButton(btnIngresar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnIngresar.Click += BtnIngresar_Click;

            btnSalir = new Button { Text = "X", Location = new Point(310, 10), Width = 30, Height = 30, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Cursor = Cursors.Hand };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            rightPanel.Controls.Add(lblTitulo);
            rightPanel.Controls.Add(lblSub);
            rightPanel.Controls.Add(lblUser);
            rightPanel.Controls.Add(txtUsuario);
            rightPanel.Controls.Add(lblPass);
            rightPanel.Controls.Add(txtPassword);
            rightPanel.Controls.Add(btnIngresar);
            rightPanel.Controls.Add(btnSalir);

            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
            
            this.AcceptButton = btnIngresar;
            this.CancelButton = btnSalir;
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                CustomDialog.ShowWarning("Por favor ingresa usuario y contraseña.");
                return;
            }

            try
            {
                UsuarioAutenticado = _usuarioRepo.Autenticar(txtUsuario.Text, txtPassword.Text);
                if (UsuarioAutenticado != null)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    CustomDialog.ShowWarning("Usuario o contraseña incorrectos.", "Error de autenticación");
                }
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError($"Error de conexión:\n{ex.Message}", "Error de Sistema");
            }
        }
    }
}
