using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using momospos.Models;
using momospos.Repositories;
using System.Linq;

namespace momospos.Views
{
    public class CobroForm : Form
    {
        private decimal _totalAPagar;
        public decimal PagoEfectivo { get; private set; }
        public decimal PagoTarjeta { get; private set; }
        public decimal PagoCredito { get; private set; }
        public decimal Cambio { get; private set; }
        public int? ClienteIdSeleccionado { get; private set; }

        private Label lblTotal;
        private TextBox txtEfectivo;
        private TextBox txtTarjeta;
        private Label lblRestanteOCambio;
        private Label lblMontoRestanteOCambio;
        private ComboBox cbClientes;
        
        private Button btnCobrar;
        private Button btnCredito;
        private Button btnCancelar;

        private ClienteRepository _clienteRepo;

        public CobroForm(decimal totalAPagar)
        {
            _totalAPagar = totalAPagar;
            _clienteRepo = new ClienteRepository();
            BuildUI();
            Theme.SetIcon(this);
            CargarClientes();
            ActualizarSaldos();
        }

        private void CargarClientes()
        {
            var clientes = _clienteRepo.ObtenerTodos().Where(c => c.Estado == "ACTIVO").ToList();
            cbClientes.DataSource = clientes;
            cbClientes.DisplayMember = "Nombre";
            cbClientes.ValueMember = "Id";
            cbClientes.SelectedIndex = -1;
        }

        private void BuildUI()
        {
            this.Text = "Cobrar Venta";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "TOTAL A PAGAR", Font = Theme.FontSubtitle, ForeColor = Theme.TextLight, AutoSize = true, Location = new Point(20, 15) };
            
            lblTotal = new Label { 
                Text = _totalAPagar.ToString("C"), 
                Font = new Font("Segoe UI", 32, FontStyle.Bold), 
                ForeColor = Color.White, 
                AutoSize = true, 
                Location = new Point(20, 40) 
            };
            
            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblTotal);

            int startY = 120;
            int marginY = 60;
            int labelX = 30;
            int inputX = 180;
            
            this.Controls.Add(new Label { Text = "Pago Efectivo:", Font = Theme.FontTitle, Location = new Point(labelX, startY), AutoSize = true });
            txtEfectivo = new TextBox { Location = new Point(inputX, startY-3), Width = 250, Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = HorizontalAlignment.Right };
            txtEfectivo.TextChanged += (s, e) => ActualizarSaldos();
            txtEfectivo.KeyPress += ValidarNumeros;
            this.Controls.Add(txtEfectivo);
            startY += marginY;

            this.Controls.Add(new Label { Text = "Pago Tarjeta:", Font = Theme.FontTitle, Location = new Point(labelX, startY), AutoSize = true });
            txtTarjeta = new TextBox { Location = new Point(inputX, startY-3), Width = 250, Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = HorizontalAlignment.Right };
            txtTarjeta.TextChanged += (s, e) => ActualizarSaldos();
            txtTarjeta.KeyPress += ValidarNumeros;
            this.Controls.Add(txtTarjeta);
            startY += marginY + 20;

            lblRestanteOCambio = new Label { Text = "Falta:", Font = Theme.FontTitle, Location = new Point(labelX, startY), AutoSize = true, ForeColor = Theme.DangerColor };
            lblMontoRestanteOCambio = new Label { Text = "$0.00", Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(inputX, startY - 5), AutoSize = true, ForeColor = Theme.DangerColor };
            this.Controls.Add(lblRestanteOCambio);
            this.Controls.Add(lblMontoRestanteOCambio);

            startY += marginY;

            btnCobrar = new Button { Text = "Confirmar Pago", Location = new Point(inputX, startY), Width = 250, Height = 50 };
            Theme.StyleButton(btnCobrar, Theme.SuccessColor, Theme.TextLight, Theme.FontTitle);
            btnCobrar.Click += BtnCobrar_Click;
            this.Controls.Add(btnCobrar);

            startY += 70;
            
            Panel divisor = new Panel { Location = new Point(20, startY), Width = 450, Height = 1, BackColor = Color.LightGray };
            this.Controls.Add(divisor);
            
            startY += 20;
            this.Controls.Add(new Label { Text = "Cliente (Crédito):", Font = Theme.FontNormal, Location = new Point(labelX, startY + 5), AutoSize = true });
            cbClientes = new ComboBox { Location = new Point(inputX, startY), Width = 250, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cbClientes);
            
            startY += 40;
            btnCredito = new Button { Text = "💳 Vender a Crédito", Location = new Point(inputX, startY), Width = 250, Height = 40 };
            Theme.StyleButton(btnCredito, Color.FromArgb(41, 128, 185), Theme.TextLight, Theme.FontSubtitle);
            btnCredito.Click += BtnCredito_Click;
            this.Controls.Add(btnCredito);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(labelX, startY), Width = 130, Height = 40 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancelar);

            this.Controls.Add(topPanel);
        }

        private void ValidarNumeros(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true;
        }

        private void ActualizarSaldos()
        {
            decimal.TryParse(txtEfectivo.Text, out decimal efectivo);
            decimal.TryParse(txtTarjeta.Text, out decimal tarjeta);

            decimal pagado = efectivo + tarjeta;
            decimal diferencia = pagado - _totalAPagar;

            if (diferencia < 0)
            {
                lblRestanteOCambio.Text = "Falta:";
                lblRestanteOCambio.ForeColor = Theme.DangerColor;
                lblMontoRestanteOCambio.Text = Math.Abs(diferencia).ToString("C");
                lblMontoRestanteOCambio.ForeColor = Theme.DangerColor;
            }
            else
            {
                lblRestanteOCambio.Text = "Cambio:";
                lblRestanteOCambio.ForeColor = Theme.SuccessColor;
                lblMontoRestanteOCambio.Text = diferencia.ToString("C");
                lblMontoRestanteOCambio.ForeColor = Theme.SuccessColor;
            }
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtEfectivo.Text, out decimal efectivo);
            decimal.TryParse(txtTarjeta.Text, out decimal tarjeta);
            decimal pagado = efectivo + tarjeta;

            if (pagado < _totalAPagar)
            {
                MessageBox.Show("El pago ingresado no cubre el total de la venta.", "Pago Incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PagoEfectivo = efectivo;
            PagoTarjeta = tarjeta;
            PagoCredito = 0;
            Cambio = (pagado > _totalAPagar && tarjeta == 0) ? (pagado - _totalAPagar) : (pagado - _totalAPagar); 
            ClienteIdSeleccionado = null;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCredito_Click(object sender, EventArgs e)
        {
            if (cbClientes.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un cliente para vender a crédito.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cliente = (Cliente)cbClientes.SelectedItem;
            if (cliente.LimiteCredito > 0)
            {
                decimal disponible = cliente.LimiteCredito - cliente.Saldo;

                if (_totalAPagar > disponible)
                {
                    MessageBox.Show($"Crédito insuficiente.\nLímite: {cliente.LimiteCredito:C}\nDeuda Actual: {cliente.Saldo:C}\nDisponible: {disponible:C}", "Crédito Rechazado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            PagoEfectivo = 0;
            PagoTarjeta = 0;
            PagoCredito = _totalAPagar;
            Cambio = 0;
            ClienteIdSeleccionado = cliente.Id;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtEfectivo.Focus();
        }
    }
}
