using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace momospos.Views
{
    public class BuscadorProductoForm : Form
    {
        private TextBox txtBuscar;
        private DataGridView dgvResultados;
        private Label lblConteo;
        private Button btnAceptarMulti;
        private ProductoRepository _productoRepo;
        
        public Producto ProductoSeleccionado { get; private set; }
        public List<Producto> ProductosMultiSeleccionados { get; private set; }

        private bool _multiSelectMode = false;
        private Dictionary<int, Producto> _selectedProducts = new Dictionary<int, Producto>();

        public BuscadorProductoForm()
        {
            _productoRepo = new ProductoRepository();
            BuildUI();
            Theme.SetIcon(this);
        }

        public BuscadorProductoForm(List<Producto> preseleccionados) : this()
        {
            _multiSelectMode = true;
            if (preseleccionados != null)
            {
                foreach (var p in preseleccionados)
                {
                    _selectedProducts[p.Id] = p;
                }
            }
            ConfigurarMultiSelect();
        }

        private void BuildUI()
        {
            this.Text = "Buscador de Productos";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            try { this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, "Resources", "logo2.ico")); } catch { }

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "Buscar:", Font = Theme.FontSubtitle, Location = new Point(20, 25), AutoSize = true, ForeColor = Theme.TextDark };
            txtBuscar = new TextBox { Location = new Point(100, 22), Width = 550, Font = Theme.FontTitle };
            txtBuscar.TextChanged += (s, e) => Buscar();
            txtBuscar.KeyDown += TxtBuscar_KeyDown;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(txtBuscar);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5), WrapContents = true };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, ForeColor = Theme.TextDark, Margin = new Padding(0, 10, 50, 0) };
            bottomPanel.Controls.Add(lblConteo);

            dgvResultados = new DataGridView();
            dgvResultados.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvResultados);
            dgvResultados.CellDoubleClick += DgvResultados_CellDoubleClick;
            dgvResultados.KeyDown += DgvResultados_KeyDown;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvResultados);

            this.Controls.Add(marginPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void ConfigurarMultiSelect()
        {
            this.Text = "Seleccionar Productos";
            
            // Reemplazamos bottom panel layout
            FlowLayoutPanel bottom = (FlowLayoutPanel)this.Controls[1]; // bottomPanel
            
            btnAceptarMulti = new Button { Text = "Aceptar ✔️", Width = 150, Height = 40, Margin = new Padding(10, 0, 0, 0) };
            Theme.StyleButton(btnAceptarMulti, Theme.SuccessColor);
            btnAceptarMulti.Click += (s, e) => 
            {
                ProductosMultiSeleccionados = _selectedProducts.Values.ToList();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            bottom.Controls.Add(btnAceptarMulti);

            dgvResultados.ReadOnly = false; // Permitimos edición para el checkbox
            dgvResultados.CurrentCellDirtyStateChanged += DgvResultados_CurrentCellDirtyStateChanged;
            dgvResultados.CellValueChanged += DgvResultados_CellValueChanged;
        }

        private void Buscar()
        {
            string query = txtBuscar.Text.Trim();
            var resultados = _productoRepo.BuscarPorNombre(query);
            dgvResultados.DataSource = resultados;
            
            OcultarColumnas();
            
            if (_multiSelectMode)
            {
                AplicarSeleccionesGrid();
            }

            lblConteo.Text = $"Total de registros: {resultados.Count}";
        }
        
        private void OcultarColumnas()
        {
            // Si es multiselect y la columna de check no existe, la creamos
            if (_multiSelectMode && dgvResultados.Columns["CheckSeleccion"] == null)
            {
                DataGridViewCheckBoxColumn checkCol = new DataGridViewCheckBoxColumn
                {
                    Name = "CheckSeleccion",
                    HeaderText = "✔",
                    Width = 50,
                    ReadOnly = false,
                    TrueValue = true,
                    FalseValue = false
                };
                dgvResultados.Columns.Insert(0, checkCol);
            }

            foreach (DataGridViewColumn col in dgvResultados.Columns)
            {
                if (col.Name == "CheckSeleccion")
                {
                    col.Visible = true;
                    continue;
                }

                if (col.Name == "Nombre" || col.Name == "Descripcion" || col.Name == "PrecioVenta" || col.Name == "StockActual")
                {
                    col.Visible = true;
                    col.ReadOnly = true; // Solo el checkbox es editable
                    if (col.Name == "Nombre") 
                    {
                        col.HeaderText = "Nombre";
                        col.Width = 350;
                    }
                    if (col.Name == "Descripcion")
                    {
                        col.HeaderText = "Descripción";
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    if (col.Name == "PrecioVenta") 
                    {
                        col.HeaderText = "Precio Venta";
                        col.DefaultCellStyle.Format = "N2";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    }
                    if (col.Name == "StockActual") 
                    {
                        col.HeaderText = "Stock Actual";
                        col.DefaultCellStyle.Format = "N2";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    }
                }
                else
                {
                    col.Visible = false;
                }
            }
        }

        private void AplicarSeleccionesGrid()
        {
            foreach (DataGridViewRow row in dgvResultados.Rows)
            {
                if (row.DataBoundItem is Producto p)
                {
                    row.Cells["CheckSeleccion"].Value = _selectedProducts.ContainsKey(p.Id);
                }
            }
        }

        private void DgvResultados_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvResultados.IsCurrentCellDirty && dgvResultados.CurrentCell.OwningColumn.Name == "CheckSeleccion")
            {
                dgvResultados.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvResultados_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_multiSelectMode && e.RowIndex >= 0 && dgvResultados.Columns[e.ColumnIndex].Name == "CheckSeleccion")
            {
                bool isChecked = Convert.ToBoolean(dgvResultados.Rows[e.RowIndex].Cells["CheckSeleccion"].Value);
                if (dgvResultados.Rows[e.RowIndex].DataBoundItem is Producto p)
                {
                    if (isChecked)
                        _selectedProducts[p.Id] = p;
                    else
                        _selectedProducts.Remove(p.Id);
                }
            }
        }

        private void SeleccionarProducto()
        {
            if (_multiSelectMode) return; // Si es multi, se usa el botón Aceptar

            if (dgvResultados.CurrentRow != null && dgvResultados.CurrentRow.DataBoundItem is Producto p)
            {
                ProductoSeleccionado = p;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void DgvResultados_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) SeleccionarProducto();
        }

        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                dgvResultados.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && dgvResultados.Rows != null && dgvResultados.Rows.Count > 0)
            {
                dgvResultados.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void DgvResultados_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SeleccionarProducto();
                e.Handled = true;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtBuscar.Focus();
            Buscar();
        }
    }
}
