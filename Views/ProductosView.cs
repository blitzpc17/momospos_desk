using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using ClosedXML.Excel;

namespace momospos.Views
{
    public class ProductosView : UserControl
    {
        private DataGridView dgvProductos;
        private Button btnNuevo;
        private Button btnActualizar;
        private TextBox txtBuscar;
        private Label lblConteo;
        
        private ProductoRepository _productoRepo;
        private List<Producto> _todosProductos;

        public ProductosView()
        {
            _productoRepo = new ProductoRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15) };
            
            Label lblTitulo = new Label { Text = "Inventario de Productos", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 20) };
            
            btnNuevo = new Button { Text = "+ Añadir Producto", Location = new Point(300, 15), Width = 150, Height = 40 };
            Theme.StyleButton(btnNuevo, Theme.PrimaryColor);
            btnNuevo.Click += BtnNuevo_Click;

            btnActualizar = new Button { Text = "Refrescar", Location = new Point(460, 15), Width = 100, Height = 40 };
            Theme.StyleButton(btnActualizar, Theme.SecondaryColor);
            btnActualizar.Click += (s, e) => CargarDatos();

            Button btnImportar = new Button { Text = "⬆️ Importación Masiva", Location = new Point(570, 15), Width = 160, Height = 40 };
            Theme.StyleButton(btnImportar, Color.FromArgb(155, 89, 182)); // Morado
            btnImportar.Click += (s, e) => { 
                if (new ImportarProductosForm().ShowDialog() == DialogResult.OK) CargarDatos(); 
            };

            Button btnGenerarCodigos = new Button { Text = "🖨️ Etiquetas", Location = new Point(740, 15), Width = 120, Height = 40 };
            Theme.StyleButton(btnGenerarCodigos, Color.FromArgb(46, 204, 113));
            btnGenerarCodigos.Click += (s, e) => { new GeneradorCodigosForm().ShowDialog(); };

            Button btnLotes = new Button { Text = "📦 Gestionar Lotes", Location = new Point(870, 15), Width = 150, Height = 40 };
            Theme.StyleButton(btnLotes, Color.FromArgb(230, 126, 34)); // Naranja
            btnLotes.Click += MiLotes_Click;

            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(1030, 25) };
            txtBuscar = new TextBox { Location = new Point(1110, 22), Width = 200, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnNuevo);
            topPanel.Controls.Add(btnActualizar);
            topPanel.Controls.Add(btnImportar);
            topPanel.Controls.Add(btnGenerarCodigos);
            topPanel.Controls.Add(btnLotes);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
            
            Button btnExportar = new Button { Text = "📥 Exportar a Excel", Width = 180, Height = 40, Margin = new Padding(20, 0, 0, 0) };
            Theme.StyleButton(btnExportar, Color.Teal, Theme.TextLight, Theme.FontNormal);
            btnExportar.Click += BtnExportar_Click;

            bottomPanel.Controls.Add(lblConteo);
            bottomPanel.Controls.Add(btnExportar);

            dgvProductos = new DataGridView();
            dgvProductos.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvProductos);

            // CMS para Editar/Eliminar
            var cms = new ContextMenuStrip();
            var miEditar = new ToolStripMenuItem("✏️ Editar Producto");
            miEditar.Click += MiEditar_Click;
            var miLotes = new ToolStripMenuItem("📦 Gestionar Lotes");
            miLotes.Click += MiLotes_Click;
            var miEliminar = new ToolStripMenuItem("🗑️ Eliminar Producto");
            miEliminar.Click += MiEliminar_Click;
            cms.Items.Add(miEditar);
            cms.Items.Add(miLotes);
            cms.Items.Add(miEliminar);
            dgvProductos.ContextMenuStrip = cms;

            this.Controls.Add(dgvProductos);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void CargarDatos()
        {
            try
            {
                _todosProductos = _productoRepo.ObtenerTodos();
                FiltrarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos:\n{ex.Message}");
            }
        }

        private void FiltrarDatos()
        {
            if (_todosProductos == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();
            var filtrados = _todosProductos;

            var configRepo = new ConfiguracionRepository();
            bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";

            if (!string.IsNullOrEmpty(filtro))
            {
                filtrados = _todosProductos.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLower().Contains(filtro)) || 
                    (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(filtro)) ||
                    (p.ClaveProducto != null && p.ClaveProducto.ToLower().Contains(filtro)) ||
                    (p.CodigoProveedor != null && p.CodigoProveedor.ToLower().Contains(filtro)) ||
                    (isFarmacia && p.SustanciaActiva != null && p.SustanciaActiva.ToLower().Contains(filtro))
                ).ToList();
            }

            dgvProductos.DataSource = filtrados;
            
            if (dgvProductos.Columns["Id"] != null) 
            {
                dgvProductos.Columns["Id"].Visible = false;
                dgvProductos.Columns["Id"].Frozen = true; // Fix para excepción de columnas inmovilizadas
            }
            if (dgvProductos.Columns["CategoriaId"] != null) dgvProductos.Columns["CategoriaId"].Visible = false;
            if (dgvProductos.Columns["UnidadMedidaId"] != null) dgvProductos.Columns["UnidadMedidaId"].Visible = false;
            if (dgvProductos.Columns["PermiteFraccion"] != null) dgvProductos.Columns["PermiteFraccion"].Visible = false;
            if (dgvProductos.Columns["RutaImagen"] != null) dgvProductos.Columns["RutaImagen"].Visible = false;

            // Mejorar visualización de columnas
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            
            if (dgvProductos.Columns["CodigoBarras"] != null)
            {
                dgvProductos.Columns["CodigoBarras"].Frozen = true;
                dgvProductos.Columns["CodigoBarras"].HeaderText = "Código Barras";
            }
            if (dgvProductos.Columns["Nombre"] != null)
            {
                dgvProductos.Columns["Nombre"].Frozen = true;
                dgvProductos.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvProductos.Columns["Nombre"].Width = 280;
            }
            if (dgvProductos.Columns["Descripcion"] != null)
            {
                dgvProductos.Columns["Descripcion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvProductos.Columns["Descripcion"].Width = 280;
                dgvProductos.Columns["Descripcion"].HeaderText = "Descripción";
            }
            if (dgvProductos.Columns["SustanciaActiva"] != null)
            {
                dgvProductos.Columns["SustanciaActiva"].Visible = isFarmacia;
                if (isFarmacia)
                {
                    dgvProductos.Columns["SustanciaActiva"].HeaderText = "DCI / Compuesto";
                    dgvProductos.Columns["SustanciaActiva"].DisplayIndex = 3;
                }
            }

            // Configurar columnas numéricas (Alineación y Textos)
            string[] colsNumericas = { "PrecioCompra", "PrecioVenta", "StockActual", "StockMinimo", "PrecioMayoreo", "CantidadMayoreo" };
            foreach (string colName in colsNumericas)
            {
                if (dgvProductos.Columns[colName] != null)
                {
                    dgvProductos.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    // Agregar espacio en el encabezado para que se vea mejor (ej. PrecioCompra -> Precio Compra)
                    if (colName == "PrecioCompra") dgvProductos.Columns[colName].HeaderText = "Precio Compra";
                    if (colName == "PrecioVenta") dgvProductos.Columns[colName].HeaderText = "Precio Venta";
                    if (colName == "StockActual") dgvProductos.Columns[colName].HeaderText = "Stock Actual";
                    if (colName == "StockMinimo") dgvProductos.Columns[colName].HeaderText = "Stock Mínimo";
                    if (colName == "PrecioMayoreo") dgvProductos.Columns[colName].HeaderText = "Precio Mayoreo";
                    if (colName == "CantidadMayoreo") dgvProductos.Columns[colName].HeaderText = "Cant. Mayoreo";
                }
            }

            lblConteo.Text = $"Total de registros: {filtrados.Count}";
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            var form = new ProductoForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"¡Producto '{form.ProductoRegistrado.Nombre}' guardado correctamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDatos();
            }
        }

        private void MiEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                var row = dgvProductos.SelectedRows[0];
                var producto = row.DataBoundItem as Producto;
                if (producto != null)
                {
                    var form = new ProductoForm(producto);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show($"¡Producto '{form.ProductoRegistrado.Nombre}' actualizado correctamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarDatos();
                    }
                }
            }
        }

        private void MiLotes_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                var row = dgvProductos.SelectedRows[0];
                var producto = row.DataBoundItem as Producto;
                if (producto != null)
                {
                    if (producto.AplicaCaducidad)
                    {
                        new ProductoLotesForm(producto).ShowDialog();
                        CargarDatos();
                    }
                    else
                    {
                        MessageBox.Show("Este producto no tiene activada la opción de caducidad y lotes.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void MiEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                var row = dgvProductos.SelectedRows[0];
                var producto = row.DataBoundItem as Producto;
                if (producto != null)
                {
                    var r = MessageBox.Show($"¿Estás seguro de eliminar el producto '{producto.Nombre}'?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r == DialogResult.Yes)
                    {
                        _productoRepo.Eliminar(producto.Id);
                        MessageBox.Show("Producto eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarDatos();
                    }
                }
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Archivos de Excel (*.xlsx)|*.xlsx", FileName = "Productos.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Productos");

                            // Cabeceras
                            int colIndex = 1;
                            foreach (DataGridViewColumn col in dgvProductos.Columns)
                            {
                                if (col.Visible)
                                {
                                    worksheet.Cell(1, colIndex).Value = col.HeaderText;
                                    worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                                    colIndex++;
                                }
                            }

                            // Filas
                            int rowIndex = 2;
                            foreach (DataGridViewRow row in dgvProductos.Rows)
                            {
                                if (!row.IsNewRow)
                                {
                                    colIndex = 1;
                                    foreach (DataGridViewColumn col in dgvProductos.Columns)
                                    {
                                        if (col.Visible)
                                        {
                                            var cellVal = row.Cells[col.Index].Value;
                                            
                                            if (cellVal != null)
                                            {
                                                if (col.Name == "CodigoBarras" || cellVal is string)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).Style.NumberFormat.Format = "@";
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(cellVal.ToString());
                                                }
                                                else if (cellVal is decimal d)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(d);
                                                    worksheet.Cell(rowIndex, colIndex).Style.NumberFormat.Format = "$#,##0.00";
                                                }
                                                else if (cellVal is int i)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(i);
                                                }
                                                else
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(cellVal.ToString());
                                                }
                                            }
                                            colIndex++;
                                        }
                                    }
                                    rowIndex++;
                                }
                            }

                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                        }

                        MessageBox.Show("Archivo exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
