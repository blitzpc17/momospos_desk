using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Views;

namespace MomosClinic
{
    public class MainForm : Form
    {
        private Panel sideMenuPanel;
        private Panel headerPanel;
        private Panel mainContentPanel;
        private PictureBox pbDoctorLogo;
        
        private Button btnDashboard;
        private Button btnAgenda;
        private Button btnPacientes;
        private Button btnConsultas;
        private Button btnRecetas;
        private Button btnServicios;
        private Button btnConfiguracion;

        private NotifyIcon _notifyIcon;
        private Timer _alertTimer;
        private System.Collections.Generic.HashSet<int> _citasNotificadas;
        private MomosClinic.Repositories.CitaRepository _citaRepo;
        private momospos.Repositories.ConfiguracionRepository _configRepo;
        private momospos.Models.Usuario _usuarioLogueado;
        
        public MainForm(momospos.Models.Usuario usuarioLogueado)
        {
            _usuarioLogueado = usuarioLogueado;
            _citaRepo = new MomosClinic.Repositories.CitaRepository();
            _configRepo = new momospos.Repositories.ConfiguracionRepository();
            _citasNotificadas = new System.Collections.Generic.HashSet<int>();
            
            BuildUI();
            ConfigurarAlertas();
        }

        private void BuildUI()
        {
            this.Text = "MomosClinic Pro - Expediente Médico";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BackgroundColor;

            // Header
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            Panel bottomBorder = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Theme.PrimaryColor };
            headerPanel.Controls.Add(bottomBorder);

            // Hamburger Menu
            Button btnHamburger = new Button { Text = "☰", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.PrimaryColor, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Size = new Size(50, 50), Location = new Point(5, 5), Cursor = Cursors.Hand };
            btnHamburger.FlatAppearance.BorderSize = 0;
            btnHamburger.Click += (s, e) => {
                bool isCollapsed = sideMenuPanel.Width == 250;
                sideMenuPanel.Width = isCollapsed ? 60 : 250;
                
                if (pbDoctorLogo != null)
                {
                    if (isCollapsed)
                    {
                        pbDoctorLogo.Size = new Size(40, 40);
                        pbDoctorLogo.Location = new Point(10, 20);
                    }
                    else
                    {
                        pbDoctorLogo.Size = new Size(150, 150);
                        pbDoctorLogo.Location = new Point(50, 20);
                    }
                }
                
                var newPadding = isCollapsed ? new Padding(0) : new Padding(15, 0, 0, 0);
                var newAlign = isCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;

                Action<Button, string, string> updateBtn = (btn, emoji, fullText) => {
                    if (btn == null) return;
                    btn.Text = isCollapsed ? emoji : fullText;
                    btn.Padding = newPadding;
                    btn.TextAlign = newAlign;
                    btn.Width = isCollapsed ? 60 : 230; // Ajustar ancho para evitar que el texto se esconda
                };

                updateBtn(btnDashboard, "📊", "📊 Dashboard");
                updateBtn(btnAgenda, "📅", "📅 Agenda");
                updateBtn(btnPacientes, "👥", "👥 Pacientes");
                updateBtn(btnConsultas, "🩺", "🩺 Consultas");
                updateBtn(btnRecetas, "💊", "💊 Recetas");
                updateBtn(btnServicios, "💼", "💼 Servicios Médicos");
                updateBtn(btnConfiguracion, "⚙️", "⚙️ Configuración");
            };
            headerPanel.Controls.Add(btnHamburger);

            string clinicName = _configRepo.ObtenerValor("ClinicName");
            if (string.IsNullOrWhiteSpace(clinicName)) clinicName = "MomosClinic Pro";

            Label lblTitle = new Label { 
                Text = $"⚕️ {clinicName}", 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                ForeColor = Theme.PrimaryColor, 
                AutoSize = true, 
                Location = new Point(60, 15) 
            };
            headerPanel.Controls.Add(lblTitle);

