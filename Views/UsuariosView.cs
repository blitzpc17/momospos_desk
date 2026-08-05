using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using Dapper;

namespace momospos.Views
{
    public class UsuariosView : UserControl
    {
        private DataGridView dgvUsuarios;
        private TextBox txtBuscar;
        private Label lblConteo;
        
        private List<Usuario> _todosUsuarios;

        public UsuariosView()
        {
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15) };
            Label lblTitulo = new Label { Text = "Administración de Usuarios", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 20) };
            
            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(580, 25) };
            txtBuscar = new TextBox { Location = new Point(660, 22), Width = 250, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            bottomPanel.Controls.Add(lblConteo);

            dgvUsuarios = new DataGridView();
            dgvUsuarios.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvUsuarios);

            this.Controls.Add(dgvUsuarios);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void CargarDatos()
        {
            try
            {
                using (var db = new Npgsql.NpgsqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    _todosUsuarios = db.Query<Usuario>("SELECT Id, Nombre, Usuario as UsuarioLogin, PasswordHash, EsAdmin, Estado, CreadoEn FROM Usuarios ORDER BY Nombre").ToList();
                }
                FiltrarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios:\n{ex.Message}");
            }
        }

        private void FiltrarDatos()
        {
            if (_todosUsuarios == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();
            var filtrados = _todosUsuarios;

            if (!string.IsNullOrEmpty(filtro))
            {
                filtrados = _todosUsuarios.Where(u => 
                    (u.Nombre != null && u.Nombre.ToLower().Contains(filtro)) || 
                    (u.UsuarioLogin != null && u.UsuarioLogin.ToLower().Contains(filtro))
                ).ToList();
            }

            dgvUsuarios.DataSource = filtrados;
            
            if (dgvUsuarios.Columns["Id"] != null) dgvUsuarios.Columns["Id"].Visible = false;
            if (dgvUsuarios.Columns["PasswordHash"] != null) dgvUsuarios.Columns["PasswordHash"].Visible = false;

            lblConteo.Text = $"Total de registros: {filtrados.Count}";
        }
    }
}
