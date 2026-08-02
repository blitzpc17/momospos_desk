using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using System.Collections.Generic;

namespace momospos.Views
{
    public class ConfiguracionView : UserControl
    {
        private TextBox txtNombreNegocio;
        private TextBox txtRFC;
        private TextBox txtDireccion;
        private TextBox txtMensajeTicket;
        private ComboBox cbImpresoras;
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
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            _configRepo.GuardarValor("NombreNegocio", txtNombreNegocio.Text);
            _configRepo.GuardarValor("RFC", txtRFC.Text);
            _configRepo.GuardarValor("Direccion", txtDireccion.Text);
            _configRepo.GuardarValor("MensajeTicket", txtMensajeTicket.Text);
            _configRepo.GuardarValor("ImpresoraTicket", cbImpresoras.SelectedItem?.ToString());

            MessageBox.Show("Configuración guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
