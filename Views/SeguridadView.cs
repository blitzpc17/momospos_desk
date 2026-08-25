using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using System.Collections.Generic;
using System.Linq;

namespace momospos.Views
{
    public class SeguridadView : UserControl
    {
        private ComboBox cmbRoles;
        private TreeView tvModulosRol;
        private Button btnGuardarRol;

        private ComboBox cmbUsuarios;
        private ComboBox cmbRolUsuario;
        private TreeView tvModulosUsuario;
        private Button btnGuardarUsuario;

        private SeguridadRepository _seguridadRepo;
        private UsuarioRepository _usuarioRepo;
        private List<Modulo> _todosModulos;

        public SeguridadView()
        {
            _seguridadRepo = new SeguridadRepository();
            _usuarioRepo = new UsuarioRepository();
            BuildUI();
            CargarDatosBasicos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // HEADER
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "🔑 Seguridad y Permisos", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            topPanel.Controls.Add(lblTitulo);

            // LAYOUT SPLIT
            TableLayoutPanel splitPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20)
            };
            splitPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            splitPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // PANEL ROLES (Left)
            TableLayoutPanel panelRoles = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(10) };
            panelRoles.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panelRoles.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panelRoles.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panelRoles.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelRoles.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lblRoles = new Label { Text = "Permisos por Rol", Font = Theme.FontSubtitle, AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            Label lblSelectRol = new Label { Text = "Selecciona un Rol:", AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            
            cmbRoles = new ComboBox { Width = 350, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 15) };
            cmbRoles.SelectedIndexChanged += CmbRoles_SelectedIndexChanged;

            tvModulosRol = new TreeView { Dock = DockStyle.Fill, CheckBoxes = true, Font = Theme.FontNormal, Margin = new Padding(0, 0, 0, 15) };
            
            btnGuardarRol = new Button { Text = "💾 Guardar Permisos del Rol", Width = 280, Height = 45, Margin = new Padding(0) };
            Theme.StyleButton(btnGuardarRol, Theme.PrimaryColor);
            btnGuardarRol.Click += BtnGuardarRol_Click;

            panelRoles.Controls.Add(lblRoles, 0, 0);
            panelRoles.Controls.Add(lblSelectRol, 0, 1);
            panelRoles.Controls.Add(cmbRoles, 0, 2);
            panelRoles.Controls.Add(tvModulosRol, 0, 3);
            panelRoles.Controls.Add(btnGuardarRol, 0, 4);

            // PANEL USUARIOS (Right)
            TableLayoutPanel panelUsuarios = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(10) };
            panelUsuarios.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
            panelUsuarios.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Combos Panel
            panelUsuarios.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Helper Text
            panelUsuarios.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Tree
            panelUsuarios.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Save

            Label lblUsuarios = new Label { Text = "Excepciones por Usuario", Font = Theme.FontSubtitle, AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            
            // Sub-panel para los dos combos
            TableLayoutPanel combosPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            combosPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            combosPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            
            Label lblSelectUsu = new Label { Text = "Selecciona un Usuario:", AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            cmbUsuarios = new ComboBox { Dock = DockStyle.Fill, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 10, 0) };
            cmbUsuarios.SelectedIndexChanged += CmbUsuarios_SelectedIndexChanged;

            Label lblRolAsignado = new Label { Text = "Rol Asignado:", AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            cmbRolUsuario = new ComboBox { Dock = DockStyle.Fill, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0) };
            
            combosPanel.Controls.Add(lblSelectUsu, 0, 0);
            combosPanel.Controls.Add(lblRolAsignado, 1, 0);
            combosPanel.Controls.Add(cmbUsuarios, 0, 1);
            combosPanel.Controls.Add(cmbRolUsuario, 1, 1);

            Label lblHelper = new Label { Text = "Marca/Desmarca para crear excepciones explícitas a su rol:", AutoSize = true, ForeColor = Color.Gray, Margin = new Padding(0, 0, 0, 5) };
            tvModulosUsuario = new TreeView { Dock = DockStyle.Fill, CheckBoxes = true, Font = Theme.FontNormal, Margin = new Padding(0, 0, 0, 15) };
            
            btnGuardarUsuario = new Button { Text = "💾 Guardar Permisos del Usuario", Width = 280, Height = 45, Margin = new Padding(0) };
            Theme.StyleButton(btnGuardarUsuario, Theme.PrimaryColor);
            btnGuardarUsuario.Click += BtnGuardarUsuario_Click;

            panelUsuarios.Controls.Add(lblUsuarios, 0, 0);
            panelUsuarios.Controls.Add(combosPanel, 0, 1);
            panelUsuarios.Controls.Add(lblHelper, 0, 2);
            panelUsuarios.Controls.Add(tvModulosUsuario, 0, 3);
            panelUsuarios.Controls.Add(btnGuardarUsuario, 0, 4);

            splitPanel.Controls.Add(panelRoles, 0, 0);
            splitPanel.Controls.Add(panelUsuarios, 1, 0);

            this.Controls.Add(splitPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDatosBasicos()
        {
            _todosModulos = _seguridadRepo.ObtenerTodosLosModulosPlana();
            var roles = _seguridadRepo.ObtenerTodosLosRoles().ToList();
            var usuarios = _usuarioRepo.ObtenerTodos().ToList();

            // Llenar combos de roles
            cmbRoles.DataSource = new List<Rol>(roles);
            cmbRoles.DisplayMember = "Nombre";
            cmbRoles.ValueMember = "Id";

            // Se agrega opción vacía para usuarios sin rol
            var rolesParaUsuario = new List<Rol>(roles);
            rolesParaUsuario.Insert(0, new Rol { Id = 0, Nombre = "-- SIN ROL --" });
            cmbRolUsuario.DataSource = rolesParaUsuario;
            cmbRolUsuario.DisplayMember = "Nombre";
            cmbRolUsuario.ValueMember = "Id";

            cmbUsuarios.DataSource = usuarios;
            cmbUsuarios.DisplayMember = "Nombre";
            cmbUsuarios.ValueMember = "Id";

            ConstruirArbolTreeView(tvModulosRol, _todosModulos);
            ConstruirArbolTreeView(tvModulosUsuario, _todosModulos);
        }

        private void ConstruirArbolTreeView(TreeView tv, List<Modulo> modulosPlana)
        {
            tv.Nodes.Clear();
            var dict = new Dictionary<int, TreeNode>();
            
            // Primero creamos todos los nodos
            foreach (var m in modulosPlana.OrderBy(x => x.Orden))
            {
                var node = new TreeNode($"{m.Icono} {m.Nombre}") { Tag = m.Id };
                dict[m.Id] = node;
            }

            // Armamos la jerarquía
            foreach (var m in modulosPlana.OrderBy(x => x.Orden))
            {
                if (m.PadreId.HasValue && dict.ContainsKey(m.PadreId.Value))
                {
                    dict[m.PadreId.Value].Nodes.Add(dict[m.Id]);
                }
                else
                {
                    tv.Nodes.Add(dict[m.Id]);
                }
            }
            tv.ExpandAll();
        }

        private void CmbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRoles.SelectedItem is Rol rol)
            {
                var modulosRol = _seguridadRepo.ObtenerModulosPorRol(rol.Id);
                MarcarNodos(tvModulosRol.Nodes, modulosRol);
            }
        }

        private void CmbUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUsuarios.SelectedItem is Usuario usuario)
            {
                if (usuario.EsAdmin)
                {
                    MessageBox.Show("Este usuario es Administrador y tiene acceso total. No se pueden modificar sus permisos específicos.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tvModulosUsuario.Enabled = false;
                    cmbRolUsuario.Enabled = false;
                    btnGuardarUsuario.Enabled = false;
                    return;
                }
                else
                {
                    tvModulosUsuario.Enabled = true;
                    cmbRolUsuario.Enabled = true;
                    btnGuardarUsuario.Enabled = true;
                }

                // Cargar Rol
                var rol = _seguridadRepo.ObtenerRolDeUsuario(usuario.Id);
                cmbRolUsuario.SelectedValue = rol?.Id ?? 0;

                // Cargar árbol base del rol + excepciones
                var permisosFinales = new List<int>();
                if (rol != null)
                {
                    permisosFinales.AddRange(_seguridadRepo.ObtenerModulosPorRol(rol.Id));
                }

                var excepciones = _seguridadRepo.ObtenerModulosPorUsuario(usuario.Id);
                foreach (var exc in excepciones)
                {
                    if (exc.Concedido && !permisosFinales.Contains(exc.ModuloId)) permisosFinales.Add(exc.ModuloId);
                    if (!exc.Concedido) permisosFinales.Remove(exc.ModuloId);
                }

                MarcarNodos(tvModulosUsuario.Nodes, permisosFinales);
            }
        }

        private void MarcarNodos(TreeNodeCollection nodos, List<int> idsAMarcar)
        {
            foreach (TreeNode nodo in nodos)
            {
                int id = (int)nodo.Tag;
                nodo.Checked = idsAMarcar.Contains(id);
                MarcarNodos(nodo.Nodes, idsAMarcar);
            }
        }

        private void RecolectarNodosMarcados(TreeNodeCollection nodos, List<int> recolectados)
        {
            foreach (TreeNode nodo in nodos)
            {
                if (nodo.Checked) recolectados.Add((int)nodo.Tag);
                RecolectarNodosMarcados(nodo.Nodes, recolectados);
            }
        }

        private void BtnGuardarRol_Click(object sender, EventArgs e)
        {
            if (cmbRoles.SelectedItem is Rol rol)
            {
                var seleccionados = new List<int>();
                RecolectarNodosMarcados(tvModulosRol.Nodes, seleccionados);
                
                try
                {
                    _seguridadRepo.GuardarModulosPorRol(rol.Id, seleccionados);
                    MessageBox.Show("Permisos de rol actualizados exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnGuardarUsuario_Click(object sender, EventArgs e)
        {
            if (cmbUsuarios.SelectedItem is Usuario usuario)
            {
                int? rolId = null;
                if (cmbRolUsuario.SelectedValue is int rId && rId > 0)
                {
                    rolId = rId;
                }

                var marcadosEnUI = new List<int>();
                RecolectarNodosMarcados(tvModulosUsuario.Nodes, marcadosEnUI);

                var baseRol = rolId.HasValue ? _seguridadRepo.ObtenerModulosPorRol(rolId.Value) : new List<int>();
                
                var excepciones = new List<UsuarioModulo>();

                // Todos los módulos: verificamos si coinciden con la base del rol o son excepciones
                foreach (var mod in _todosModulos)
                {
                    bool deberiaTener = baseRol.Contains(mod.Id);
                    bool tieneEnUI = marcadosEnUI.Contains(mod.Id);

                    if (deberiaTener && !tieneEnUI)
                    {
                        // Excepción negativa
                        excepciones.Add(new UsuarioModulo { ModuloId = mod.Id, Concedido = false });
                    }
                    else if (!deberiaTener && tieneEnUI)
                    {
                        // Excepción positiva
                        excepciones.Add(new UsuarioModulo { ModuloId = mod.Id, Concedido = true });
                    }
                }

                try
                {
                    _seguridadRepo.GuardarPermisosUsuario(usuario.Id, rolId, excepciones);
                    MessageBox.Show("Permisos de usuario actualizados exitosamente.\n(Tendrá que volver a iniciar sesión para ver los cambios).", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
