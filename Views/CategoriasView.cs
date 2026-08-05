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
    public class CategoriasView : UserControl
    {
        private DataGridView dgvCategorias;
        private Button btnNuevo;
        private TextBox txtBuscar;
        private Label lblConteo;

        private CategoriaRepository _categoriaRepo;
        private List<Categoria> _todasCategorias;

        public CategoriasView()
        {
            _categoriaRepo = new CategoriaRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "Categorías de Productos", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            
            btnNuevo = new Button { Text = "+ Añadir Categoría", Location = new Point(350, 20), Width = 180, Height = 45 };
            Theme.StyleButton(btnNuevo, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnNuevo.Click += BtnNuevo_Click;

            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(560, 30) };
            txtBuscar = new TextBox { Location = new Point(640, 27), Width = 250, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnNuevo);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            bottomPanel.Controls.Add(lblConteo);

            dgvCategorias = new DataGridView();
            dgvCategorias.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvCategorias);
            
            // Para un diseño "más pro", agregamos un margen blanco alrededor de la tabla
            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvCategorias);

            this.Controls.Add(marginPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDatos()
        {
            try
            {
                _todasCategorias = _categoriaRepo.ObtenerTodas();
                FiltrarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar:\n{ex.Message}");
            }
        }

        private void FiltrarDatos()
        {
            if (_todasCategorias == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();
            var filtrados = _todasCategorias;

            if (!string.IsNullOrEmpty(filtro))
            {
                filtrados = _todasCategorias.Where(c => 
                    (c.Nombre != null && c.Nombre.ToLower().Contains(filtro))
                ).ToList();
            }

            dgvCategorias.DataSource = filtrados;
            
            if (dgvCategorias.Columns["Id"] != null) dgvCategorias.Columns["Id"].Visible = false;
            
            if (dgvCategorias.Columns["Nombre"] != null) 
                dgvCategorias.Columns["Nombre"].HeaderText = "Nombre de la Categoría";

            lblConteo.Text = $"Total de registros: {filtrados.Count}";
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            string inputNombre = Interaction.InputBox("Nombre de la nueva Categoría:", "Nueva Categoría", "");
            if (string.IsNullOrWhiteSpace(inputNombre)) return;

            var c = new Categoria { Nombre = inputNombre };
            try
            {
                _categoriaRepo.Guardar(c);
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}");
            }
        }
    }
}
