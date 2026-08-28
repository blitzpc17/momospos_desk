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

            Button btnNuevo = new Button { Text = "➕ Nuevo Usuario", Location = new Point(350, 18), Width = 150, Height = 35 };
            Theme.StyleButton(btnNuevo, Theme.SuccessColor);
            btnNuevo.Click += BtnNuevo_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);
            topPanel.Controls.Add(btnNuevo);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            bottomPanel.Controls.Add(lblConteo);

            dgvUsuarios = new DataGridView();
            dgvUsuarios.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvUsuarios);
            dgvUsuarios.CellDoubleClick += DgvUsuarios_CellDoubleClick;
            dgvUsuarios.MouseClick += DgvUsuarios_MouseClick;

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
                momospos.Views.CustomMessageBox.Show($"Error al cargar usuarios:\n{ex.Message}");
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
            if (dgvUsuarios.Columns["CajaSesionIdActiva"] != null) dgvUsuarios.Columns["CajaSesionIdActiva"].Visible = false;
            if (dgvUsuarios.Columns["NombreCajaActiva"] != null) dgvUsuarios.Columns["NombreCajaActiva"].Visible = false;
            if (dgvUsuarios.Columns["CreadoEn"] != null) dgvUsuarios.Columns["CreadoEn"].HeaderText = "Creado El";
            if (dgvUsuarios.Columns["EsAdmin"] != null) dgvUsuarios.Columns["EsAdmin"].HeaderText = "¿Admin?";
            if (dgvUsuarios.Columns["UsuarioLogin"] != null) dgvUsuarios.Columns["UsuarioLogin"].HeaderText = "Login";

            lblConteo.Text = $"Total de registros: {filtrados.Count}";
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            var form = new momospos.Views.Dialogs.UsuarioForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var repo = new UsuarioRepository();
                repo.Registrar(form.Usuario);
                momospos.Views.Dialogs.CustomDialog.ShowMessage("Usuario creado correctamente.", "Éxito");
                CargarDatos();
            }
        }

        private void DgvUsuarios_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditarUsuario();
            }
        }

        private void DgvUsuarios_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dgvUsuarios.HitTest(e.X, e.Y).RowIndex;

                if (currentMouseOverRow >= 0)
                {
                    dgvUsuarios.ClearSelection();
                    dgvUsuarios.Rows[currentMouseOverRow].Selected = true;

                    ContextMenu m = new ContextMenu();
                    m.MenuItems.Add(new MenuItem("✏️ Editar", (s, ev) => EditarUsuario()));
                    
                    var usuarioSeleccionado = (Usuario)dgvUsuarios.Rows[currentMouseOverRow].DataBoundItem;
                    if (usuarioSeleccionado.Estado == "ACTIVO")
                        m.MenuItems.Add(new MenuItem("❌ Dar de Baja", (s, ev) => CambiarEstadoUsuario("INACTIVO")));
                    else
                        m.MenuItems.Add(new MenuItem("✅ Reactivar", (s, ev) => CambiarEstadoUsuario("ACTIVO")));

                    m.Show(dgvUsuarios, new Point(e.X, e.Y));
                }
            }
        }

        private void EditarUsuario()
        {
            if (dgvUsuarios.CurrentRow == null) return;
            var usuario = (Usuario)dgvUsuarios.CurrentRow.DataBoundItem;

            var form = new momospos.Views.Dialogs.UsuarioForm(usuario);
            if (form.ShowDialog() == DialogResult.OK)
            {
                var repo = new UsuarioRepository();
                repo.Actualizar(form.Usuario);
                momospos.Views.Dialogs.CustomDialog.ShowMessage("Usuario actualizado correctamente.", "Éxito");
                CargarDatos();
            }
        }

        private void CambiarEstadoUsuario(string nuevoEstado)
        {
            if (dgvUsuarios.CurrentRow == null) return;
            var usuario = (Usuario)dgvUsuarios.CurrentRow.DataBoundItem;

            if (momospos.Views.Dialogs.CustomDialog.ShowConfirm($"¿Está seguro de cambiar el estado del usuario '{usuario.Nombre}' a {nuevoEstado}?"))
            {
                var repo = new UsuarioRepository();
                repo.CambiarEstado(usuario.Id, nuevoEstado);
                CargarDatos();
            }
        }
    }
}
