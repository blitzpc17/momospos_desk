using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using System.Collections.Generic;
using momospos.Models;
using Microsoft.VisualBasic;

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

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Dock = DockStyle.Left };
            bottomPanel.Controls.Add(lblConteo);

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
                    var articulos = _ventaRepo.ObtenerArticulosVendidosPorPeriodo(dtpInicio.Value, dtpFin.Value);

                    decimal sumaGenerado = 0;
                    foreach (var a in articulos) sumaGenerado += a.TotalGenerado;

                    lblTotalVendido.Text = sumaGenerado.ToString("C");
                    lblTotalEfectivo.Text = "N/A";
                    lblTotalTarjeta.Text = "N/A";

                    dgvHistorial.DataSource = null;
                    dgvHistorial.DataSource = articulos;

                    if (dgvHistorial.Columns["CantidadTotal"] != null)
                    {
                        dgvHistorial.Columns["CantidadTotal"].HeaderText = "Cant. Vendida";
                        dgvHistorial.Columns["CantidadTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["CantidadTotal"].DefaultCellStyle.Format = "N2";
                    }
                    if (dgvHistorial.Columns["TotalGenerado"] != null)
                    {
                        dgvHistorial.Columns["TotalGenerado"].HeaderText = "Total Generado";
                        dgvHistorial.Columns["TotalGenerado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvHistorial.Columns["TotalGenerado"].DefaultCellStyle.Format = "C2";
                    }
                    if (dgvHistorial.Columns["Nombre"] != null)
                        dgvHistorial.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    lblConteo.Text = $"Total de artículos diferentes: {articulos?.Count ?? 0}";
                }
                else // Historial de Ventas
                {
                    btnSolicitarCancelacion.Visible = true;
                    var reporte = _ventaRepo.ObtenerReporteVentas(dtpInicio.Value, dtpFin.Value);

                    lblTotalVendido.Text = reporte.TotalVendido.ToString("C");
                    lblTotalEfectivo.Text = reporte.TotalEfectivo.ToString("C");
                    lblTotalTarjeta.Text = reporte.TotalTarjeta.ToString("C");

                    dgvHistorial.DataSource = null;
                    dgvHistorial.DataSource = reporte.Historial;
                    
                    if (dgvHistorial.Columns["Id"] != null) dgvHistorial.Columns["Id"].Visible = false;
                    if (dgvHistorial.Columns["CajaSesionId"] != null) dgvHistorial.Columns["CajaSesionId"].Visible = false;
                    if (dgvHistorial.Columns["UsuarioId"] != null) dgvHistorial.Columns["UsuarioId"].Visible = false;
                    if (dgvHistorial.Columns["ClienteId"] != null) dgvHistorial.Columns["ClienteId"].Visible = false;

                    lblConteo.Text = $"Total de ventas: {reporte.Historial?.Count ?? 0}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte:\n{ex.Message}");
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
