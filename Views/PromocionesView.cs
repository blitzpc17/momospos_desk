using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using momospos.Views.Dialogs;

namespace momospos.Views
{
    public class PromocionesView : UserControl
    {
        private DataGridView dgvPromociones;
        private TextBox txtBuscar;
        private Label lblConteo;
        
        private List<Promocion> _todasPromociones;
        private PromocionRepository _repo;

        public PromocionesView()
        {
            _repo = new PromocionRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15) };
            Label lblTitulo = new Label { Text = "🎁 Promociones Dinámicas", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 20) };
            
            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(580, 25) };
            txtBuscar = new TextBox { Location = new Point(660, 22), Width = 250, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            Button btnNueva = new Button { Text = "➕ Nueva Promoción", Location = new Point(380, 18), Width = 170, Height = 35 };
            Theme.StyleButton(btnNueva, Theme.SuccessColor);
            btnNueva.Click += BtnNueva_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);
            topPanel.Controls.Add(btnNueva);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            bottomPanel.Controls.Add(lblConteo);

            dgvPromociones = new DataGridView();
            dgvPromociones.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvPromociones);
            dgvPromociones.CellDoubleClick += DgvPromociones_CellDoubleClick;
            dgvPromociones.MouseClick += DgvPromociones_MouseClick;

            this.Controls.Add(dgvPromociones);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void CargarDatos()
        {
            try
            {
                _todasPromociones = _repo.ObtenerTodas().ToList();
                FiltrarDatos();
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError("Error al cargar promociones: " + ex.Message);
            }
        }

        private void FiltrarDatos()
        {
            if (_todasPromociones == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();
            var filtrados = _todasPromociones;

            if (!string.IsNullOrEmpty(filtro))
            {
                filtrados = _todasPromociones.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLower().Contains(filtro)) || 
                    (p.ProductoNombre != null && p.ProductoNombre.ToLower().Contains(filtro))
                ).ToList();
            }

            dgvPromociones.DataSource = filtrados;
            
            // Ocultar columnas innecesarias
            if (dgvPromociones.Columns["Id"] != null) dgvPromociones.Columns["Id"].Visible = false;
            if (dgvPromociones.Columns["ProductoId"] != null) dgvPromociones.Columns["ProductoId"].Visible = false;
            
            if (dgvPromociones.Columns["ProductoNombre"] != null) dgvPromociones.Columns["ProductoNombre"].HeaderText = "Producto";
            if (dgvPromociones.Columns["ProductoCodigo"] != null) dgvPromociones.Columns["ProductoCodigo"].HeaderText = "Cód. Barras";
            if (dgvPromociones.Columns["DescuentoPorcentaje"] != null) dgvPromociones.Columns["DescuentoPorcentaje"].HeaderText = "% Desc";
            
            // Dar formato a colores de activo/inactivo
            foreach(DataGridViewRow row in dgvPromociones.Rows)
            {
                bool activo = Convert.ToBoolean(row.Cells["Activo"].Value);
                if (!activo)
                    row.DefaultCellStyle.ForeColor = Color.Gray;
            }

            lblConteo.Text = $"Total de promociones: {filtrados.Count}";
        }

        private void BtnNueva_Click(object sender, EventArgs e)
        {
            var form = new PromocionForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _repo.Registrar(form.PromocionConfigurada);
                CustomDialog.ShowMessage("Promoción creada exitosamente.", "Éxito");
                CargarDatos();
            }
        }

        private void DgvPromociones_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditarPromocion();
            }
        }

        private void DgvPromociones_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int row = dgvPromociones.HitTest(e.X, e.Y).RowIndex;

                if (row >= 0)
                {
                    dgvPromociones.ClearSelection();
                    dgvPromociones.Rows[row].Selected = true;

                    var promo = (Promocion)dgvPromociones.Rows[row].DataBoundItem;
                    ContextMenu m = new ContextMenu();
                    m.MenuItems.Add(new MenuItem("✏️ Editar", (s, ev) => EditarPromocion()));
                    
                    if (promo.Activo)
                        m.MenuItems.Add(new MenuItem("⏸️ Desactivar", (s, ev) => CambiarEstado(promo, false)));
                    else
                        m.MenuItems.Add(new MenuItem("▶️ Activar", (s, ev) => CambiarEstado(promo, true)));
                        
                    m.MenuItems.Add("-");
                    m.MenuItems.Add(new MenuItem("🗑️ Eliminar", (s, ev) => Eliminar(promo)));

                    m.Show(dgvPromociones, new Point(e.X, e.Y));
                }
            }
        }

        private void EditarPromocion()
        {
            if (dgvPromociones.CurrentRow == null) return;
            var promo = (Promocion)dgvPromociones.CurrentRow.DataBoundItem;

            var form = new PromocionForm(promo);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _repo.Actualizar(form.PromocionConfigurada);
                CustomDialog.ShowMessage("Promoción actualizada exitosamente.", "Éxito");
                CargarDatos();
            }
        }

        private void CambiarEstado(Promocion promo, bool estado)
        {
            if (CustomDialog.ShowConfirm($"¿Está seguro de {(estado ? "activar" : "desactivar")} la promoción '{promo.Nombre}'?"))
            {
                _repo.CambiarEstado(promo.Id, estado);
                CargarDatos();
            }
        }
        
        private void Eliminar(Promocion promo)
        {
            if (CustomDialog.ShowConfirm($"¿Está completamente seguro de eliminar la promoción '{promo.Nombre}'?\nEsta acción no se puede deshacer.", "Atención"))
            {
                _repo.Eliminar(promo.Id);
                CargarDatos();
            }
        }
    }
}