            Label lblUsuario = new Label {
                Text = $"👤 {_usuarioLogueado?.Nombre ?? "Admin"} ({(_usuarioLogueado?.EsAdmin == true ? "Administrador" : "Local")})",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(900, 20)
            };
            // Lo anclamos a la derecha
            lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUsuario.Left = this.Width - 300;
            headerPanel.Controls.Add(lblUsuario);

            this.Controls.Add(headerPanel);

            // Side Menu
            sideMenuPanel = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.SecondaryColor };
            this.Controls.Add(sideMenuPanel);

            // Logo Doctor
            pbDoctorLogo = new PictureBox { Size = new Size(150, 150), Location = new Point(50, 20), SizeMode = PictureBoxSizeMode.Zoom, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            string logo = _configRepo.ObtenerValor("ClinicLogo");
            if (!string.IsNullOrWhiteSpace(logo) && System.IO.File.Exists(logo)) pbDoctorLogo.Image = MomosClinic.Helpers.ImageHelper.LoadImageWithoutLock(logo);
            sideMenuPanel.Controls.Add(pbDoctorLogo);

            // Content Area
            mainContentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BackgroundColor };
            this.Controls.Add(mainContentPanel);
            mainContentPanel.BringToFront();

            // Botones de Menú
            int y = 180;

            // Dashboard
            btnDashboard = CreateMenuButton("📊 Dashboard", y);
            y += 60;
            
            // Agenda
            btnAgenda = CreateMenuButton("📅 Agenda", y);
            y += 60;
            
            // Pacientes
            btnPacientes = CreateMenuButton("👥 Pacientes", y);
            y += 60;
            
            // Consultas
            btnConsultas = CreateMenuButton("🩺 Consultas", y);
            y += 60;
            
            // Recetas
            btnRecetas = CreateMenuButton("💊 Recetas", y);
            y += 60;

            // Servicios (Solo Medico o Admin)
            btnServicios = CreateMenuButton("💼 Servicios Médicos", y);
            y += 60;

            // Configuracion (Solo Admin)
            btnConfiguracion = CreateMenuButton("⚙️ Configuración", y);
            y += 60;

            // Submenú Configuración
            Panel pnlConfigSubMenu = new Panel {
                Location = new Point(10, y),
                Width = 230,
                Height = 200,
                BackColor = Theme.SecondaryColor,
                Visible = false
            };
            
            Button btnConfigGeneral = CreateMenuButton("   ⚙️ General", 0);
            Button btnConfigUsuarios = CreateMenuButton("   👥 Usuarios", 50);
            Button btnConfigRoles = CreateMenuButton("   📋 Roles", 100);
            Button btnConfigPermisos = CreateMenuButton("   🔑 Permisos", 150);
            
            pnlConfigSubMenu.Controls.Add(btnConfigGeneral);
            pnlConfigSubMenu.Controls.Add(btnConfigUsuarios);
            pnlConfigSubMenu.Controls.Add(btnConfigRoles);
            pnlConfigSubMenu.Controls.Add(btnConfigPermisos);

            btnConfiguracion.Click += (s, e) => {
                pnlConfigSubMenu.Visible = !pnlConfigSubMenu.Visible;
            };

            // Permisos Centralizados (MomosPOS)
            var seguridadRepo = new momospos.Repositories.SeguridadRepository();
            int uId = _usuarioLogueado?.Id ?? 0;

            if (seguridadRepo.UsuarioTienePermiso(uId, "DashboardView"))
                sideMenuPanel.Controls.Add(btnDashboard);
            
            if (seguridadRepo.UsuarioTienePermiso(uId, "AgendaView"))
                sideMenuPanel.Controls.Add(btnAgenda);
            
            if (seguridadRepo.UsuarioTienePermiso(uId, "PacientesView"))
                sideMenuPanel.Controls.Add(btnPacientes);

            if (seguridadRepo.UsuarioTienePermiso(uId, "ConsultasView"))
                sideMenuPanel.Controls.Add(btnConsultas);
                
