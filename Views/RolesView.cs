using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using System.Linq;
using momospos.Models;

namespace momospos.Views
{
    public class RolesView : UserControl
    {
        private DataGridView dgvRoles;
        private RolRepository _repo;

        public RolesView()
        {
            _repo = new RolRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15) };
            Label lblTitulo = new Label { Text = "👥 Catálogo de Roles", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 20) };
            
            Button btnNuevo = new Button { Text = "➕ Nuevo Rol", Location = new Point(350, 18), Width = 150, Height = 35 };
            Theme.StyleButton(btnNuevo, Theme.SuccessColor);
            btnNuevo.Click += BtnNuevo_Click;

            Button btnEditar = new Button { Text = "✏️ Editar Rol", Location = new Point(510, 18), Width = 150, Height = 35 };
            Theme.StyleButton(btnEditar, Theme.WarningColor);
            btnEditar.Click += BtnEditar_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnNuevo);
            topPanel.Controls.Add(btnEditar);

            dgvRoles = new DataGridView();
            dgvRoles.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvRoles);

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvRoles);

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDatos()
        {
            var roles = _repo.ObtenerTodos().ToList();
            dgvRoles.DataSource = roles;

            if (dgvRoles.Columns.Count > 0)
            {
                dgvRoles.Columns["Id"].Width = 50;
                dgvRoles.Columns["Nombre"].Width = 200;
                dgvRoles.Columns["Descripcion"].Width = 300;
                dgvRoles.Columns["Activo"].Width = 80;
            }
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            var form = new Dialogs.RolForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _repo.Insertar(form.RolActual);
                CargarDatos();
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvRoles.SelectedRows.Count == 0) return;
            var id = (int)dgvRoles.SelectedRows[0].Cells["Id"].Value;
            var rol = _repo.ObtenerPorId(id);
            if (rol != null)
            {
                var form = new Dialogs.RolForm(rol);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _repo.Actualizar(form.RolActual);
                    CargarDatos();
                }
            }
        }
    }
}
