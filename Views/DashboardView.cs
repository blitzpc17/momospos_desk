using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using momospos.Repositories;

namespace momospos.Views
{
    public class DashboardView : UserControl
    {
        private DashboardRepository _dashboardRepo;
        private FlowLayoutPanel _cardsPanel;
        private TableLayoutPanel _contentTable;
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFin;

        private Chart _chartMasVendidos;
        private DataGridView _dgvMenosVendidos;
        private DataGridView _dgvStockBajo;
        private DataGridView _dgvProximosCaducar;

        public DashboardView()
        {
            _dashboardRepo = new DashboardRepository();
            BuildUI();
            CargarMetricas();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "📈 Resumen del Negocio", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            Label lblInicio = new Label { Text = "Desde:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(320, 30), ForeColor = Theme.TextDark };
            dtpInicio = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(380, 27), Width = 110, Font = Theme.FontNormal, Value = DateTime.Today };
            
            Label lblFin = new Label { Text = "Hasta:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(510, 30), ForeColor = Theme.TextDark };
            dtpFin = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(560, 27), Width = 110, Font = Theme.FontNormal, Value = DateTime.Today };

            Button btnActualizar = new Button { Text = "🔄 Filtrar", Location = new Point(690, 20), Width = 100, Height = 40 };
            Theme.StyleButton(btnActualizar, Theme.SecondaryColor);
            btnActualizar.Click += (s, e) => CargarMetricas();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblInicio);
            topPanel.Controls.Add(dtpInicio);
            topPanel.Controls.Add(lblFin);
            topPanel.Controls.Add(dtpFin);
            topPanel.Controls.Add(btnActualizar);

            _cardsPanel = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Height = 160,
                Padding = new Padding(15),
                AutoScroll = true,
                WrapContents = false
            };

