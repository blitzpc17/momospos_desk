using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using System.Collections.Generic;
using System.IO.Ports;
using momospos.Helpers;

namespace momospos.Views
{
    public class ConfiguracionView : UserControl
    {
        private TextBox txtNombreNegocio;
        private TextBox txtRFC;
        private TextBox txtDireccion;
        private TextBox txtMensajeTicket;
        private ComboBox cbImpresoras;
        private ComboBox cbTamanoTicket;
        private CheckBox chkAbrirCajon;
        private ComboBox cbGiroPrincipal;
        private CheckBox chkGiroFarmaceutico;
        
        // Báscula
        private CheckBox chkUsarBascula;
        private ComboBox cbPuertoBascula;
        private Button btnProbarBascula;
        
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

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "⚙️ Configuración del Sistema", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            topPanel.Controls.Add(lblTitulo);

            Panel contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };

            int startY = 20;
            int marginY = 80;

            contentPanel.Controls.Add(new Label { Text = "Nombre del Negocio:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtNombreNegocio = new TextBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            contentPanel.Controls.Add(txtNombreNegocio);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "RFC:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtRFC = new TextBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            contentPanel.Controls.Add(txtRFC);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "Dirección (Se imprime en el ticket):", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtDireccion = new TextBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            contentPanel.Controls.Add(txtDireccion);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "Mensaje de despedida (Ticket):", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtMensajeTicket = new TextBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14) };
            contentPanel.Controls.Add(txtMensajeTicket);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "Impresora de Tickets:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            cbImpresoras = new ComboBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            
            // Cargar impresoras
            cbImpresoras.Items.Add("Microsoft Print to PDF"); // Opción por defecto para PDF
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer != "Microsoft Print to PDF")
                    cbImpresoras.Items.Add(printer);
            }
            cbImpresoras.SelectedIndex = 0;
            contentPanel.Controls.Add(cbImpresoras);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "Tamaño de Ticket:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            cbTamanoTicket = new ComboBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTamanoTicket.Items.Add("58mm");
            cbTamanoTicket.Items.Add("80mm");
            cbTamanoTicket.SelectedIndex = 0;
            contentPanel.Controls.Add(cbTamanoTicket);

            startY += marginY;

            chkAbrirCajon = new CheckBox { Text = "Abrir cajón de dinero al imprimir", Font = new Font("Segoe UI", 12), Location = new Point(40, startY), AutoSize = true };
            contentPanel.Controls.Add(chkAbrirCajon);

            startY += 40;

            contentPanel.Controls.Add(new Label { Text = "Giro Principal:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            cbGiroPrincipal = new ComboBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGiroPrincipal.Items.AddRange(new string[] { "General / Abarrotes", "Farmacia", "Papelería", "Verdulería / Carnicería" });
            cbGiroPrincipal.SelectedIndex = 0;
            contentPanel.Controls.Add(cbGiroPrincipal);

            cbGiroPrincipal.SelectedIndexChanged += (s, e) => {
                if (cbGiroPrincipal.SelectedItem != null && cbGiroPrincipal.SelectedItem.ToString() == "Farmacia")
                {
                    chkGiroFarmaceutico.Checked = true;
                }
            };

            startY += marginY;

            chkGiroFarmaceutico = new CheckBox { Text = "Habilitar opciones de control de caducidades, lotes y vigencias", Font = new Font("Segoe UI", 12), Location = new Point(40, startY), AutoSize = true };
            contentPanel.Controls.Add(chkGiroFarmaceutico);

            // -- BÁSCULA (Columna Derecha) --
            int basculaX = 500;
            int basculaY = 20;

            Label lblTituloBascula = new Label { Text = "Báscula Local (COM)", Font = Theme.FontSubtitle, Location = new Point(basculaX, basculaY), AutoSize = true };
            contentPanel.Controls.Add(lblTituloBascula);
            
            basculaY += 40;
            chkUsarBascula = new CheckBox { Text = "Habilitar conexión con báscula", Font = new Font("Segoe UI", 12), Location = new Point(basculaX, basculaY), AutoSize = true };
            contentPanel.Controls.Add(chkUsarBascula);

            basculaY += 40;
            contentPanel.Controls.Add(new Label { Text = "Puerto COM:", Font = new Font("Segoe UI", 12), Location = new Point(basculaX, basculaY), AutoSize = true });
            cbPuertoBascula = new ComboBox { Location = new Point(basculaX + 110, basculaY), Width = 150, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            
            // Llenar puertos
            cbPuertoBascula.Items.AddRange(SerialPort.GetPortNames());
            contentPanel.Controls.Add(cbPuertoBascula);

            basculaY += 50;
            btnProbarBascula = new Button { Text = "Probar Conexión", Location = new Point(basculaX, basculaY), Width = 150, Height = 40 };
            Theme.StyleButton(btnProbarBascula, Color.Teal, Color.White, new Font("Segoe UI", 11, FontStyle.Bold));
            btnProbarBascula.Click += BtnProbarBascula_Click;
            contentPanel.Controls.Add(btnProbarBascula);

            startY += marginY + 20;

            btnGuardar = new Button { Text = "Guardar Configuración", Location = new Point(40, startY), Width = 250, Height = 50 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontTitle);
            btnGuardar.Click += BtnGuardar_Click;
            contentPanel.Controls.Add(btnGuardar);

            this.Controls.Add(contentPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarConfiguracion()
        {
            var confs = _configRepo.ObtenerTodas();
            if (confs.ContainsKey("NombreNegocio")) txtNombreNegocio.Text = confs["NombreNegocio"];
            if (confs.ContainsKey("RFC")) txtRFC.Text = confs["RFC"];
            if (confs.ContainsKey("Direccion")) txtDireccion.Text = confs["Direccion"];
            if (confs.ContainsKey("MensajeTicket")) txtMensajeTicket.Text = confs["MensajeTicket"];
            
            if (confs.ContainsKey("ImpresoraTicket"))
            {
                if (cbImpresoras.Items.Contains(confs["ImpresoraTicket"]))
                    cbImpresoras.SelectedItem = confs["ImpresoraTicket"];
            }

            if (confs.ContainsKey("TamanoTicket"))
            {
                if (cbTamanoTicket.Items.Contains(confs["TamanoTicket"]))
                    cbTamanoTicket.SelectedItem = confs["TamanoTicket"];
            }
            
            if (confs.ContainsKey("AbrirCajon"))
            {
                chkAbrirCajon.Checked = confs["AbrirCajon"] == "True";
            }

            if (confs.ContainsKey("GiroPrincipal"))
            {
                if (cbGiroPrincipal.Items.Contains(confs["GiroPrincipal"]))
                    cbGiroPrincipal.SelectedItem = confs["GiroPrincipal"];
            }

            if (confs.ContainsKey("GiroFarmaceutico"))
            {
                chkGiroFarmaceutico.Checked = confs["GiroFarmaceutico"] == "true";
            }

            // Cargar Bascula (Local)
            chkUsarBascula.Checked = ConfiguracionHelper.ObtenerUsarBascula();
            string puerto = ConfiguracionHelper.ObtenerPuertoBascula();
            if (cbPuertoBascula.Items.Contains(puerto))
                cbPuertoBascula.SelectedItem = puerto;
            else if (cbPuertoBascula.Items.Count > 0)
                cbPuertoBascula.SelectedIndex = 0;
        }

        private void BtnProbarBascula_Click(object sender, EventArgs e)
        {
            if (cbPuertoBascula.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un puerto COM primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string puerto = cbPuertoBascula.SelectedItem.ToString();
            try
            {
                decimal peso = BasculaHelper.LeerPeso(puerto);
                MessageBox.Show($"¡Conexión exitosa!\n\nPeso leído: {peso} kg", "Prueba de Báscula", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer la báscula:\n{ex.Message}", "Prueba Fallida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            _configRepo.GuardarValor("NombreNegocio", txtNombreNegocio.Text);
            _configRepo.GuardarValor("RFC", txtRFC.Text);
            _configRepo.GuardarValor("Direccion", txtDireccion.Text);
            _configRepo.GuardarValor("MensajeTicket", txtMensajeTicket.Text);
            _configRepo.GuardarValor("ImpresoraTicket", cbImpresoras.SelectedItem?.ToString());
            _configRepo.GuardarValor("TamanoTicket", cbTamanoTicket.SelectedItem?.ToString());
            _configRepo.GuardarValor("AbrirCajon", chkAbrirCajon.Checked.ToString());
            _configRepo.GuardarValor("GiroPrincipal", cbGiroPrincipal.SelectedItem?.ToString());
            _configRepo.GuardarValor("GiroFarmaceutico", chkGiroFarmaceutico.Checked ? "true" : "false");

            // Guardar Bascula
            ConfiguracionHelper.GuardarUsarBascula(chkUsarBascula.Checked);
            if (cbPuertoBascula.SelectedItem != null)
                ConfiguracionHelper.GuardarPuertoBascula(cbPuertoBascula.SelectedItem.ToString());

            MessageBox.Show("Configuración guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