            if (seguridadRepo.UsuarioTienePermiso(uId, "RecetasView"))
                sideMenuPanel.Controls.Add(btnRecetas);
                
            if (seguridadRepo.UsuarioTienePermiso(uId, "ServiciosView"))
                sideMenuPanel.Controls.Add(btnServicios);
                
            if (seguridadRepo.UsuarioTienePermiso(uId, "ConfiguracionView"))
            {
                sideMenuPanel.Controls.Add(btnConfiguracion);
                sideMenuPanel.Controls.Add(pnlConfigSubMenu);
            }

            // Eventos de Navegación
            btnDashboard.Click += (s, e) => LoadView(new MomosClinic.Views.DashboardView());
            btnAgenda.Click += (s, e) => LoadView(new MomosClinic.Views.AgendaView());
            btnPacientes.Click += (s, e) => LoadView(new MomosClinic.Views.PacientesView(_usuarioLogueado?.Nombre ?? "Admin"));
            btnConsultas.Click += (s, e) => LoadView(new MomosClinic.Views.ConsultasView());
            btnRecetas.Click += (s, e) => LoadView(new MomosClinic.Views.RecetasView());
            btnServicios.Click += (s, e) => LoadView(new MomosClinic.Views.ServiciosView());
            btnConfigGeneral.Click += (s, e) => LoadView(new MomosClinic.Views.ConfiguracionView());
            btnConfigUsuarios.Click += (s, e) => LoadView(new momospos.Views.UsuariosView());
            btnConfigRoles.Click += (s, e) => LoadView(new momospos.Views.RolesView());
            btnConfigPermisos.Click += (s, e) => LoadView(new momospos.Views.SeguridadView());
            
            // Carga inicial
            bool isAdmin = _usuarioLogueado?.EsAdmin ?? false;
            if (isAdmin || seguridadRepo.UsuarioTienePermiso(uId, "DashboardView"))
                btnDashboard.PerformClick();
            else
                btnAgenda.PerformClick();
        }

        private Button CreateMenuButton(string text, int yPos)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(10, yPos),
                Width = 230,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                BackColor = Theme.SecondaryColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Theme.PrimaryColor;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 97, 141);
            
            btn.MouseEnter += (s, e) => {
                btn.BackColor = Theme.PrimaryColor;
                btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = Theme.SecondaryColor;
                btn.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            };
            
            return btn;
        }

        private void LoadView(Control view)
        {
            mainContentPanel.Controls.Clear();
            view.Dock = DockStyle.Fill;
            mainContentPanel.Controls.Add(view);
        }

        private void ConfigurarAlertas()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = SystemIcons.Information; // Or custom icon
            _notifyIcon.Visible = true;
            _notifyIcon.BalloonTipTitle = "Recordatorio de Cita";

            _alertTimer = new Timer();
            _alertTimer.Interval = 60000; // Check every minute
            _alertTimer.Tick += (s, e) => VerificarProximasCitas();
            _alertTimer.Start();
        }

        private void VerificarProximasCitas()
        {
            try
            {
                var config = _configRepo.ObtenerTodas();
                // Default to 15 minutes if missing
                int minutosAviso = 15;
                if (config != null && config.ContainsKey("AlertaMinutosCita"))
                {
                    int.TryParse(config["AlertaMinutosCita"], out minutosAviso);
                }

                var citas = _citaRepo.ObtenerProximasCitas(minutosAviso);

                foreach (var cita in citas)
                {
                    if (!_citasNotificadas.Contains(cita.Id))
                    {
                        string msg = $"Cita próxima: {cita.NombrePaciente} a las {cita.FechaHora.ToString("hh:mm tt")}";
                        _notifyIcon.BalloonTipText = msg;
                        _notifyIcon.ShowBalloonTip(10000);
                        _citasNotificadas.Add(cita.Id);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore DB transient errors in background worker
            }
        }
    }
}