            _contentTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20)
            };
            _contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _contentTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _contentTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            _chartMasVendidos = CrearChart("🏆 Top 5 Más Vendidos");
            _dgvMenosVendidos = CrearGrid("📉 Top 5 Menos Vendidos (con ventas > 0)");
            _dgvStockBajo = CrearGrid("⚠️ Productos Stock Crítico");
            _dgvProximosCaducar = CrearGrid("📅 Lotes Próximos a Caducar (90 días)");

            var panelChart = CrearContenedorGrid("Top 5 Productos Más Vendidos", _chartMasVendidos);
            var panelGridMenos = CrearContenedorGrid("Top 5 Productos Menos Vendidos", _dgvMenosVendidos);
            var panelGridStock = CrearContenedorGrid("Productos con Stock Crítico", _dgvStockBajo);
            var panelGridCaducidad = CrearContenedorGrid("Lotes Próximos a Caducar", _dgvProximosCaducar);

            _contentTable.Controls.Add(panelChart, 0, 0);
            _contentTable.Controls.Add(panelGridMenos, 0, 1);
            _contentTable.Controls.Add(panelGridStock, 1, 0);
            _contentTable.Controls.Add(panelGridCaducidad, 1, 1);

            this.Controls.Add(_contentTable);
            this.Controls.Add(_cardsPanel);
            this.Controls.Add(topPanel);
        }

        private Chart CrearChart(string titulo)
        {
            var chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White, MinimumSize = new Size(10, 10) };
            var area = new ChartArea { BackColor = Color.White };
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.Add(area);

            var series = new Series
            {
                Name = "Ventas",
                Color = Theme.PrimaryColor,
                IsValueShownAsLabel = true,
                ChartType = SeriesChartType.Column
            };
            chart.Series.Add(series);

            return chart;
        }

        private DataGridView CrearGrid(string nombre)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.LightGray
            };

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Theme.SecondaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = Theme.FontNormalBold;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;
            dgv.DefaultCellStyle.Font = Theme.FontNormal;
            dgv.DefaultCellStyle.SelectionBackColor = Theme.PrimaryColor;
            dgv.RowTemplate.Height = 35;

            return dgv;
        }

        private Panel CrearContenedorGrid(string titulo, Control contenido)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, MinimumSize = new Size(50, 50) };
            var lblTitulo = new Label 
            { 
                Text = titulo, 
                Dock = DockStyle.Top, 
                Font = Theme.FontNormalBold, 
                BackColor = Theme.BackgroundColor, 
                ForeColor = Theme.TextDark,
                Padding = new Padding(5),
                Height = 30
            };
            panel.Controls.Add(contenido);
            panel.Controls.Add(lblTitulo);
            return panel;
        }

        private void CargarMetricas()
        {
            try
            {
                var metricas = _dashboardRepo.ObtenerMetricas(dtpInicio.Value, dtpFin.Value);
                
                _cardsPanel.Controls.Clear();
                
                _cardsPanel.Controls.Add(CrearTarjeta("💰 Ventas de Hoy", metricas.VentasHoy.ToString("C2"), Theme.PrimaryColor));
                _cardsPanel.Controls.Add(CrearTarjeta("🧾 Tickets Emitidos", metricas.TicketsHoy.ToString(), Theme.SecondaryColor));
                _cardsPanel.Controls.Add(CrearTarjeta("💸 Retiros Hoy", metricas.RetirosHoy.ToString("C2"), Color.FromArgb(155, 89, 182))); // Morado
                _cardsPanel.Controls.Add(CrearTarjeta("⏳ Cuentas por Cobrar", metricas.CuentasPorCobrar.ToString("C2"), Color.FromArgb(243, 156, 18))); // Naranja

                // Chart Más Vendidos
                _chartMasVendidos.Series[0].Points.Clear();
                foreach (var item in metricas.ProductosMasVendidos)
                {
                    var p = _chartMasVendidos.Series[0].Points.AddXY(item.Nombre, item.CantidadTotal);
                }

                // Grid Menos Vendidos
                _dgvMenosVendidos.DataSource = metricas.ProductosMenosVendidos.Select(x => new {
                    Producto = x.Nombre,
                    Cantidad = x.CantidadTotal.ToString("N2"),
                    Ingreso = x.TotalGenerado.ToString("C2")
                }).ToList();

                // Grid Stock Bajo
                _dgvStockBajo.DataSource = metricas.ProductosStockBajo.Select(x => new {
                    Código = x.CodigoBarras,
                    Producto = x.Nombre,
                    Stock_Actual = x.StockActual.ToString("N2"),
                    Mínimo = x.StockMinimo.ToString("N2")
                }).ToList();

                // Dar formato especial al stock bajo
                foreach (DataGridViewRow row in _dgvStockBajo.Rows)
                {
                    decimal stock = Convert.ToDecimal(row.Cells["Stock_Actual"].Value);
                    if (stock <= 0)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236); // Rojo claro para agotados
                    else
                        row.DefaultCellStyle.BackColor = Color.FromArgb(252, 243, 207); // Amarillo para próximos a agotarse
                }

                // Grid Próximos a Caducar
                _dgvProximosCaducar.DataSource = metricas.LotesProximosCaducar.Select(x => new {
                    Producto = x.ProductoNombre,
                    Lote = x.NumeroLote,
                    Stock = x.StockLote.ToString("N2"),
                    Caducidad = x.FechaCaducidad.ToString("dd/MM/yyyy"),
                    Días = x.DiasRestantes
                }).ToList();

                foreach (DataGridViewRow row in _dgvProximosCaducar.Rows)
                {
                    int dias = Convert.ToInt32(row.Cells["Días"].Value);
                    if (dias <= 30)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236); // Rojo
                    else if (dias <= 60)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(252, 243, 207); // Amarillo
                    else
                        row.DefaultCellStyle.BackColor = Color.FromArgb(232, 248, 245); // Verde claro
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al cargar las métricas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CrearTarjeta(string titulo, string valor, Color color)
        {
            Panel p = new Panel { Width = 280, Height = 130, BackColor = Color.White, Margin = new Padding(10) };
            
            p.BorderStyle = BorderStyle.FixedSingle;

            Panel pTop = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = color };
            
            Label lTitulo = new Label { Text = titulo, Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 20) };
            Label lValor = new Label { Text = valor, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 55) };

            p.Controls.Add(pTop);
            p.Controls.Add(lTitulo);
            p.Controls.Add(lValor);

            return p;
        }
    }
}
