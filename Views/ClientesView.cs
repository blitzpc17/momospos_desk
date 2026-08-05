using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using Microsoft.VisualBasic;

namespace momospos.Views
{
    public class ClientesView : UserControl
    {
        private DataGridView dgvClientes;
        private Button btnNuevo;
        private Button btnActualizar;
        private TextBox txtBuscar;
        private Label lblConteo;

        private ClienteRepository _clienteRepo;
        private List<Cliente> _todosClientes;

        public ClientesView()
        {
            _clienteRepo = new ClienteRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15) };
            
            Label lblTitulo = new Label { Text = "Directorio de Clientes", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 20) };
            
            btnNuevo = new Button { Text = "+ Añadir Cliente", Location = new Point(300, 15), Width = 150, Height = 40 };
            Theme.StyleButton(btnNuevo, Theme.PrimaryColor);
            btnNuevo.Click += BtnNuevo_Click;

            btnActualizar = new Button { Text = "Refrescar", Location = new Point(460, 15), Width = 100, Height = 40 };
            Theme.StyleButton(btnActualizar, Theme.SecondaryColor);
            btnActualizar.Click += (s, e) => CargarDatos();

            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(580, 25) };
            txtBuscar = new TextBox { Location = new Point(660, 22), Width = 250, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnNuevo);
            topPanel.Controls.Add(btnActualizar);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            bottomPanel.Controls.Add(lblConteo);

            dgvClientes = new DataGridView();
            dgvClientes.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvClientes);

            this.Controls.Add(dgvClientes);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void CargarDatos()
        {
            try
            {
                _todosClientes = _clienteRepo.ObtenerTodos();
                FiltrarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes:\n{ex.Message}");
            }
        }

        private void FiltrarDatos()
        {
            if (_todosClientes == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();
            var filtrados = _todosClientes;

            if (!string.IsNullOrEmpty(filtro))
            {
                filtrados = _todosClientes.Where(c => 
                    (c.Nombre != null && c.Nombre.ToLower().Contains(filtro)) || 
                    (c.Correo != null && c.Correo.ToLower().Contains(filtro)) ||
                    (c.Telefono != null && c.Telefono.ToLower().Contains(filtro))
                ).ToList();
            }

            dgvClientes.DataSource = filtrados;
            if (dgvClientes.Columns["Id"] != null) dgvClientes.Columns["Id"].Visible = false;

            lblConteo.Text = $"Total de registros: {filtrados.Count}";
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            var form = new ClienteForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"¡Cliente '{form.ClienteRegistrado.Nombre}' registrado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDatos();
            }
        }
    }
}
