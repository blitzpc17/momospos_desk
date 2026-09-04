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
        private List<VentaDetalladaDTO> _ventaDetallada;
        private List<CorteHistorialDTO> _historialCortes;

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
            
            var configRepo = new ConfiguracionRepository();
            bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";
            
            cbTipoReporte.Items.AddRange(new string[] { "Historial de Ventas", "Reporte de Venta Detallado", "Artículos Vendidos" });
            if (isFarmacia)
            {
                cbTipoReporte.Items.Add("Libro Controlados");
            }
            cbTipoReporte.Items.Add("Reporte de Caducidades");
            cbTipoReporte.Items.Add("Reporte de Cortes");

            cbTipoReporte.SelectedIndex = 0;
            cbTipoReporte.SelectedIndexChanged += (s, e) => GenerarReporte();

            dtpInicio = new DateTimePicker { Location = new Point(520, 35), Format = DateTimePickerFormat.Short, Font = Theme.FontNormal, Width = 120 };
            dtpFin = new DateTimePicker { Location = new Point(660, 35), Format = DateTimePickerFormat.Short, Font = Theme.FontNormal, Width = 120 };
            
            btnGenerar = new Button { Text = "Generar", Location = new Point(800, 32), Width = 100, Height = 40 };
            Theme.StyleButton(btnGenerar, Theme.PrimaryColor);
            btnGenerar.Click += (s, e) => GenerarReporte();



            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(new Label { Text = "Tipo:", Font = Theme.FontNormal, Location = new Point(350, 10), AutoSize = true, ForeColor = Theme.TextDark });
            topPanel.Controls.Add(cbTipoReporte);
            topPanel.Controls.Add(new Label { Text = "Desde:", Font = Theme.FontNormal, Location = new Point(520, 10), AutoSize = true, ForeColor = Theme.TextDark });
            topPanel.Controls.Add(dtpInicio);
            topPanel.Controls.Add(new Label { Text = "Hasta:", Font = Theme.FontNormal, Location = new Point(660, 10), AutoSize = true, ForeColor = Theme.TextDark });
            topPanel.Controls.Add(dtpFin);
            topPanel.Controls.Add(btnGenerar);


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
            dgvHistorial.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.CellDoubleClick += DgvHistorial_CellDoubleClick;

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
                txtBuscar.TextChanged -= TxtBuscar_TextChanged;
                
                foreach (DataGridViewColumn col in dgvHistorial.Columns) col.Frozen = false;
                dgvHistorial.DataSource = null;
                dgvHistorial.Columns.Clear();

                string tipoReporte = cbTipoReporte.SelectedItem?.ToString();

                if (tipoReporte == "Reporte de Venta Detallado")
                {

                    _ventaDetallada = _ventaRepo.ObtenerReporteVentaDetallado(dtpInicio.Value, dtpFin.Value);

                    decimal sumaCosto = 0;
                    decimal sumaVenta = 0;
                    foreach (var v in _ventaDetallada) 
                    {
                        sumaCosto += v.TotalCosto;
                        sumaVenta += v.TotalVenta;
                    }

                    lblTotalVendido.Text = sumaVenta.ToString("C");
                    lblTotalEfectivo.Text = sumaCosto.ToString("C"); // Usamos este espacio para mostrar Total Costo temporalmente
                    lblTotalTarjeta.Text = (sumaVenta - sumaCosto).ToString("C"); // Ganancia bruta

                    // Cambiamos titulos de tarjetas si queremos ser específicos
                    // pero para no romper el resto de reportes, lo mantenemos simple.

                    txtBuscar.Text = "";
                    AplicarFiltro();

                    if (dgvHistorial.Columns["Folio"] != null) dgvHistorial.Columns["Folio"].HeaderText = "FOLIO";
                    if (dgvHistorial.Columns["Fecha"] != null) { dgvHistorial.Columns["Fecha"].HeaderText = "FECHA"; dgvHistorial.Columns["Fecha"].DefaultCellStyle.Format = "dd.MM.yyyy"; }
                    if (dgvHistorial.Columns["Hora"] != null) dgvHistorial.Columns["Hora"].HeaderText = "HORA";
                    if (dgvHistorial.Columns["CodigoBarras"] != null) dgvHistorial.Columns["CodigoBarras"].HeaderText = "Codigo de Barras";
                    if (dgvHistorial.Columns["Nombre"] != null) { dgvHistorial.Columns["Nombre"].HeaderText = "Nombre"; dgvHistorial.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }
                    if (dgvHistorial.Columns["Descripcion"] != null) { dgvHistorial.Columns["Descripcion"].HeaderText = "Descripcion"; dgvHistorial.Columns["Descripcion"].Visible = false; /* Oculta en tabla por espacio, pero Excel la exporta si la hacemos visible. Vamos a dejarla visible para que se exporte. */ dgvHistorial.Columns["Descripcion"].Visible = true; }
                    if (dgvHistorial.Columns["Categoria"] != null) dgvHistorial.Columns["Categoria"].HeaderText = "Categoria";
                    if (dgvHistorial.Columns["UnidadMedida"] != null) dgvHistorial.Columns["UnidadMedida"].HeaderText = "unid. Med.";
                    if (dgvHistorial.Columns["Servicio"] != null) dgvHistorial.Columns["Servicio"].HeaderText = "Servicio";
                    if (dgvHistorial.Columns["Cantidad"] != null) { dgvHistorial.Columns["Cantidad"].HeaderText = "cantidad"; dgvHistorial.Columns["Cantidad"].DefaultCellStyle.Format = "N2"; }
                    if (dgvHistorial.Columns["PrecioCosto"] != null) { dgvHistorial.Columns["PrecioCosto"].HeaderText = "Precio Costo"; dgvHistorial.Columns["PrecioCosto"].DefaultCellStyle.Format = "C2"; }
                    if (dgvHistorial.Columns["TotalCosto"] != null) { dgvHistorial.Columns["TotalCosto"].HeaderText = "Total Costo"; dgvHistorial.Columns["TotalCosto"].DefaultCellStyle.Format = "C2"; }
                    if (dgvHistorial.Columns["PrecioNormal"] != null) { dgvHistorial.Columns["PrecioNormal"].HeaderText = "Precio Catálogo"; dgvHistorial.Columns["PrecioNormal"].DefaultCellStyle.Format = "C2"; dgvHistorial.Columns["PrecioNormal"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; }
                    if (dgvHistorial.Columns["DescuentoMayoreo"] != null) { dgvHistorial.Columns["DescuentoMayoreo"].HeaderText = "Desc. Mayoreo"; dgvHistorial.Columns["DescuentoMayoreo"].DefaultCellStyle.Format = "C2"; dgvHistorial.Columns["DescuentoMayoreo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; }
                    if (dgvHistorial.Columns["DescuentoManual"] != null) { dgvHistorial.Columns["DescuentoManual"].HeaderText = "Desc. Manual"; dgvHistorial.Columns["DescuentoManual"].DefaultCellStyle.Format = "C2"; dgvHistorial.Columns["DescuentoManual"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; }
                    if (dgvHistorial.Columns["PrecioVenta"] != null) { dgvHistorial.Columns["PrecioVenta"].HeaderText = "Precio Final Un."; dgvHistorial.Columns["PrecioVenta"].DefaultCellStyle.Format = "C2"; dgvHistorial.Columns["PrecioVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; }
                    if (dgvHistorial.Columns["TotalVenta"] != null) { dgvHistorial.Columns["TotalVenta"].HeaderText = "Total Venta"; dgvHistorial.Columns["TotalVenta"].DefaultCellStyle.Format = "C2"; dgvHistorial.Columns["TotalVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; }
                }
                else if (tipoReporte == "Artículos Vendidos")
                {

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
                    if (dgvHistorial.Columns["SustanciaActiva"] != null)
                    {
                        var configRepo = new ConfiguracionRepository();
                        bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";
                        dgvHistorial.Columns["SustanciaActiva"].HeaderText = "DCI / Compuesto";
                        dgvHistorial.Columns["SustanciaActiva"].Visible = isFarmacia;
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
                else if (tipoReporte == "Libro Controlados")
                {

                    var reporte = _ventaRepo.ObtenerReporteMedicamentosControlados(dtpInicio.Value, dtpFin.Value);
                    
                    lblTotalVendido.Text = "N/A";
                    lblTotalEfectivo.Text = "N/A";
                    lblTotalTarjeta.Text = "N/A";

                    txtBuscar.Text = "";
                    dgvHistorial.DataSource = null;
                    dgvHistorial.DataSource = reporte;
                    lblConteo.Text = $"Total de registros: {reporte.Count}";
                    
                    if (dgvHistorial.Columns["NombreProducto"] != null) dgvHistorial.Columns["NombreProducto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    if (dgvHistorial.Columns["FechaVenta"] != null) dgvHistorial.Columns["FechaVenta"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
                else if (tipoReporte == "Reporte de Caducidades")
                {

                    var prodRepo = new ProductoRepository();
                    var reporte = prodRepo.ObtenerReporteCaducidades();
                    
                    lblTotalVendido.Text = "N/A";
                    lblTotalEfectivo.Text = "N/A";
                    lblTotalTarjeta.Text = "N/A";

                    txtBuscar.Text = "";
                    dgvHistorial.DataSource = null;
                    dgvHistorial.DataSource = reporte;
                    lblConteo.Text = $"Total de lotes activos: {reporte.Count}";

                    if (dgvHistorial.Columns["Nombre"] != null) dgvHistorial.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    if (dgvHistorial.Columns["CostoInvertido"] != null) dgvHistorial.Columns["CostoInvertido"].DefaultCellStyle.Format = "C2";
                    if (dgvHistorial.Columns["GananciaProyectada"] != null) dgvHistorial.Columns["GananciaProyectada"].DefaultCellStyle.Format = "C2";
                    if (dgvHistorial.Columns["FechaCaducidad"] != null) dgvHistorial.Columns["FechaCaducidad"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                else if (tipoReporte == "Reporte de Cortes")
                {
                    var cajaRepo = new CajaRepository();
                    _historialCortes = cajaRepo.ObtenerReporteCortes(dtpInicio.Value, dtpFin.Value);
                    
                    lblTotalVendido.Text = "N/A";
                    lblTotalEfectivo.Text = "N/A";
                    lblTotalTarjeta.Text = "N/A";

                    txtBuscar.Text = "";
                    dgvHistorial.DataSource = null;
                    dgvHistorial.DataSource = _historialCortes;
                    lblConteo.Text = $"Total de cortes: {_historialCortes.Count}";
                    
                    if (dgvHistorial.Columns["SesionId"] != null) dgvHistorial.Columns["SesionId"].HeaderText = "ID";
                    if (dgvHistorial.Columns["CajaId"] != null) dgvHistorial.Columns["CajaId"].HeaderText = "Caja";
                    if (dgvHistorial.Columns["NombreCajero"] != null) { dgvHistorial.Columns["NombreCajero"].HeaderText = "Cajero"; dgvHistorial.Columns["NombreCajero"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }
                    if (dgvHistorial.Columns["FechaApertura"] != null) dgvHistorial.Columns["FechaApertura"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    if (dgvHistorial.Columns["FechaCierre"] != null) dgvHistorial.Columns["FechaCierre"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    if (dgvHistorial.Columns["FondoInicial"] != null) dgvHistorial.Columns["FondoInicial"].DefaultCellStyle.Format = "C2";
                    if (dgvHistorial.Columns["EfectivoEsperado"] != null) dgvHistorial.Columns["EfectivoEsperado"].DefaultCellStyle.Format = "C2";
                    if (dgvHistorial.Columns["EfectivoContado"] != null) dgvHistorial.Columns["EfectivoContado"].DefaultCellStyle.Format = "C2";
                    if (dgvHistorial.Columns["Diferencia"] != null) dgvHistorial.Columns["Diferencia"].DefaultCellStyle.Format = "C2";
                }
                else // Historial de Ventas
                {

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
                    if (dgvHistorial.Columns["DescuentoTotal"] != null) { dgvHistorial.Columns["DescuentoTotal"].HeaderText = "Descuento"; dgvHistorial.Columns["DescuentoTotal"].DefaultCellStyle.Format = "C2"; }
                }

                ActualizarComboFiltro();

                ActualizarComboFiltro();

                txtBuscar.TextChanged -= TxtBuscar_TextChanged;
                txtBuscar.TextChanged += TxtBuscar_TextChanged;

                AjustarFormatoYCongelar();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show($"Error al generar reporte:\n{ex.Message}");
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
            AjustarFormatoYCongelar();
        }

        private void AjustarFormatoYCongelar()
        {
            foreach (DataGridViewColumn col in dgvHistorial.Columns)
            {
                // Alineaciones solicitadas: Numeros a la izquierda, lo demas a la derecha
                if (col.ValueType == typeof(decimal) || col.ValueType == typeof(int) || col.ValueType == typeof(long) || col.ValueType == typeof(double) || col.ValueType == typeof(float))
                {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    if (col.ValueType == typeof(decimal) && string.IsNullOrEmpty(col.DefaultCellStyle.Format))
                    {
                        col.DefaultCellStyle.Format = "N2";
                    }
                }
                else
                {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // Ajustar columnas al contenido (si no estan ya como Fill)
                if (col.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }
            }

            // Descongelar todas antes de volver a congelar
            foreach (DataGridViewColumn col in dgvHistorial.Columns) col.Frozen = false;
            
            // Congelar solo la primera columna visible para no estropear el scroll
            foreach (DataGridViewColumn col in dgvHistorial.Columns)
            {
                if (col.Visible)
                {
                    col.Frozen = true;
                    break;
                }
            }
        }

        private void AplicarFiltro()
        {
            foreach (DataGridViewColumn col in dgvHistorial.Columns) col.Frozen = false;
            
            string query = txtBuscar.Text.ToLower().Trim();
            string columnaFiltro = cbFiltroColumna.SelectedItem?.ToString() ?? "Todas las columnas";
            string tipoReporte = cbTipoReporte.SelectedItem?.ToString();
            
            if (tipoReporte == "Reporte de Venta Detallado")
            {
                if (_ventaDetallada == null) return;
                
                if (string.IsNullOrEmpty(query))
                {
                    dgvHistorial.DataSource = _ventaDetallada;
                    lblConteo.Text = $"Total de registros: {_ventaDetallada.Count}";
                }
                else
                {
                    var filtrados = _ventaDetallada.FindAll(x => 
                        (x.Folio != null && x.Folio.ToLower().Contains(query)) ||
                        (x.Nombre != null && x.Nombre.ToLower().Contains(query)) ||
                        (x.CodigoBarras != null && x.CodigoBarras.ToLower().Contains(query)) ||
                        (x.Categoria != null && x.Categoria.ToLower().Contains(query))
                    );
                    dgvHistorial.DataSource = filtrados;
                    lblConteo.Text = $"Total de registros: {filtrados.Count} (Filtrados)";
                }
            }
            else if (tipoReporte == "Artículos Vendidos")
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
                            (x.Categoria != null && x.Categoria.ToLower().Contains(query)) ||
                            (x.SustanciaActiva != null && x.SustanciaActiva.ToLower().Contains(query))
                        )) ||
                        (columnaFiltro == "CodigoBarras" && x.CodigoBarras != null && x.CodigoBarras.ToLower().Contains(query)) ||
                        (columnaFiltro == "Nombre" && x.Nombre != null && x.Nombre.ToLower().Contains(query)) ||
                        (columnaFiltro == "DCI / Compuesto" && x.SustanciaActiva != null && x.SustanciaActiva.ToLower().Contains(query)) ||
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
            else if (tipoReporte == "Reporte de Cortes")
            {
                if (_historialCortes == null) return;
                
                if (string.IsNullOrEmpty(query))
                {
                    dgvHistorial.DataSource = _historialCortes;
                    lblConteo.Text = $"Total de cortes: {_historialCortes.Count}";
                }
                else
                {
                    var filtrados = _historialCortes.FindAll(x => 
                        (x.NombreCajero != null && x.NombreCajero.ToLower().Contains(query)) ||
                        (x.CajaId.ToString().Contains(query)) ||
                        (x.FechaApertura.ToString().ToLower().Contains(query)) ||
                        (x.FechaCierre.HasValue && x.FechaCierre.Value.ToString().ToLower().Contains(query))
                    );
                    dgvHistorial.DataSource = filtrados;
                    lblConteo.Text = $"Total de cortes: {filtrados.Count} (Filtrados)";
                }
            }
            else if (tipoReporte == "Historial de Ventas")
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
            // Para Libro Controlados y Caducidades, se podría implementar filtro manual similar,
            // pero como usan DataSource dinámico, el usuario puede exportarlos o usar el grid directamente si agregamos la logica.
            // Por simplicidad, si seleccionan esos reportes y buscan, ignoramos por ahora a menos que lo pidan.
        }

        private void DgvHistorial_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (cbTipoReporte.SelectedItem?.ToString() == "Historial de Ventas")
                {
                    var row = dgvHistorial.Rows[e.RowIndex];
                    if (row.DataBoundItem is Venta ventaItem)
                    {
                        try
                        {
                            var ventaCompleta = _ventaRepo.ObtenerVentaPorId(ventaItem.Id);
                            if (ventaCompleta != null)
                            {
                                var dialog = new Dialogs.VentaDetalleForm(ventaCompleta);
                                dialog.ShowDialog();
                            }
                        }
                        catch (Exception ex)
                        {
                            momospos.Views.CustomMessageBox.Show("No se pudo cargar el detalle de la venta. " + ex.Message);
                        }
                    }
                }
                else if (cbTipoReporte.SelectedItem?.ToString() == "Reporte de Cortes")
                {
                    var row = dgvHistorial.Rows[e.RowIndex];
                    if (row.DataBoundItem is CorteHistorialDTO corteItem)
                    {
                        var dlg = momospos.Views.CustomMessageBox.Show($"¿Deseas reimprimir el ticket del corte de la caja {corteItem.CajaId} del día {corteItem.FechaCierre:dd/MM/yyyy}?", "Reimprimir Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dlg == DialogResult.Yes)
                        {
                            try
                            {
                                var cajaRepo = new CajaRepository();
                                var sesion = cajaRepo.ObtenerSesionPorId(corteItem.SesionId);
                                if (sesion != null)
                                {
                                    var printer = new CortePrinter(sesion, corteItem.NombreCajero, false);
                                    printer.Imprimir();
                                }
                            }
                            catch (Exception ex)
                            {
                                momospos.Views.CustomMessageBox.Show("Error al reimprimir corte: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.Rows.Count == 0)
            {
                momospos.Views.CustomMessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                            int totalCostoColIndex = -1;
                            int totalVentaColIndex = -1;

                            foreach (DataGridViewColumn col in dgvHistorial.Columns)
                            {
                                if (col.Visible)
                                {
                                    worksheet.Cell(1, colIndex).Value = col.HeaderText;
                                    worksheet.Cell(1, colIndex).Style.Font.Bold = true;

                                    if (col.Name == "TotalCosto") totalCostoColIndex = colIndex;
                                    if (col.Name == "TotalVenta") totalVentaColIndex = colIndex;

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
                                                    else
                                                        worksheet.Cell(rowIndex, colIndex).Style.NumberFormat.Format = "#,##0.00"; // Fallback para cualquier otro decimal como cantidades
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

                            if (rowIndex > 2)
                            {
                                if (totalCostoColIndex != -1)
                                {
                                    var cell = worksheet.Cell(rowIndex, totalCostoColIndex);
                                    string colLetter = worksheet.Column(totalCostoColIndex).ColumnLetter();
                                    cell.FormulaA1 = $"SUM({colLetter}2:{colLetter}{rowIndex - 1})";
                                    cell.Style.Font.Bold = true;
                                    cell.Style.NumberFormat.Format = "$#,##0.00";
                                    cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Double;
                                }
                                if (totalVentaColIndex != -1)
                                {
                                    var cell = worksheet.Cell(rowIndex, totalVentaColIndex);
                                    string colLetter = worksheet.Column(totalVentaColIndex).ColumnLetter();
                                    cell.FormulaA1 = $"SUM({colLetter}2:{colLetter}{rowIndex - 1})";
                                    cell.Style.Font.Bold = true;
                                    cell.Style.NumberFormat.Format = "$#,##0.00";
                                    cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Double;
                                }
                            }

                            worksheet.SheetView.FreezeRows(1);
                            worksheet.SheetView.FreezeColumns(2);
                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                        }

                        momospos.Views.CustomMessageBox.Show("Archivo exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        momospos.Views.CustomMessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


    }
}
