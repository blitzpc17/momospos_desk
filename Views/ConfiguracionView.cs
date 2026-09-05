using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using momospos.Repositories;
using System.Collections.Generic;
using System.IO.Ports;
using momospos.Helpers;

namespace momospos.Views
{
    public class ConfiguracionView : UserControl
    {
        private TabControl tabControl;

        // General
        private TextBox txtNombreNegocio;
        private TextBox txtRFC;
        private TextBox txtDireccion;
        private TextBox txtTelefonos;
        private TextBox txtMensajeTicket;
        private ComboBox cbGiroPrincipal;
        private TextBox txtRutaRecursos;
        private Button btnExaminarRuta;
        private PictureBox pbLogoSistema;
        private Button btnSubirLogoSistema;
        private string _rutaLogoSistemaTemp = null;

        // Impresion
        private ComboBox cbImpresoras;
        private ComboBox cbTamanoTicket;
        private CheckBox chkAbrirCajon;
        private PictureBox pbLogoTicket;
        private Button btnSubirLogoTicket;
        private string _rutaLogoTicketTemp = null;
        private CheckBox chkUsarBascula;
        private ComboBox cbPuertoBascula;
        private Button btnProbarBascula;

        // Avanzado
        private CheckBox chkGiroFarmaceutico;
        private CheckBox chkRequiereAutorizacion;
        private CheckBox chkPermitirDescuentoVenta;
        private CheckBox chkCorteCiego;

        // Correo
        private TextBox txtEmailEmisor;
        private TextBox txtPasswordApp;
        private TextBox txtEmailDestino;

        // Base de Datos
        private TextBox txtDbServidor;
        private TextBox txtDbPuerto;
        private TextBox txtDbBaseDatos;
        private TextBox txtDbUsuario;
        private TextBox txtDbPassword;
        private Button btnGuardarDb;

        private Button btnGuardar;
        private ConfiguracionRepository _configRepo;

        public ConfiguracionView()
        {
            _configRepo = new ConfiguracionRepository();
            BuildUI();
            CargarConfiguracion();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "⚙️ Configuración del Sistema", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 10) };
            topPanel.Controls.Add(lblTitulo);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            btnGuardar = new Button { Text = "Guardar Configuración", Location = new Point(20, 15), Width = 250, Height = 50 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontTitle);
            btnGuardar.Click += BtnGuardar_Click;
            bottomPanel.Controls.Add(btnGuardar);

