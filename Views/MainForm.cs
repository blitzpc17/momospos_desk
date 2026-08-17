using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;

namespace momospos.Views
{
    public class MainForm : Form
    {
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Label lblCajero;

        private VentasView ventasView;
        private ProductosView productosView;
        private ClientesView clientesView;
        private UsuariosView usuariosView;

        private Usuario _usuarioActual;
        private CajaSesion _sesionActual;

        public MainForm(Usuario usuario, CajaSesion sesion)
        {
            _usuarioActual = usuario;
            _sesionActual = sesion;
            
            // Instanciar Vistas antes de BuildUI para que el Drawer dinámico pueda usarlas
            ventasView = new VentasView(_usuarioActual, _sesionActual);
            productosView = new ProductosView();
            clientesView = new ClientesView();
            usuariosView = new UsuariosView();

            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "MomosPOS - Sistema de Punto de Venta Profesional";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BackgroundColor;

            try { this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, "Resources", "logo2.ico")); } catch { }

            // --- CONTENT PANEL ---
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BackgroundColor };

            // --- SIDEBAR (Drawer Dinámico) ---
            sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = Theme.SecondaryColor };

            Panel logoPanel = new Panel { Dock = DockStyle.Top, Height = 110 };
            
            Button btnToggle = new Button
            {
                Text = "☰",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Location = new Point(5, 30),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += BtnToggle_Click;

            PictureBox picLogo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(60, 10),
                Size = new Size(180, 90),
                BackColor = Color.Transparent
            };
            
            try 
            { 
                string logoPath = Theme.GetLogoPath();
                if(!string.IsNullOrEmpty(logoPath))
                {
                    picLogo.Image = Image.FromFile(logoPath);
                }
            } 
            catch { }
            
            logoPanel.Controls.Add(btnToggle);
            logoPanel.Controls.Add(picLogo);
            sidebarPanel.Controls.Add(logoPanel);

            FlowLayoutPanel flpDrawer = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            sidebarPanel.Controls.Add(flpDrawer);
            sidebarPanel.Controls.SetChildIndex(logoPanel, sidebarPanel.Controls.Count - 1); // Logo arriba

            Button btnCerrarTurno = CreateMenuButton("Cerrar Turno", "🛑", 0, false);
            btnCerrarTurno.Dock = DockStyle.Bottom;
            btnCerrarTurno.ForeColor = Color.FromArgb(255, 100, 100);
            btnCerrarTurno.Click += (s, e) => { LoadView(new CorteCajaView(_usuarioActual, _sesionActual)); SetActiveButton(btnCerrarTurno); };
            sidebarPanel.Controls.Add(btnCerrarTurno);

            // Cargar árbol de módulos
            SeguridadRepository seguridadRepo = new SeguridadRepository();
            var modulos = seguridadRepo.ObtenerArbolModulos(_usuarioActual.Id, _usuarioActual.EsAdmin);
            RenderizarModulos(modulos, flpDrawer, 0);

            // --- HEADER INFO ---
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            
            // Un pequeño panel sombreado visual debajo del header
            Panel shadowPanel = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray };
            headerPanel.Controls.Add(shadowPanel);

            lblCajero = new Label { 
                Text = $"👤 Cajero: {_usuarioActual.Nombre}   |   🟢 Caja: {_sesionActual.Estado}", 
                Font = Theme.FontNormal, 
                ForeColor = Theme.SecondaryColor,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Width = 500,
                Padding = new Padding(0, 0, 20, 0)
            };
            headerPanel.Controls.Add(lblCajero);

            // --- CONTENT PANEL ---
            // (Inicializado al principio del método)

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(sidebarPanel);
            
            this.KeyPreview = true;
        }

        private Button _activeBtn = null;
        private void SetActiveButton(Button btn)
        {
            if (_activeBtn != null)
            {
                // Solo si el color anterior era PrimaryColor, lo regresamos.
                // En el drawer, los padres no cambian de color como activos de la misma manera
                if (_activeBtn.BackColor == Theme.PrimaryColor) 
                    _activeBtn.BackColor = Theme.SecondaryColor;
            }
            _activeBtn = btn;
            _activeBtn.BackColor = Theme.PrimaryColor;
        }

        private void RenderizarModulos(System.Collections.Generic.List<Modulo> modulos, FlowLayoutPanel contenedor, int nivel)
        {
            foreach (var modulo in modulos)
            {
                if (modulo.Clave == "CANCELAR_VENTAS") continue; // Módulo lógico (permiso), no visual

                bool tieneHijos = modulo.Submodulos != null && modulo.Submodulos.Count > 0;
                
                Button btn = CreateMenuButton(modulo.Nombre, modulo.Icono, nivel, tieneHijos);
                contenedor.Controls.Add(btn);

                if (tieneHijos)
                {
                    // Contenedor colapsable
                    FlowLayoutPanel panelHijos = new FlowLayoutPanel
                    {
                        Width = sidebarPanel.Width,
                        AutoSize = true,
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false,
                        Visible = false, // Inicia colapsado
                        Margin = new Padding(0)
                    };
                    contenedor.Controls.Add(panelHijos);

                    // Toggle al hacer click en el padre
                    btn.Click += (s, e) =>
                    {
                        panelHijos.Visible = !panelHijos.Visible;
                        btn.Text = new string(' ', nivel * 4) + (panelHijos.Visible ? "▼" : "▶") + "  " + modulo.Icono + "  " + modulo.Nombre;
                    };

                    RenderizarModulos(modulo.Submodulos, panelHijos, nivel + 1);
                }
                else
                {
                    // Asignar acción de cargar vista si es una vista real
                    btn.Click += (s, e) => 
                    {
                        CargarVistaPorClave(modulo.Clave);
                        SetActiveButton(btn);
                    };

                    // Seleccionar "DashboardView" por defecto, o "VentasView" como alternativa
                    if (modulo.Clave == "DashboardView" || modulo.Clave == "VentasView")
                    {
                        // Si es VentasView pero ya se cargó una vista (ej. Dashboard), lo ignoramos como default
                        if (modulo.Clave == "DashboardView" || this.ActiveControl == null) // Hack rápido
                        {
                            CargarVistaPorClave(modulo.Clave);
                            SetActiveButton(btn);
                        }
                    }
                }
            }
        }

        private void CargarVistaPorClave(string clave)
        {
            switch (clave)
            {
                case "DashboardView": LoadView(new DashboardView()); break;
                case "VentasView": LoadView(ventasView); break;
                case "ProductosView": LoadView(productosView); break;
                case "ComprasView": LoadView(new ComprasView()); break;
                case "CategoriasView": LoadView(new CategoriasView()); break;
                case "MermasView": LoadView(new MermasView(_usuarioActual)); break;
                case "ClientesView": LoadView(clientesView); break;
                case "CuentasCobrarView": LoadView(new CuentasCobrarView()); break;
                case "UsuariosView": LoadView(usuariosView); break;
                case "ReportesView": LoadView(new ReportesView(_usuarioActual)); break;
                case "ReporteExistenciasView": LoadView(new ReporteExistenciasView()); break;
                case "ConfiguracionView": LoadView(new ConfiguracionView()); break;
                case "AutorizacionesView": LoadView(new AutorizacionesView(_usuarioActual)); break;
                case "SeguridadView": LoadView(new SeguridadView()); break; 
                case "PromocionesView": LoadView(new PromocionesView()); break;
            }
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            bool isCollapsed = sidebarPanel.Width == 60;
            sidebarPanel.Width = isCollapsed ? 260 : 60;
            
            // Actualizar todos los botones de menu
            ActualizarBotonesMenu(sidebarPanel, !isCollapsed);
        }

        private void ActualizarBotonesMenu(Control parent, bool colapsar)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn && btn.Tag is Tuple<string, string> info)
                {
                    btn.Text = colapsar ? info.Item1 : info.Item2;
                    btn.Width = sidebarPanel.Width;
                }
                else if (c is FlowLayoutPanel flp)
                {
                    ActualizarBotonesMenu(flp, colapsar);
                }
                else if (c is Panel p)
                {
                    ActualizarBotonesMenu(p, colapsar);
                }
            }
        }

        private Button CreateMenuButton(string text, string icono, int nivel, bool tieneHijos)
        {
            // Icono predeterminado para padres sin icono asignado
            if (string.IsNullOrEmpty(icono) && tieneHijos) 
            {
                if (text.Contains("Ventas")) icono = "🛒";
                else if (text.Contains("Inventario") || text.Contains("Producto")) icono = "📦";
                else if (text.Contains("Persona") || text.Contains("Cliente")) icono = "👥";
                else if (text.Contains("Admin") || text.Contains("Config")) icono = "⚙️";
                else icono = "📁";
            }

            string paddingSpace = new string(' ', nivel * 4);
            string flecha = tieneHijos ? "▶ " : "  ";
            string iconStr = string.IsNullOrEmpty(icono) ? "   " : icono + " ";
            
            string fullText = paddingSpace + flecha + iconStr + text;
            string collapsedText = string.IsNullOrEmpty(icono) ? (tieneHijos ? "📁" : "🔹") : icono;

            Button btn = new Button { 
                Text = fullText, 
                Tag = new Tuple<string, string>(collapsedText, fullText),
                Width = sidebarPanel.Width, 
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = Theme.FontSubtitle;
            btn.ForeColor = Theme.TextLight;
            
            // Color base según nivel
            if (nivel == 0)
                btn.BackColor = Theme.SecondaryColor; // Dark blue
            else if (nivel == 1)
                btn.BackColor = Color.FromArgb(44, 62, 80); // Slightly lighter
            else
                btn.BackColor = Color.FromArgb(52, 73, 94); // Lighter

            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185); // Hover azul
            return btn;
        }

        private void LoadView(Control view)
        {
            contentPanel.Controls.Clear();
            view.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(view);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (contentPanel.Controls.Count > 0 && contentPanel.Controls[0] is VentasView vv)
            {
                if (keyData == Keys.F12)
                {
                    vv.ProcessF12();
                    return true;
                }
                else if (keyData == Keys.F3)
                {
                    vv.AbrirBuscador();
                    return true;
                }
                else if (keyData == Keys.F4)
                {
                    vv.AbrirRetiro();
                    return true;
                }
                else if (keyData == Keys.F6)
                {
                    vv.PausarVenta();
                    return true;
                }
                else if (keyData == Keys.F7)
                {
                    vv.RecuperarVenta();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
