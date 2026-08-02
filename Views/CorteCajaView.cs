using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using System.Linq;

namespace momospos.Views
{
    public class CorteCajaView : UserControl
    {
        private TextBox txtEfectivoContado;
        private Button btnCerrarTurno;
        private Label lblFondoInicial;
        private Label lblEfectivoEsperado;
        private Label lblTotalVentas;
        private Label lblTotalRetiros;
        private DataGridView dgvMovimientos;

        private CajaRepository _cajaRepo;
        private Usuario _usuarioActual;
        private CajaSesion _sesionActual;

        public CorteCajaView(Usuario usuarioActual, CajaSesion sesionActual)
        {
            _usuarioActual = usuarioActual;
            _sesionActual = sesionActual;
            _cajaRepo = new CajaRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // HEADER
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "🛑 Corte de Caja y Cierre de Turno", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            Label lblSubtitulo = new Label { Text = "Ingrese el efectivo físico contado para realizar el cierre de la caja.", Font = Theme.FontNormal, ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 60) };
            
            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblSubtitulo);

            // RESUMEN PANEL (LEFT)
            Panel resumenPanel = new Panel { Dock = DockStyle.Left, Width = 400, Padding = new Padding(20) };
            
            Label lblHeaderResumen = new Label { Text = "Resumen del Turno", Font = Theme.FontSubtitle, Location = new Point(20, 20), AutoSize = true };
            
            lblFondoInicial = new Label { Text = "Fondo Inicial: $0.00", Font = Theme.FontNormal, Location = new Point(20, 70), AutoSize = true };
            lblTotalVentas = new Label { Text = "+ Ventas Efectivo: $0.00", Font = Theme.FontNormal, ForeColor = Theme.SuccessColor, Location = new Point(20, 110), AutoSize = true };
            lblTotalRetiros = new Label { Text = "- Retiros/Devol: $0.00", Font = Theme.FontNormal, ForeColor = Theme.DangerColor, Location = new Point(20, 150), AutoSize = true };
            
            lblEfectivoEsperado = new Label { Text = "Efectivo Esperado:\n$0.00", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Theme.PrimaryColor, Location = new Point(20, 200), AutoSize = true };

            Label lblIngreso = new Label { Text = "Efectivo Físico Contado:", Font = Theme.FontSubtitle, Location = new Point(20, 300), AutoSize = true };
            txtEfectivoContado = new TextBox { Location = new Point(20, 340), Width = 300, Font = new Font("Segoe UI", 24, FontStyle.Bold), TextAlign = HorizontalAlignment.Right };
            txtEfectivoContado.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true; };

            btnCerrarTurno = new Button { Text = "🔒 CERRAR TURNO", Location = new Point(20, 420), Width = 300, Height = 60 };
            Theme.StyleButton(btnCerrarTurno, Theme.DangerColor, Theme.TextLight, Theme.FontTitle);
            btnCerrarTurno.Click += BtnCerrarTurno_Click;

            resumenPanel.Controls.Add(lblHeaderResumen);
            resumenPanel.Controls.Add(lblFondoInicial);
            resumenPanel.Controls.Add(lblTotalVentas);
            resumenPanel.Controls.Add(lblTotalRetiros);
            resumenPanel.Controls.Add(lblEfectivoEsperado);
            resumenPanel.Controls.Add(lblIngreso);
            resumenPanel.Controls.Add(txtEfectivoContado);
            resumenPanel.Controls.Add(btnCerrarTurno);

            // DETALLES PANEL (RIGHT)
            Panel detallesPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Label lblMovimientos = new Label { Text = "Movimientos de Caja", Font = Theme.FontSubtitle, Location = new Point(20, 20), AutoSize = true };
            
            dgvMovimientos = new DataGridView();
            dgvMovimientos.Location = new Point(20, 70);
            dgvMovimientos.Width = 600;
            dgvMovimientos.Height = 500;
            dgvMovimientos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Theme.StyleDataGridView(dgvMovimientos);

            detallesPanel.Controls.Add(lblMovimientos);
            detallesPanel.Controls.Add(dgvMovimientos);

            this.Controls.Add(detallesPanel);
            this.Controls.Add(resumenPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDatos()
        {
            try
            {
                // Actualizar info desde BD por si hubo cambios
                _sesionActual = _cajaRepo.ObtenerSesionAbierta();
                if (_sesionActual == null) return;

                var movimientos = _cajaRepo.ObtenerMovimientosSesion(_sesionActual.Id).ToList();
                
                decimal ventasEf = movimientos.Where(x => x.Importe > 0).Sum(x => x.Importe);
                decimal retiros = movimientos.Where(x => x.Importe < 0).Sum(x => x.Importe); // Ya viene negativo

                lblFondoInicial.Text = $"Fondo Inicial: {_sesionActual.FondoInicial:C}";
                lblTotalVentas.Text = $"+ Ingresos: {ventasEf:C}";
                lblTotalRetiros.Text = $"- Retiros/Devol: {retiros:C}";
                lblEfectivoEsperado.Text = $"Efectivo Esperado:\n{_sesionActual.EfectivoEsperado:C}";

                dgvMovimientos.DataSource = movimientos;
                if (dgvMovimientos.Columns["Id"] != null) dgvMovimientos.Columns["Id"].Visible = false;
                if (dgvMovimientos.Columns["CajaSesionId"] != null) dgvMovimientos.Columns["CajaSesionId"].Visible = false;
                if (dgvMovimientos.Columns["UsuarioId"] != null) dgvMovimientos.Columns["UsuarioId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos:\n{ex.Message}");
            }
        }

        private void BtnCerrarTurno_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtEfectivoContado.Text, out decimal cantidadContada))
            {
                MessageBox.Show("Monto inválido. Ingrese el efectivo físico contado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"¿Seguro que desea cerrar el turno con {cantidadContada:C}?", "Cerrar Turno", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _sesionActual.UsuarioCierreId = _usuarioActual.Id;
                    _sesionActual.FechaCierre = DateTime.Now;
                    _sesionActual.EfectivoContado = cantidadContada;
                    _sesionActual.Diferencia = cantidadContada - _sesionActual.EfectivoEsperado;
                    
                    _cajaRepo.CerrarCaja(_sesionActual);
                    
                    string msg = $"CORTE REALIZADO EXITOSAMENTE\n\nEfectivo Esperado: {_sesionActual.EfectivoEsperado:C}\nContado Físico: {cantidadContada:C}\nDiferencia: {_sesionActual.Diferencia:C}";
                    MessageBox.Show(msg, "Corte de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    Application.Exit(); // El sistema se cierra al finalizar turno
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cerrar caja:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