            Button btnVistaPrevia = new Button { Text = "Vista Previa de Ticket", Location = new Point(290, 15), Width = 250, Height = 50 };
            Theme.StyleButton(btnVistaPrevia, Theme.SecondaryColor, Theme.TextLight, Theme.FontTitle);
            btnVistaPrevia.Click += BtnVistaPrevia_Click;
            bottomPanel.Controls.Add(btnVistaPrevia);

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12), Padding = new Point(15, 10) };

            TabPage tabGeneral = new TabPage("General");
            TabPage tabImpresion = new TabPage("Impresión y Hardware");
            TabPage tabAvanzado = new TabPage("Avanzado");
            TabPage tabCorreo = new TabPage("Correo / Notificaciones");
            TabPage tabBaseDatos = new TabPage("Base de Datos");

            BuildTabGeneral(tabGeneral);
            BuildTabImpresion(tabImpresion);
            BuildTabAvanzado(tabAvanzado);
            BuildTabCorreo(tabCorreo);
            BuildTabBaseDatos(tabBaseDatos);

            tabControl.TabPages.Add(tabGeneral);
            tabControl.TabPages.Add(tabImpresion);
            tabControl.TabPages.Add(tabAvanzado);
            tabControl.TabPages.Add(tabCorreo);
            tabControl.TabPages.Add(tabBaseDatos);

            this.Controls.Add(tabControl);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void BuildTabGeneral(TabPage tab)
        {
            tab.AutoScroll = true;
            tab.BackColor = Color.White;
            int y = 20;
            int margin = 70;

            tab.Controls.Add(new Label { Text = "Nombre del Negocio:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtNombreNegocio = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtNombreNegocio);
            y += margin;

            tab.Controls.Add(new Label { Text = "RFC:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtRFC = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtRFC);
            y += margin;

            tab.Controls.Add(new Label { Text = "Dirección:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtDireccion = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtDireccion);
            y += margin;

            tab.Controls.Add(new Label { Text = "Teléfonos:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtTelefonos = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtTelefonos);
            y += margin;

            tab.Controls.Add(new Label { Text = "Mensaje de despedida:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtMensajeTicket = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtMensajeTicket);
            y += margin;

            tab.Controls.Add(new Label { Text = "Giro Principal:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            cbGiroPrincipal = new ComboBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGiroPrincipal.Items.AddRange(new string[] { "General / Abarrotes", "Farmacia", "Papelería", "Verdulería / Carnicería" });
            tab.Controls.Add(cbGiroPrincipal);
            
            cbGiroPrincipal.SelectedIndexChanged += (s, e) => {
                if (cbGiroPrincipal.SelectedItem != null && cbGiroPrincipal.SelectedItem.ToString() == "Farmacia")
                    chkGiroFarmaceutico.Checked = true;
            };
            y += margin;

            // Columna Derecha (General)
            int col2 = 500;
            int y2 = 20;

            tab.Controls.Add(new Label { Text = "Directorio de Recursos:", Font = Theme.FontSubtitle, Location = new Point(col2, y2), AutoSize = true });
            txtRutaRecursos = new TextBox { Location = new Point(col2, y2 + 30), Width = 250, Font = new Font("Segoe UI", 12) };
            tab.Controls.Add(txtRutaRecursos);

            btnExaminarRuta = new Button { Text = "Examinar...", Location = new Point(col2 + 260, y2 + 28), Width = 100, Height = 32 };
            Theme.StyleButton(btnExaminarRuta, Color.Gray, Color.White, new Font("Segoe UI", 10));
            btnExaminarRuta.Click += (s, e) => {
                using (var fbd = new FolderBrowserDialog()) {
                    if (fbd.ShowDialog() == DialogResult.OK) txtRutaRecursos.Text = fbd.SelectedPath;
                }
            };
            tab.Controls.Add(btnExaminarRuta);
            y2 += margin + 20;

            tab.Controls.Add(new Label { Text = "Logo del Sistema (Pantallas, a color)", Font = Theme.FontSubtitle, Location = new Point(col2, y2), AutoSize = true });
            pbLogoSistema = new PictureBox { Location = new Point(col2, y2 + 30), Width = 150, Height = 150, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            tab.Controls.Add(pbLogoSistema);

            btnSubirLogoSistema = new Button { Text = "Subir Logo", Location = new Point(col2 + 170, y2 + 30), Width = 100, Height = 40 };
            Theme.StyleButton(btnSubirLogoSistema, Theme.SecondaryColor);
            btnSubirLogoSistema.Click += (s, e) => {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg" }) {
                    if (ofd.ShowDialog() == DialogResult.OK) {
                        _rutaLogoSistemaTemp = ofd.FileName;
                        pbLogoSistema.Image = Image.FromFile(_rutaLogoSistemaTemp);
                    }
                }
            };
            tab.Controls.Add(btnSubirLogoSistema);
        }

        private void BuildTabImpresion(TabPage tab)
        {
            tab.AutoScroll = true;
            tab.BackColor = Color.White;
            int y = 20;
            int margin = 70;

            tab.Controls.Add(new Label { Text = "Impresora de Tickets:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            cbImpresoras = new ComboBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cbImpresoras.Items.Add("Microsoft Print to PDF");
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer != "Microsoft Print to PDF") cbImpresoras.Items.Add(printer);
            }
            tab.Controls.Add(cbImpresoras);
            y += margin;

            tab.Controls.Add(new Label { Text = "Tamaño de Ticket:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            cbTamanoTicket = new ComboBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTamanoTicket.Items.Add("58mm");
            cbTamanoTicket.Items.Add("80mm");
            tab.Controls.Add(cbTamanoTicket);
            y += margin;

            chkAbrirCajon = new CheckBox { Text = "Abrir cajón de dinero al imprimir", Font = new Font("Segoe UI", 12), Location = new Point(20, y), AutoSize = true };
            tab.Controls.Add(chkAbrirCajon);
            y += margin;

            tab.Controls.Add(new Label { Text = "Báscula Local (COM):", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            chkUsarBascula = new CheckBox { Text = "Habilitar conexión con báscula", Font = new Font("Segoe UI", 12), Location = new Point(20, y + 30), AutoSize = true };
            tab.Controls.Add(chkUsarBascula);
            
            cbPuertoBascula = new ComboBox { Location = new Point(20, y + 60), Width = 150, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbPuertoBascula.Items.AddRange(SerialPort.GetPortNames());
            tab.Controls.Add(cbPuertoBascula);

            btnProbarBascula = new Button { Text = "Probar Conexión", Location = new Point(190, y + 59), Width = 150, Height = 32 };
            Theme.StyleButton(btnProbarBascula, Color.Teal, Color.White, new Font("Segoe UI", 10, FontStyle.Bold));
            btnProbarBascula.Click += BtnProbarBascula_Click;
            tab.Controls.Add(btnProbarBascula);

            // Columna Derecha (Impresión)
            int col2 = 500;
            int y2 = 20;

            tab.Controls.Add(new Label { Text = "Logo para el Ticket (Se imprimirá en B/N)", Font = Theme.FontSubtitle, Location = new Point(col2, y2), AutoSize = true });
            pbLogoTicket = new PictureBox { Location = new Point(col2, y2 + 30), Width = 150, Height = 150, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            tab.Controls.Add(pbLogoTicket);

            btnSubirLogoTicket = new Button { Text = "Subir Logo Ticket", Location = new Point(col2 + 170, y2 + 30), Width = 150, Height = 40 };
            Theme.StyleButton(btnSubirLogoTicket, Theme.SecondaryColor);
            btnSubirLogoTicket.Click += (s, e) => {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg" }) {
                    if (ofd.ShowDialog() == DialogResult.OK) {
                        _rutaLogoTicketTemp = ofd.FileName;
                        pbLogoTicket.Image = Image.FromFile(_rutaLogoTicketTemp);
                    }
                }
            };
            tab.Controls.Add(btnSubirLogoTicket);
            
            Label lblAviso = new Label { Text = "* Al guardar, el fondo transparente\nse convertirá a blanco sólido\nautomáticamente.", Location = new Point(col2 + 170, y2 + 80), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            tab.Controls.Add(lblAviso);
        }

        private void BuildTabAvanzado(TabPage tab)
        {
            tab.AutoScroll = true;
            tab.BackColor = Color.White;
            int y = 20;

            chkGiroFarmaceutico = new CheckBox { Text = "Habilitar opciones de control de caducidades, lotes y vigencias (Farmacia)", Font = new Font("Segoe UI", 12), Location = new Point(20, y), AutoSize = true };
            tab.Controls.Add(chkGiroFarmaceutico);
            y += 40;

            chkRequiereAutorizacion = new CheckBox { Text = "Requerir autorización de supervisor para eliminar artículos y cancelar venta", Font = new Font("Segoe UI", 12), Location = new Point(20, y), AutoSize = true };
            tab.Controls.Add(chkRequiereAutorizacion);
            y += 40;

            chkPermitirDescuentoVenta = new CheckBox { Text = "Permitir editar precios y descuentos directamente en la tabla (Grid)", Font = new Font("Segoe UI", 12), Location = new Point(20, y), AutoSize = true };
            tab.Controls.Add(chkPermitirDescuentoVenta);
            y += 40;

            chkCorteCiego = new CheckBox { Text = "Activar Corte Ciego (Ocultar monto esperado al cajero durante el cierre)", Font = new Font("Segoe UI", 12), Location = new Point(20, y), AutoSize = true };
            tab.Controls.Add(chkCorteCiego);
        }

        private void BuildTabCorreo(TabPage tab)
        {
            tab.AutoScroll = true;
            tab.BackColor = Color.White;
            int y = 20;
            int margin = 70;

            Label lblDesc = new Label { Text = "Configure una cuenta SMTP (ej. Gmail) para enviar automáticamente los cortes de turno y del día.", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true, ForeColor = Color.Gray };
            tab.Controls.Add(lblDesc);
            y += 50;

            tab.Controls.Add(new Label { Text = "Correo Emisor (El que envía):", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtEmailEmisor = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtEmailEmisor);
            y += margin;

            tab.Controls.Add(new Label { Text = "Contraseña de App (SMTP):", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtPasswordApp = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14), PasswordChar = '*' };
            tab.Controls.Add(txtPasswordApp);
            y += margin;

            tab.Controls.Add(new Label { Text = "Correo Destinatario (Dueño/Admin):", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtEmailDestino = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtEmailDestino);
        }

        private void BuildTabBaseDatos(TabPage tab)
        {
            tab.AutoScroll = true;
            tab.BackColor = Color.White;
            int y = 20;
            int margin = 70;

            Label lblDesc = new Label { Text = "Configuración de conexión a PostgreSQL. Al guardar se cerrará la sesión.", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true, ForeColor = Color.Gray };
            tab.Controls.Add(lblDesc);
            y += 50;

            tab.Controls.Add(new Label { Text = "Servidor (Host):", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtDbServidor = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtDbServidor);
            y += margin;

            tab.Controls.Add(new Label { Text = "Puerto:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtDbPuerto = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtDbPuerto);
            y += margin;

            tab.Controls.Add(new Label { Text = "Base de Datos:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtDbBaseDatos = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtDbBaseDatos);
            y += margin;

            tab.Controls.Add(new Label { Text = "Usuario:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtDbUsuario = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            tab.Controls.Add(txtDbUsuario);
            y += margin;

            tab.Controls.Add(new Label { Text = "Contraseña:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            txtDbPassword = new TextBox { Location = new Point(20, y + 30), Width = 400, Font = new Font("Segoe UI", 14), PasswordChar = '*' };
            tab.Controls.Add(txtDbPassword);
            y += margin;

            btnGuardarDb = new Button { Text = "Probar Conexión", Location = new Point(20, y + 30), Width = 150, Height = 40 };
            Theme.StyleButton(btnGuardarDb, Theme.SecondaryColor, Theme.TextLight, Theme.FontTitle);
            btnGuardarDb.Click += BtnProbarConexionDb_Click;
            tab.Controls.Add(btnGuardarDb);
        }

        private void BtnProbarConexionDb_Click(object sender, EventArgs e)
        {
            try
            {
                bool exitoso = momospos.Helpers.ConfiguracionHelper.ProbarConexion(
                    txtDbServidor.Text.Trim(), 
                    txtDbPuerto.Text.Trim(), 
                    txtDbBaseDatos.Text.Trim(), 
                    txtDbUsuario.Text.Trim(), 
                    txtDbPassword.Text.Trim()
                );
                
                if (exitoso)
                {
                    momospos.Views.CustomMessageBox.Show("¡Conexión exitosa!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    momospos.Views.CustomMessageBox.Show("No se pudo conectar a la base de datos con estos datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show($"Error al probar: {ex.Message}", "Error");
            }
        }



        private void CargarConfiguracion()
        {
            var confs = _configRepo.ObtenerTodas();
            if (confs.ContainsKey("NombreNegocio") && confs["NombreNegocio"] != null) txtNombreNegocio.Text = confs["NombreNegocio"];
            if (confs.ContainsKey("RFC") && confs["RFC"] != null) txtRFC.Text = confs["RFC"];
            if (confs.ContainsKey("Direccion") && confs["Direccion"] != null) txtDireccion.Text = confs["Direccion"];
            if (confs.ContainsKey("Telefonos") && confs["Telefonos"] != null) txtTelefonos.Text = confs["Telefonos"];
            if (confs.ContainsKey("MensajeTicket") && confs["MensajeTicket"] != null) txtMensajeTicket.Text = confs["MensajeTicket"];
            
            if (confs.ContainsKey("ImpresoraTicket") && confs["ImpresoraTicket"] != null && cbImpresoras.Items.Contains(confs["ImpresoraTicket"]))
                cbImpresoras.SelectedItem = confs["ImpresoraTicket"];

            if (confs.ContainsKey("TamanoTicket") && confs["TamanoTicket"] != null && cbTamanoTicket.Items.Contains(confs["TamanoTicket"]))
                cbTamanoTicket.SelectedItem = confs["TamanoTicket"];
            
            if (confs.ContainsKey("AbrirCajon") && confs["AbrirCajon"] != null)
                chkAbrirCajon.Checked = confs["AbrirCajon"] == "True";

            if (confs.ContainsKey("GiroPrincipal") && confs["GiroPrincipal"] != null && cbGiroPrincipal.Items.Contains(confs["GiroPrincipal"]))
                cbGiroPrincipal.SelectedItem = confs["GiroPrincipal"];

            if (confs.ContainsKey("GiroFarmaceutico") && confs["GiroFarmaceutico"] != null)
                chkGiroFarmaceutico.Checked = confs["GiroFarmaceutico"] == "true";

            if (confs.ContainsKey("RequerirAutorizacionCancelacion") && confs["RequerirAutorizacionCancelacion"] != null)
                chkRequiereAutorizacion.Checked = confs["RequerirAutorizacionCancelacion"] == "true";

            if (confs.ContainsKey("PermitirDescuentoVenta") && confs["PermitirDescuentoVenta"] != null)
                chkPermitirDescuentoVenta.Checked = confs["PermitirDescuentoVenta"] == "true";

            if (confs.ContainsKey("CorteCiego") && confs["CorteCiego"] != null)
                chkCorteCiego.Checked = confs["CorteCiego"] == "true";

            if (confs.ContainsKey("EmailEmisor") && confs["EmailEmisor"] != null) txtEmailEmisor.Text = confs["EmailEmisor"];
            if (confs.ContainsKey("EmailPassword") && confs["EmailPassword"] != null) txtPasswordApp.Text = confs["EmailPassword"];
            if (confs.ContainsKey("EmailDestino") && confs["EmailDestino"] != null) txtEmailDestino.Text = confs["EmailDestino"];

            chkUsarBascula.Checked = ConfiguracionHelper.ObtenerUsarBascula();
            string puerto = ConfiguracionHelper.ObtenerPuertoBascula();
            if (cbPuertoBascula.Items.Contains(puerto)) cbPuertoBascula.SelectedItem = puerto;
            else if (cbPuertoBascula.Items.Count > 0) cbPuertoBascula.SelectedIndex = 0;
                
            txtRutaRecursos.Text = confs.ContainsKey("RutaRecursos") ? confs["RutaRecursos"] : @"C:\MomosPos_Resources";

            if (confs.ContainsKey("RutaLogo") && !string.IsNullOrEmpty(confs["RutaLogo"]) && System.IO.File.Exists(confs["RutaLogo"]))
            {
                try {
                    using (var fs = new System.IO.FileStream(confs["RutaLogo"], System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        pbLogoSistema.Image = Image.FromStream(fs);
                } catch { }
            }

            if (confs.ContainsKey("RutaLogoTicket") && !string.IsNullOrEmpty(confs["RutaLogoTicket"]) && System.IO.File.Exists(confs["RutaLogoTicket"]))
            {
                try {
                    using (var fs = new System.IO.FileStream(confs["RutaLogoTicket"], System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        pbLogoTicket.Image = Image.FromStream(fs);
                } catch { }
            }

            // Cargar Configuración de Base de Datos
            string connString = momospos.Helpers.ConfiguracionHelper.ObtenerCadenaConexion();
            if (momospos.Helpers.ConfiguracionHelper.AnalizarCadena(connString, out string host, out string port, out string db, out string user, out string pass))
            {
                txtDbServidor.Text = host;
                txtDbPuerto.Text = port;
                txtDbBaseDatos.Text = db;
                txtDbUsuario.Text = user;
                txtDbPassword.Text = pass;
            }
        }

        private void BtnProbarBascula_Click(object sender, EventArgs e)
        {
            if (cbPuertoBascula.SelectedItem == null) { momospos.Views.CustomMessageBox.Show("Seleccione un puerto COM primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                decimal peso = BasculaHelper.LeerPeso(cbPuertoBascula.SelectedItem.ToString());
                momospos.Views.CustomMessageBox.Show($"¡Conexión exitosa!\n\nPeso leído: {peso} kg", "Prueba de Báscula", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show($"Error al leer la báscula:\n{ex.Message}", "Prueba Fallida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnVistaPrevia_Click(object sender, EventArgs e)
        {
            try
            {
                // Crear diccionario con los valores actuales de la pantalla
                var currentConfigs = new Dictionary<string, string>();
                currentConfigs["NombreNegocio"] = txtNombreNegocio.Text;
                currentConfigs["RFC"] = txtRFC.Text;
                currentConfigs["Direccion"] = txtDireccion.Text;
                currentConfigs["Telefonos"] = txtTelefonos.Text;
                currentConfigs["MensajeTicket"] = txtMensajeTicket.Text;
                currentConfigs["TamanoTicket"] = cbTamanoTicket.SelectedItem?.ToString();
                
                if (!string.IsNullOrEmpty(_rutaLogoSistemaTemp)) currentConfigs["RutaLogo"] = _rutaLogoSistemaTemp;
                else if (_configRepo.ObtenerTodas().ContainsKey("RutaLogo")) currentConfigs["RutaLogo"] = _configRepo.ObtenerTodas()["RutaLogo"];
                
                // Generar venta de prueba
                var ventaPrueba = new momospos.Models.Venta
                {
                    Folio = "VP-0001",
                    Fecha = DateTime.Now,
                    UsuarioId = 1,
                    Total = 100.00m,
                    Pagado = 200.00m,
                    Cambio = 100.00m,
                    Detalles = new List<momospos.Models.VentaDetalle>
                    {
                        new momospos.Models.VentaDetalle { Cantidad = 1, Descripcion = "ARTICULO DE PRUEBA 1", PrecioUnitario = 50.00m, Subtotal = 50.00m },
                        new momospos.Models.VentaDetalle { Cantidad = 2, Descripcion = "ARTICULO DE PRUEBA 2", PrecioUnitario = 25.00m, Subtotal = 50.00m }
                    }
                };

                string tempPdf = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TicketPreview.pdf");
                
                var printer = new TicketPrinter(ventaPrueba, currentConfigs);
                printer.ImprimirComoPdf(tempPdf);

                if (System.IO.File.Exists(tempPdf))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempPdf,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show($"Error al generar vista previa:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            _configRepo.GuardarValor("NombreNegocio", txtNombreNegocio.Text);
            _configRepo.GuardarValor("RFC", txtRFC.Text);
            _configRepo.GuardarValor("Direccion", txtDireccion.Text);
            _configRepo.GuardarValor("Telefonos", txtTelefonos.Text);
            _configRepo.GuardarValor("MensajeTicket", txtMensajeTicket.Text);
            _configRepo.GuardarValor("ImpresoraTicket", cbImpresoras.SelectedItem?.ToString());
            _configRepo.GuardarValor("TamanoTicket", cbTamanoTicket.SelectedItem?.ToString());
            _configRepo.GuardarValor("AbrirCajon", chkAbrirCajon.Checked.ToString());
            _configRepo.GuardarValor("GiroPrincipal", cbGiroPrincipal.SelectedItem?.ToString());
            _configRepo.GuardarValor("GiroFarmaceutico", chkGiroFarmaceutico.Checked ? "true" : "false");
            _configRepo.GuardarValor("RequerirAutorizacionCancelacion", chkRequiereAutorizacion.Checked ? "true" : "false");
            _configRepo.GuardarValor("PermitirDescuentoVenta", chkPermitirDescuentoVenta.Checked ? "true" : "false");
            _configRepo.GuardarValor("CorteCiego", chkCorteCiego.Checked ? "true" : "false");

            _configRepo.GuardarValor("EmailEmisor", txtEmailEmisor.Text.Trim());
            _configRepo.GuardarValor("EmailPassword", txtPasswordApp.Text.Trim());
            _configRepo.GuardarValor("EmailDestino", txtEmailDestino.Text.Trim());

            momospos.Helpers.ConfiguracionHelper.GuardarUsarBascula(chkUsarBascula.Checked);
            if (cbPuertoBascula.SelectedItem != null)
                ConfiguracionHelper.GuardarPuertoBascula(cbPuertoBascula.SelectedItem.ToString());

            if (!string.IsNullOrWhiteSpace(txtRutaRecursos.Text))
            {
                _configRepo.GuardarValor("RutaRecursos", txtRutaRecursos.Text.Trim());
                try { System.IO.Directory.CreateDirectory(txtRutaRecursos.Text.Trim()); } catch { }
            }

            // Guardar Logo Sistema
            if (!string.IsNullOrEmpty(_rutaLogoSistemaTemp) && !string.IsNullOrWhiteSpace(txtRutaRecursos.Text))
            {
                try 
                {
                    string dirLogo = System.IO.Path.Combine(txtRutaRecursos.Text.Trim(), "Logo");
                    if (!System.IO.Directory.Exists(dirLogo)) System.IO.Directory.CreateDirectory(dirLogo);
                    string destPath = System.IO.Path.Combine(dirLogo, "logo_sistema.png");
                    System.IO.File.Copy(_rutaLogoSistemaTemp, destPath, true);
                    _configRepo.GuardarValor("RutaLogo", destPath);
                } 
                catch (Exception ex) { momospos.Views.CustomMessageBox.Show("Error al guardar logo de sistema: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }

            // Guardar Logo Ticket con Procesamiento de Fondo
            if (!string.IsNullOrEmpty(_rutaLogoTicketTemp) && !string.IsNullOrWhiteSpace(txtRutaRecursos.Text))
            {
                try 
                {
                    string dirLogo = System.IO.Path.Combine(txtRutaRecursos.Text.Trim(), "Logo");
                    if (!System.IO.Directory.Exists(dirLogo)) System.IO.Directory.CreateDirectory(dirLogo);
                    string destPath = System.IO.Path.Combine(dirLogo, "logo_ticket.png");
                    
                    // Procesar para remover transparencias y poner fondo blanco
                    using (Image imgOriginal = Image.FromFile(_rutaLogoTicketTemp))
                    using (Bitmap bmpTicket = new Bitmap(imgOriginal.Width, imgOriginal.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmpTicket))
                        {
                            g.Clear(Color.White); // Fondo blanco
                            g.DrawImage(imgOriginal, 0, 0, imgOriginal.Width, imgOriginal.Height);
                        }
                        bmpTicket.Save(destPath, ImageFormat.Png);
                    }
                    _configRepo.GuardarValor("RutaLogoTicket", destPath);
                } 
                catch (Exception ex) { momospos.Views.CustomMessageBox.Show("Error al guardar logo de ticket: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }

            // Comprobar si cambió la DB
            string currentConn = momospos.Helpers.ConfiguracionHelper.ObtenerCadenaConexion();
            string newHost = txtDbServidor.Text.Trim();
            string newPort = txtDbPuerto.Text.Trim();
            string newDb = txtDbBaseDatos.Text.Trim();
            string newUser = txtDbUsuario.Text.Trim();
            string newPass = txtDbPassword.Text.Trim();
            
            bool dbChanged = false;
            if (momospos.Helpers.ConfiguracionHelper.AnalizarCadena(currentConn, out string chost, out string cport, out string cdb, out string cuser, out string cpass))
            {
                if (newHost != chost || newPort != cport || newDb != cdb || newUser != cuser || newPass != cpass)
                {
                    dbChanged = true;
                }
            }
            else
            {
                dbChanged = true;
            }

            if (dbChanged)
            {
                var result = momospos.Views.CustomMessageBox.Show("Has modificado la conexión a la Base de Datos. ¿Estás seguro de que deseas cambiarla?\n\nAl confirmar, la aplicación se reiniciará y deberás volver a iniciar sesión para conectarte a la nueva base de datos.", "Confirmar Cambio de BD", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    momospos.Helpers.ConfiguracionHelper.GuardarCadenaConexion(newHost, newPort, newDb, newUser, newPass);
                    Application.Restart();
                    Environment.Exit(0);
                    return; // Fin
                }
                else
                {
                    // Restaurar los textbox a los originales
                    txtDbServidor.Text = chost;
                    txtDbPuerto.Text = cport;
                    txtDbBaseDatos.Text = cdb;
                    txtDbUsuario.Text = cuser;
                    txtDbPassword.Text = cpass;
                }
            }

            momospos.Views.CustomMessageBox.Show("Configuración guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
