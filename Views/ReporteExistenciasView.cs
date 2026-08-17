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
    public class ReporteExistenciasView : UserControl
    {
        private DataGridView dgvExistencias;
        private Button btnActualizar;
        private Button btnExportar;
        private Label lblConteo;
        private TextBox txtBuscar;
        private ComboBox cbFiltroEstado;
        
        private ProductoRepository _productoRepo;
        private List<ReporteExistenciasDTO> _datos;

        public ReporteExistenciasView()
        {
            _productoRepo = new ProductoRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "📊 Estado de Existencias (Stock)", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            btnActualizar = new Button { Text = "🔄 Refrescar", Location = new Point(400, 20), Width = 120, Height = 40 };
            Theme.StyleButton(btnActualizar, Theme.SecondaryColor);
            btnActualizar.Click += (s, e) => CargarDatos();

            Label lblFiltro = new Label { Text = "Filtrar por Estado:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(540, 30), ForeColor = Theme.TextDark };
            cbFiltroEstado = new ComboBox { Location = new Point(680, 27), Width = 150, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroEstado.Items.AddRange(new string[] { "Todos", "Suficiente", "Bajo Stock", "Sin Stock", "Caducado", "Por Caducar" });
            cbFiltroEstado.SelectedIndex = 0;
            cbFiltroEstado.SelectedIndexChanged += (s, e) => FiltrarDatos();

            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(850, 30), ForeColor = Theme.TextDark };
            txtBuscar = new TextBox { Location = new Point(930, 27), Width = 200, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnActualizar);
            topPanel.Controls.Add(lblFiltro);
            topPanel.Controls.Add(cbFiltroEstado);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 10, 15, 10) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
            
            btnExportar = new Button { Text = "📥 Exportar a Excel", Width = 180, Height = 40, Margin = new Padding(20, 0, 0, 0) };
            Theme.StyleButton(btnExportar, Color.Teal, Theme.TextLight, Theme.FontNormal);
            btnExportar.Click += BtnExportar_Click;

            bottomPanel.Controls.Add(lblConteo);
            bottomPanel.Controls.Add(btnExportar);

            dgvExistencias = new DataGridView();
            dgvExistencias.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvExistencias);
            dgvExistencias.CellFormatting += DgvExistencias_CellFormatting;

            this.Controls.Add(dgvExistencias);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void CargarDatos()
        {
            try
            {
                _datos = _productoRepo.ObtenerReporteExistencias();
                FiltrarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar existencias:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FiltrarDatos()
        {
            if (_datos == null) return;

            string filtroBusqueda = txtBuscar.Text.Trim().ToLower();
            string filtroEstado = cbFiltroEstado.SelectedItem.ToString();
            
            var filtrados = _datos.AsEnumerable();

            var configRepo = new ConfiguracionRepository();
            bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";

            if (!string.IsNullOrEmpty(filtroBusqueda))
            {
                filtrados = filtrados.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLower().Contains(filtroBusqueda)) || 
                    (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(filtroBusqueda)) ||
                    (p.Categoria != null && p.Categoria.ToLower().Contains(filtroBusqueda)) ||
                    (isFarmacia && p.SustanciaActiva != null && p.SustanciaActiva.ToLower().Contains(filtroBusqueda))
                );
            }

            if (filtroEstado != "Todos")
            {
                filtrados = filtrados.Where(p => p.Estado == filtroEstado);
            }

            var lista = filtrados.ToList();
            dgvExistencias.DataSource = lista;

            // Configuración visual
            dgvExistencias.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            
            if (dgvExistencias.Columns["CodigoBarras"] != null) dgvExistencias.Columns["CodigoBarras"].HeaderText = "Código de Barras";
            if (dgvExistencias.Columns["Nombre"] != null) 
            {
                dgvExistencias.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvExistencias.Columns["Nombre"].MinimumWidth = 150;
            }
            if (dgvExistencias.Columns["Descripcion"] != null) 
            {
                dgvExistencias.Columns["Descripcion"].HeaderText = "Descripción";
                dgvExistencias.Columns["Descripcion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvExistencias.Columns["Descripcion"].MinimumWidth = 200;
            }
            if (dgvExistencias.Columns["SustanciaActiva"] != null)
            {
                if (isFarmacia)
                {
                    dgvExistencias.Columns["SustanciaActiva"].HeaderText = "DCI / Compuesto";
                    dgvExistencias.Columns["SustanciaActiva"].Visible = true;
                }
                else
                {
                    dgvExistencias.Columns["SustanciaActiva"].Visible = false;
                }
            }
            if (dgvExistencias.Columns["StockActual"] != null) 
            {
                dgvExistencias.Columns["StockActual"].HeaderText = "Stock Actual";
                dgvExistencias.Columns["StockActual"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvExistencias.Columns["StockMinimo"] != null) 
            {
                dgvExistencias.Columns["StockMinimo"].HeaderText = "Stock Mínimo";
                dgvExistencias.Columns["StockMinimo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvExistencias.Columns["CostoInvertido"] != null) 
            {
                dgvExistencias.Columns["CostoInvertido"].HeaderText = "Costo Invertido";
                dgvExistencias.Columns["CostoInvertido"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvExistencias.Columns["CostoInvertido"].DefaultCellStyle.Format = "C2";
            }
            if (dgvExistencias.Columns["GananciaProyectada"] != null) 
            {
                dgvExistencias.Columns["GananciaProyectada"].HeaderText = "Ganancia Proyectada";
                dgvExistencias.Columns["GananciaProyectada"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvExistencias.Columns["GananciaProyectada"].DefaultCellStyle.Format = "C2";
            }
            if (dgvExistencias.Columns["Estado"] != null) 
            {
                dgvExistencias.Columns["Estado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvExistencias.Columns["Estado"].DefaultCellStyle.Font = new Font(dgvExistencias.Font, FontStyle.Bold);
            }
            if (dgvExistencias.Columns["NumeroLote"] != null) 
            {
                dgvExistencias.Columns["NumeroLote"].HeaderText = "Lote";
                dgvExistencias.Columns["NumeroLote"].Visible = isFarmacia;
                dgvExistencias.Columns["NumeroLote"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (dgvExistencias.Columns["StockActual"] != null) 
                {
                    dgvExistencias.Columns["NumeroLote"].DisplayIndex = dgvExistencias.Columns["StockActual"].DisplayIndex + 1;
                }
            }
            if (dgvExistencias.Columns["FechaCaducidad"] != null) 
            {
                dgvExistencias.Columns["FechaCaducidad"].HeaderText = "Caducidad";
                dgvExistencias.Columns["FechaCaducidad"].Visible = isFarmacia;
                dgvExistencias.Columns["FechaCaducidad"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvExistencias.Columns["FechaCaducidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (dgvExistencias.Columns["NumeroLote"] != null)
                {
                    dgvExistencias.Columns["FechaCaducidad"].DisplayIndex = dgvExistencias.Columns["NumeroLote"].DisplayIndex + 1;
                }
            }

            lblConteo.Text = $"Total de registros: {lista.Count}";
        }

        private void DgvExistencias_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvExistencias.Columns[e.ColumnIndex].Name == "Estado" && e.Value != null)
            {
                string estado = e.Value.ToString();
                if (estado == "Sin Stock")
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(231, 76, 60); // Rojo alizarin
                }
                else if (estado == "Bajo Stock")
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(243, 156, 18); // Naranja oscuro
                }
                else if (estado == "Suficiente")
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(46, 204, 113); // Verde esmeralda
                }
                else if (estado == "Caducado")
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(142, 68, 173); // Morado
                }
                else if (estado == "Por Caducar")
                {
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.BackColor = Color.FromArgb(241, 196, 15); // Amarillo
                }
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (dgvExistencias.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Archivos de Excel (*.xlsx)|*.xlsx", FileName = "ReporteExistencias.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Existencias");

                            // Cabeceras
                            int colIndex = 1;
                            foreach (DataGridViewColumn col in dgvExistencias.Columns)
                            {
                                if (col.Visible)
                                {
                                    worksheet.Cell(1, colIndex).Value = col.HeaderText;
                                    worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                                    worksheet.Cell(1, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                                    colIndex++;
                                }
                            }

                            // Filas
                            int rowIndex = 2;
                            foreach (DataGridViewRow row in dgvExistencias.Rows)
                            {
                                if (!row.IsNewRow)
                                {
                                    colIndex = 1;
                                    foreach (DataGridViewColumn col in dgvExistencias.Columns)
                                    {
                                        if (col.Visible)
                                        {
                                            var cellVal = row.Cells[col.Index].Value;
                                            var cell = worksheet.Cell(rowIndex, colIndex);

                                            if (cellVal != null)
                                            {
                                                if (col.Name == "CodigoBarras" || cellVal is string)
                                                {
                                                    cell.Style.NumberFormat.Format = "@";
                                                    cell.SetValue(cellVal.ToString());
                                                }
                                                else if (cellVal is decimal d)
                                                {
                                                    cell.SetValue(d);
                                                    if (col.DefaultCellStyle.Format == "C2")
                                                        cell.Style.NumberFormat.Format = "$#,##0.00";
                                                }
                                                else if (cellVal is int i)
                                                {
                                                    cell.SetValue(i);
                                                }
                                                else
                                                {
                                                    cell.SetValue(cellVal.ToString());
                                                }
                                            }
                                            
                                            // Aplicar colores según el estado
                                            if (col.Name == "Estado" && cellVal != null)
                                            {
                                                string estado = cellVal.ToString();
                                                cell.Style.Font.FontColor = XLColor.White;
                                                cell.Style.Font.Bold = true;
                                                
                                                if (estado == "Sin Stock")
                                                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E74C3C"); // Rojo
                                                else if (estado == "Bajo Stock")
                                                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F39C12"); // Naranja
                                                else if (estado == "Suficiente")
                                                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2ECC71"); // Verde
                                                else if (estado == "Caducado")
                                                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#8E44AD"); // Morado
                                                else if (estado == "Por Caducar")
                                                {
                                                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1C40F"); // Amarillo
                                                    cell.Style.Font.FontColor = XLColor.Black;
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
