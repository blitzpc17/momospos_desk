using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;

namespace momospos.Views
{
    public class ClienteForm : Form
    {
        private TextBox txtNombre;
        private TextBox txtTelefono;
        private TextBox txtCorreo;
        private TextBox txtLimiteCredito;
        private Button btnGuardar;
        private Button btnCancelar;

        private ClienteRepository _clienteRepo;

        public Cliente ClienteRegistrado { get; private set; }
        private Cliente _clienteAEditar;

        public ClienteForm(Cliente clienteAEditar = null)
        {
            _clienteRepo = new ClienteRepository();
            _clienteAEditar = clienteAEditar;
            BuildUI();
            Theme.SetIcon(this);
            
            if (_clienteAEditar != null)
            {
                this.Text = "Editar Cliente";
                txtNombre.Text = _clienteAEditar.Nombre;
                txtTelefono.Text = _clienteAEditar.Telefono;
                txtCorreo.Text = _clienteAEditar.Correo;
                txtLimiteCredito.Text = _clienteAEditar.LimiteCredito.ToString("N2");
            }
        }

        private void BuildUI()
        {
            this.Text = "Registrar Nuevo Cliente";
            this.Size = new Size(450, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Ficha del Cliente", Font = Theme.FontTitle, ForeColor = Theme.TextLight, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);

            int startY = 80;
            int marginY = 50;
            int labelX = 30;
            int inputX = 160;
            int inputWidth = 240;

            // Nombre
            this.Controls.Add(new Label { Text = "Nombre:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtNombre = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtNombre);
            startY += marginY;

            // Teléfono
            this.Controls.Add(new Label { Text = "Teléfono:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtTelefono = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal, MaxLength = 10 };
            txtTelefono.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };
            this.Controls.Add(txtTelefono);
            startY += marginY;

            // Correo
            this.Controls.Add(new Label { Text = "Correo:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtCorreo = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtCorreo);
            startY += marginY;

            // Limite de Crédito
            this.Controls.Add(new Label { Text = "Límite Crédito:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtLimiteCredito = new TextBox { Location = new Point(inputX, startY), Width = 120, Font = Theme.FontNormal };
            this.Controls.Add(txtLimiteCredito);
            startY += marginY;

            // Botones
            btnGuardar = new Button { Text = "Guardar", Location = new Point(inputX, startY + 10), Width = 120, Height = 40 };
            Theme.StyleButton(btnGuardar, Theme.SuccessColor);
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(inputX + 130, startY + 10), Width = 110, Height = 40 };
            Theme.StyleButton(btnCancelar, Color.Gray);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancelar);

            this.Controls.Add(topPanel);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string telefono = txtTelefono.Text.Trim();
            if (!string.IsNullOrEmpty(telefono) && telefono.Length != 10)
            {
                MessageBox.Show("El teléfono debe tener exactamente 10 dígitos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtLimiteCredito.Text, out decimal limite))
            {
                limite = 0; // Si no pone nada o es inválido, 0
            }

            ClienteRegistrado = new Cliente
            {
                Id = _clienteAEditar != null ? _clienteAEditar.Id : 0,
                Nombre = txtNombre.Text.Trim(),
                Telefono = txtTelefono.Text.Trim() ?? "",
                Correo = txtCorreo.Text.Trim() ?? "",
                LimiteCredito = limite,
                Saldo = _clienteAEditar != null ? _clienteAEditar.Saldo : 0,
                Estado = _clienteAEditar != null ? _clienteAEditar.Estado : "ACTIVO"
            };

            try
            {
                _clienteRepo.Guardar(ClienteRegistrado);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cliente:\n" + ex.Message);
            }
        }
    }
}
