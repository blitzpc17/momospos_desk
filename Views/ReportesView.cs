using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using System.Collections.Generic;
using System.IO;
using System.Text;
using momospos.Models;
using Microsoft.VisualBasic;
using ClosedXML.Excel;
namespace momospos.Views
{
    public class ReportesView : UserControl
    {
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFin;
        private ComboBox cbTipoReporte;
        private Button btnGenerar;
        private Button btnSolicitarCancelacion;

        private Label lblTotalVendido;
        private Label lblTotalEfectivo;
        private Label lblTotalTarjeta;
        private DataGridView dgvHistorial;
        private Label lblConteo;
        
        private TextBox txtBuscar;
        private ComboBox cbFiltroColumna;
        private Button btnExportar;
        
        private List<ArticuloVendidoDTO> _articulosVendidos;
        private List<Venta> _historialVentas;

        private VentaRepository _ventaRepo;
        private Usuario _usuarioActual;

        public ReportesView(Usuario usuarioActual)
        {
            _usuarioActual = usuarioActual;
            _ventaRepo = new VentaRepository();
            BuildUI();
            GenerarReporte(); // Cargar datos de hoy al iniciar
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // HEADER Y FILTROS
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "📊 Reportes y Estadísticas", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            
            cbTipoReporte = new ComboBox { Location = new Point(350, 35), Width = 150, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipoReporte.Items.AddRange(new string[] { "Historial de Ventas", "Artículos Vendidos" });
            cbTipoReporte.SelectedIndex = 0;
            cbTipoReporte.SelectedIndexChanged += (s, e) => GenerarReporte();

            dtpInicio = new DateTimePicker { Location = new Point(520, 35), Format = DateTimePickerFormat.Short, Font = Theme.FontNormal, Width = 120 };
            dtpFin = new DateTimePicker { Location = new Point(660, 35), Format = DateTimePickerFormat.Short, Font = Theme.FontNormal, Width = 120 };
            
            btnGenerar = new Button { Text = "Generar", Location = new Point(800, 32), Width = 100, Height = 40 };
            Theme.StyleButton(btnGenerar, Theme.PrimaryColor);
            btnGenerar.Click += (s, e) => GenerarReporte();

            btnSolicitarCancelacion = new Button { Text = "❌ Solicitar Cancelación", Location = new Point(910, 32), Width = 180, Height = 40 };
            Theme.StyleButton(btnSolicitarCancelacion, Theme.DangerColor);
            btnSolicitarCancelacion.Click += BtnSolicitarCancelacion_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(new Label { Text = "Tipo:", Font = Theme.FontNormal, Location = new Point(350, 10), AutoSize = true, ForeColor = Theme.TextDark });
            topPanel.Controls.Add(cbTipoReporte);
            topPanel.Controls.Add(new Label { Text = "Desde:", Font = Theme.FontNormal, Location = new Point(520, 10), AutoSize = true, ForeColor = Theme.TextDark });
            topPanel.Controls.Add(dtpInicio);
            topPanel.Controls.Add(new Label { Text = "Hasta:", Font = Theme.FontNormal, Location = new Point(660, 10), AutoSize = true, ForeColor = Theme.TextDark });
            topPanel.Controls.Add(dtpFin);
            topPanel.Controls.Add(btnGenerar);
            topPanel.Controls.Add(btnSolicitarCancelacion);

            // CARJETAS DE RESUMEN
            Panel cardsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(20) };
            
            Panel cardVendido = CrearTarjeta("Total Vendido", Theme.PrimaryColor, out lblTotalVendido);
            cardVendido.Location = new Point(20, 10);
            
            Panel cardEfectivo = CrearTarjeta("En Efectivo", Theme.SuccessColor, out lblTotalEfectivo);
            cardEfectivo.Location = new Point(280, 10);

            Panel cardTarjeta = CrearTarjeta("En Tarjeta", Color.FromArgb(243, 156, 18), out lblTotalTarjeta); // Naranja
            cardTarjeta.Location = new Point(540, 10);

            cardsPanel.Controls.Add(cardVendido);
            cardsPanel.Controls.Add(cardEfectivo);
            cardsPanel.Controls.Add(cardTarjeta);

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(15, 10, 15, 10), WrapContents = true };
            
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 12, 20, 0) };
            
            btnExportar = new Button { Text = "📥 Exportar a Excel", Width = 180, Height = 40, Margin = new Padding(0, 0, 20, 0) };
            Theme.StyleButton(btnExportar, Color.Teal, Theme.TextLight, Theme.FontNormal);
            btnExportar.Click += BtnExportar_Click;

            Label lblBuscar = new Label { Text = "🔍 Buscar en:", Font = Theme.FontNormal, AutoSize = true, Margin = new Padding(0, 12, 5, 0) };
            cbFiltroColumna = new ComboBox { Width = 140, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 8, 5, 0) };
            txtBuscar = new TextBox { Width = 200, Font = new Font("Segoe UI", 12), Margin = new Padding(0, 7, 0, 0) };
            txtBuscar.TextChanged += TxtBuscar_TextChanged;

            bottomPanel.Controls.Add(lblConteo);
            bottomPanel.Controls.Add(btnExportar);
            bottomPanel.Controls.Add(lblBuscar);
            bottomPanel.Controls.Add(cbFiltroColumna);
            bottomPanel.Controls.Add(txtBuscar);

            // TABLA DE DETALLES
            dgvHistorial = new DataGridView();
            dgvHistorial.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvHistorial);
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvHistorial);

            this.Controls.Add(marginPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(cardsPanel);
            this.Controls.Add(topPanel);
        }

        private Panel CrearTarjeta(string titulo, Color color, out Label valorLabel)
        {
            Panel p = new Panel { Width = 240, Height = 100, BackColor = Color.White };
            p.BorderStyle = BorderStyle.FixedSingle;

            Panel pTop = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = color };
            Label lTitulo = new Label { Text = titulo, Font = Theme.FontNormal, ForeColor = Color.Gray, AutoSize = true, Location = new Point(15, 20) };
            valorLabel = new Label { Text = "$0.00", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(15, 50) };

            p.Controls.Add(pTop);
            p.Controls.Add(lTitulo);
            p.Controls.Add(valorLabel);
            return p;
        }

        private void GenerarReporte()
        {
            try
            {
                if (cbTipoReporte.SelectedIndex == 1) // Artículos Vendidos
                {
                    btnSolicitarCancelacion.Visible = false;
                    _articulosVendidos = _ventaRepo.ObtenerArticulosVendidosPorPeriodo(dtpInicio.Value, dtpFin.Value);

                    decimal sumaGenerado = 0;
                    foreach (var a in _articulosVendidos) sumaGenerado += a.TotalGenerado;

                    lblTotalVendido.Text = sumaGenerado.ToString("C");
                    lblTotalEfectivo.Text = "N/A";
                    lblTotalTarjeta.Text = "N/A";

                    txtBuscar.Text = ""; // Limpiar busqueda
                    AplicarFiltro();

                    if (dgvHistorial.Columns["CantidadTotal"] != null)
                    {
                        dgvHistorial.Columns["CantidadTotal"].HeaderText = "Cant. Vendida";
                        dgvHistorial.Columns["CantidadTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["CantidadTotal"].DefaultCellStyle.Format = "N2";
                    }
                    if (dgvHistorial.Columns["PrecioCompraUnitario"] != null)
                    {
                        dgvHistorial.Columns["PrecioCompraUnitario"].HeaderText = "Precio Compra";
                        dgvHistorial.Columns["PrecioCompraUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["PrecioCompraUnitario"].DefaultCellStyle.Format = "C2";
                    }
                    if (dgvHistorial.Columns["PrecioVentaUnitario"] != null)
                    {
                        dgvHistorial.Columns["PrecioVentaUnitario"].HeaderText = "Precio Venta";
                        dgvHistorial.Columns["PrecioVentaUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["PrecioVentaUnitario"].DefaultCellStyle.Format = "C2";
                    }
                    if (dgvHistorial.Columns["TotalGenerado"] != null)
                    {
                        dgvHistorial.Columns["TotalGenerado"].HeaderText = "Total Generado";
                        dgvHistorial.Columns["TotalGenerado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["TotalGenerado"].DefaultCellStyle.Format = "C2";
                    }
                    if (dgvHistorial.Columns["Ganancia"] != null)
                    {
                        dgvHistorial.Columns["Ganancia"].HeaderText = "Ganancia";
                        dgvHistorial.Columns["Ganancia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["Ganancia"].DefaultCellStyle.Format = "C2";
                    }
                    if (dgvHistorial.Columns["Categoria"] != null)
                        dgvHistorial.Columns["Categoria"].HeaderText = "Categoría";
                    
                    if (dgvHistorial.Columns["Nombre"] != null)
                        dgvHistorial.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                else // Historial de Ventas
                {
                    btnSolicitarCancelacion.Visible = true;
                    var reporte = _ventaRepo.ObtenerReporteVentas(dtpInicio.Value, dtpFin.Value);

                    lblTotalVendido.Text = reporte.TotalVendido.ToString("C");
                    lblTotalEfectivo.Text = reporte.TotalEfectivo.ToString("C");
                    lblTotalTarjeta.Text = reporte.TotalTarjeta.ToString("C");

                    _historialVentas = reporte.Historial;
                    txtBuscar.Text = ""; // Limpiar busqueda
                    AplicarFiltro();
                    
                    if (dgvHistorial.Columns["Id"] != null) dgvHistorial.Columns["Id"].Visible = false;
                    if (dgvHistorial.Columns["CajaSesionId"] != null) dgvHistorial.Columns["CajaSesionId"].Visible = false;
                    if (dgvHistorial.Columns["UsuarioId"] != null) dgvHistorial.Columns["UsuarioId"].Visible = false;
                    if (dgvHistorial.Columns["ClienteId"] != null) dgvHistorial.Columns["ClienteId"].Visible = false;
                }

                ActualizarComboFiltro();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte:\n{ex.Message}");
            }
        }

        private void ActualizarComboFiltro()
        {
            // Evitamos disparar el evento de SelectedIndexChanged
            cbFiltroColumna.SelectedIndexChanged -= CbFiltroColumna_SelectedIndexChanged;
            
            string seleccionado = cbFiltroColumna.SelectedItem?.ToString();
            cbFiltroColumna.Items.Clear();
            cbFiltroColumna.Items.Add("Todas las columnas");

            foreach (DataGridViewColumn col in dgvHistorial.Columns)
            {
                if (col.Visible)
                {
                    cbFiltroColumna.Items.Add(col.HeaderText);
                }
            }

            if (seleccionado != null && cbFiltroColumna.Items.Contains(seleccionado))
                cbFiltroColumna.SelectedItem = seleccionado;
            else
                cbFiltroColumna.SelectedIndex = 0;
                
            cbFiltroColumna.SelectedIndexChanged += CbFiltroColumna_SelectedIndexChanged;
        }

        private void CbFiltroColumna_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string query = txtBuscar.Text.ToLower().Trim();
            string columnaFiltro = cbFiltroColumna.SelectedItem?.ToString() ?? "Todas las columnas";
            
            if (cbTipoReporte.SelectedIndex == 1) // Artículos Vendidos
            {
                if (_articulosVendidos == null) return;
                
                if (string.IsNullOrEmpty(query))
                {
                    dgvHistorial.DataSource = _articulosVendidos;
                    lblConteo.Text = $"Total de artículos diferentes: {_articulosVendidos.Count}";
                }
                else
                {
                    var filtrados = _articulosVendidos.FindAll(x => 
                        (columnaFiltro == "Todas las columnas" && (
                            (x.CodigoBarras != null && x.CodigoBarras.ToLower().Contains(query)) ||
                            (x.Nombre != null && x.Nombre.ToLower().Contains(query)) ||
                            (x.Categoria != null && x.Categoria.ToLower().Contains(query))
                        )) ||
                        (columnaFiltro == "CodigoBarras" && x.CodigoBarras != null && x.CodigoBarras.ToLower().Contains(query)) ||
                        (columnaFiltro == "Nombre" && x.Nombre != null && x.Nombre.ToLower().Contains(query)) ||
                        (columnaFiltro == "Categoría" && x.Categoria != null && x.Categoria.ToLower().Contains(query)) ||
                        (columnaFiltro == "Cant. Vendida" && x.CantidadTotal.ToString().Contains(query)) ||
                        (columnaFiltro == "Precio Compra" && x.PrecioCompraUnitario.ToString().Contains(query)) ||
                        (columnaFiltro == "Precio Venta" && x.PrecioVentaUnitario.ToString().Contains(query)) ||
                        (columnaFiltro == "Total Generado" && x.TotalGenerado.ToString().Contains(query)) ||
                        (columnaFiltro == "Ganancia" && x.Ganancia.ToString().Contains(query))
                    );
                    dgvHistorial.DataSource = filtrados;
                    lblConteo.Text = $"Total de artículos diferentes: {filtrados.Count} (Filtrados)";
                }
            }
            else // Historial Ventas
            {
                if (_historialVentas == null) return;

                if (string.IsNullOrEmpty(query))
                {
                    dgvHistorial.DataSource = _historialVentas;
                    lblConteo.Text = $"Total de ventas: {_historialVentas.Count}";
                }
                else
                {
                    var filtrados = _historialVentas.FindAll(x => 
                        (columnaFiltro == "Todas las columnas" && (
                            (x.Folio != null && x.Folio.ToLower().Contains(query)) ||
                            (x.Estado != null && x.Estado.ToLower().Contains(query)) ||
                            x.Total.ToString().Contains(query)
                        )) ||
                        (columnaFiltro == "Folio" && x.Folio != null && x.Folio.ToLower().Contains(query)) ||
                        (columnaFiltro == "Fecha" && x.Fecha.ToString().ToLower().Contains(query)) ||
                        (columnaFiltro == "Total" && x.Total.ToString().Contains(query)) ||
                        (columnaFiltro == "Pagado" && x.Pagado.ToString().Contains(query)) ||
                        (columnaFiltro == "Cambio" && x.Cambio.ToString().Contains(query)) ||
                        (columnaFiltro == "Estado" && x.Estado != null && x.Estado.ToLower().Contains(query))
                    );
                    dgvHistorial.DataSource = filtrados;
                    lblConteo.Text = $"Total de ventas: {filtrados.Count} (Filtradas)";
                }
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Archivos de Excel (*.xlsx)|*.xlsx", FileName = "Reporte.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Reporte");

                            // Cabeceras
                            int colIndex = 1;
                            foreach (DataGridViewColumn col in dgvHistorial.Columns)
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
                            foreach (DataGridViewRow row in dgvHistorial.Rows)
                            {
                                if (!row.IsNewRow)
                                {
                                    colIndex = 1;
                                    foreach (DataGridViewColumn col in dgvHistorial.Columns)
                                    {
                                        if (col.Visible)
                                        {
                                            var cellVal = row.Cells[col.Index].Value;
                                            
                                            if (cellVal != null)
                                            {
                                                if (col.Name == "CodigoBarras" || col.Name == "Folio" || cellVal is string)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).Style.NumberFormat.Format = "@";
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(cellVal.ToString());
                                                }
                                                else if (cellVal is decimal d)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(d);
                                                    if (col.DefaultCellStyle.Format == "C2")
                                                        worksheet.Cell(rowIndex, colIndex).Style.NumberFormat.Format = "$#,##0.00";
                                                    else if (col.DefaultCellStyle.Format == "N2")
                                                        worksheet.Cell(rowIndex, colIndex).Style.NumberFormat.Format = "#,##0.00";
                                                }
                                                else if (cellVal is int i)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(i);
                                                }
                                                else if (cellVal is DateTime dt)
                                                {
                                                    worksheet.Cell(rowIndex, colIndex).SetValue(dt);
                                                    worksheet.Cell(rowIndex, colIndex).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
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

        private void BtnSolicitarCancelacion_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.CurrentRow == null || !(dgvHistorial.CurrentRow.DataBoundItem is Venta venta))
            {
                MessageBox.Show("Por favor, seleccione una venta del historial.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (venta.Estado != "CONFIRMADO")
            {
                MessageBox.Show("Solo se pueden cancelar ventas que estén CONFIRMADAS.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string motivo = Interaction.InputBox($"Ingrese el motivo de la cancelación para la venta {venta.Folio}:", "Solicitar Cancelación", "");
            
            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("Debe ingresar un motivo para poder solicitar la cancelación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _ventaRepo.SolicitarCancelacionVenta(venta.Id, _usuarioActual.Id, motivo);
                MessageBox.Show("Solicitud de cancelación enviada correctamente. Esperando autorización.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GenerarReporte(); // Refrescar historial
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al solicitar cancelación:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
