using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MomosClinic.Repositories;
using momospos.Views;

namespace MomosClinic.Views
{
    public class DashboardView : UserControl
    {
        private DashboardClinicRepository _repo;
        private TableLayoutPanel _layoutCards;
        private Chart _chartConsultas;

        public DashboardView()
        {
            _repo = new DashboardClinicRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Size = new Size(1000, 700); // Tamaño base para que los anclajes calculen bien
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;
            this.Padding = new Padding(20);

            Label lblTitle = new Label { Text = "Dashboard Interactivo", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblTitle);

            _layoutCards = new TableLayoutPanel
            {
                Location = new Point(20, 80),
                Width = 960,
                Height = 120,
                ColumnCount = 4,
                RowCount = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _layoutCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _layoutCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _layoutCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _layoutCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            this.Controls.Add(_layoutCards);

            _chartConsultas = new Chart
            {
                Location = new Point(20, 230),
                Width = 960,
                Height = 430,
                MinimumSize = new Size(10, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };
            ChartArea area = new ChartArea { BackColor = Color.White };
            area.AxisX.Title = "Día del Mes";
            area.AxisY.Title = "Consultas";
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            _chartConsultas.ChartAreas.Add(area);

            Series series = new Series
            {
                Name = "Consultas",
                Color = Theme.PrimaryColor,
                ChartType = SeriesChartType.Column,
                BorderWidth = 2
            };
            _chartConsultas.Series.Add(series);
            this.Controls.Add(_chartConsultas);
        }

        private Panel CreateCard(string title, string value, Color bgColor)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, Margin = new Padding(10), BackColor = bgColor };
            
            // Efecto redondeado simulado con un padding o simplemente color sólido con border (WinForms no soporta border-radius nativo facilmente sin OnPaint)
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = Color.White, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomCenter, Padding = new Padding(0,0,0,10) };
            Label lblTitle = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.WhiteSmoke, AutoSize = false, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(0,10,0,0) };
            
            pnl.Controls.Add(lblValue);
            pnl.Controls.Add(lblTitle);
            return pnl;
        }

        private void CargarDatos()
        {
            try
            {
                var metricas = _repo.ObtenerMetricasMesActual();
                
                _layoutCards.Controls.Clear();
                _layoutCards.Controls.Add(CreateCard("Consultas (Mes)", metricas.ConsultasMesActual.ToString(), Theme.PrimaryColor), 0, 0);
                _layoutCards.Controls.Add(CreateCard("Nuevos Pacientes", metricas.PacientesNuevosMes.ToString(), Theme.SuccessColor), 1, 0);
                _layoutCards.Controls.Add(CreateCard("Recetas Emitidas", metricas.RecetasEmitidasMes.ToString(), Theme.WarningColor), 2, 0);
                _layoutCards.Controls.Add(CreateCard("Ingresos (Est.)", metricas.IngresoEstimadoMes.ToString("C0"), Theme.SecondaryColor), 3, 0);

                var grafica = _repo.ObtenerConsultasPorDiaMesActual();
                _chartConsultas.Series[0].Points.Clear();
                foreach (var g in grafica)
                {
                    _chartConsultas.Series[0].Points.AddXY(g.Dia, g.CantidadConsultas);
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error cargando dashboard: " + ex.Message);
            }
        }
    }
}
