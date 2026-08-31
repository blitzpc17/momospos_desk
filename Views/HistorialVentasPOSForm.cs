using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using momospos.Views.Dialogs;

namespace momospos.Views
{
    public class HistorialVentasPOSForm : Form
    {
        private DataGridView dgvHistorial;
        private Button btnSolicitarCancelacion;
        private Label lblConteo;

        private VentaRepository _ventaRepo;
        private Usuario _usuarioActual;
        private CajaSesion _sesionActual;

        public HistorialVentasPOSForm(Usuario usuarioActual, CajaSesion sesionActual)
        {
            _usuarioActual = usuarioActual;
            _sesionActual = sesionActual;
            _ventaRepo = new VentaRepository();
            
            BuildUI();
            Theme.SetIcon(this);
            CargarHistorial();
        }

        private void BuildUI()
        {
            this.Text = "Historial de Ventas (Punto de Venta)";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BackgroundColor;

            // TOP PANEL
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "📜 Historial de Ventas del Turno", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            
            btnSolicitarCancelacion = new Button { Text = "❌ Solicitar Cancelación", Location = new Point(710, 20), Width = 200, Height = 40 };
            Theme.StyleButton(btnSolicitarCancelacion, Theme.DangerColor);
            btnSolicitarCancelacion.Click += BtnSolicitarCancelacion_Click;
            
            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnSolicitarCancelacion);

            // BOTTOM PANEL
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(20) };
            lblConteo = new Label { Text = "Ventas: 0", Font = Theme.FontNormal, ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 15) };
            bottomPanel.Controls.Add(lblConteo);

            // GRID PANEL
            Panel fillPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 0) };
            dgvHistorial = new DataGridView();
            dgvHistorial.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvHistorial);
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.MultiSelect = false;
            
            fillPanel.Controls.Add(dgvHistorial);

            this.Controls.Add(fillPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void CargarHistorial()
        {
            try
            {
                // Cargamos solo las ventas de hoy
                DateTime inicio = DateTime.Today;
                DateTime fin = DateTime.Today.AddDays(1).AddTicks(-1);

                var reporte = _ventaRepo.ObtenerReporteVentas(inicio, fin);
                var historial = reporte.Historial;
                
                // Si la sesión no es nula, filtramos solo las ventas de esta caja sesión
                if (_sesionActual != null && historial != null)
                {
                    historial = historial.Where(v => v.CajaSesionId == _sesionActual.Id).ToList();
                }

                dgvHistorial.DataSource = historial;
                
                if (dgvHistorial.Columns["Id"] != null) dgvHistorial.Columns["Id"].Visible = false;
                if (dgvHistorial.Columns["CajaSesionId"] != null) dgvHistorial.Columns["CajaSesionId"].Visible = false;
                if (dgvHistorial.Columns["UsuarioId"] != null) dgvHistorial.Columns["UsuarioId"].Visible = false;
                if (dgvHistorial.Columns["ClienteId"] != null) dgvHistorial.Columns["ClienteId"].Visible = false;

                foreach (DataGridViewColumn col in dgvHistorial.Columns)
                {
                    if (col.ValueType == typeof(decimal) && string.IsNullOrEmpty(col.DefaultCellStyle.Format))
                    {
                        col.DefaultCellStyle.Format = "C2";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }

                if (dgvHistorial.Columns["Fecha"] != null)
                {
                    dgvHistorial.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }

                lblConteo.Text = $"Ventas mostradas: {(historial?.Count ?? 0)}";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error al cargar historial:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSolicitarCancelacion_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.CurrentRow == null || !(dgvHistorial.CurrentRow.DataBoundItem is Venta venta))
            {
                CustomMessageBox.Show("Por favor, seleccione una venta del historial.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (venta.Estado != "CONFIRMADO")
            {
                CustomMessageBox.Show("Solo se pueden cancelar ventas que estén CONFIRMADAS.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string motivo = CustomDialog.ShowInput($"Ingrese el motivo de la cancelación para la venta {venta.Folio}:", "Solicitar Cancelación", "");
            
            if (string.IsNullOrWhiteSpace(motivo))
            {
                CustomMessageBox.Show("Debe ingresar un motivo para poder solicitar la cancelación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _ventaRepo.SolicitarCancelacionVenta(venta.Id, _usuarioActual.Id, motivo);
                CustomMessageBox.Show("Solicitud de cancelación enviada correctamente. Esperando autorización del Administrador.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarHistorial(); // Refrescar historial
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error al solicitar cancelación:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
