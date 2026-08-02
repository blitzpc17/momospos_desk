using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using momospos.Models;
using momospos.Repositories;
using Microsoft.VisualBasic;

namespace momospos.Views
{
    public class CuentasCobrarView : UserControl
    {
        private DataGridView dgvDeudores;
        private TextBox txtBuscar;
        private Label lblConteo;
        
        private ClienteRepository _clienteRepo;
        private List<Cliente> _todosDeudores;

        public CuentasCobrarView()
        {
            _clienteRepo = new ClienteRepository();
            BuildUI();
            CargarDeudores();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "💳 Cuentas por Cobrar (Crédito)", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            
            Button btnAbonar = new Button { Text = "Registrar Abono", Location = new Point(this.Width - 250, 20), Width = 180, Height = 40, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            Theme.StyleButton(btnAbonar, Theme.SuccessColor);
            btnAbonar.Click += BtnAbonar_Click;

            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(20, 60) };
            txtBuscar = new TextBox { Location = new Point(100, 57), Width = 250, Font = Theme.FontNormal };
            txtBuscar.TextChanged += (s, e) => FiltrarDatos();

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnAbonar);
            topPanel.Controls.Add(lblBuscar);
            topPanel.Controls.Add(txtBuscar);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(15, 5, 15, 5) };
            lblConteo = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, AutoSize = true, Dock = DockStyle.Left };
            bottomPanel.Controls.Add(lblConteo);

            dgvDeudores = new DataGridView();
            dgvDeudores.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvDeudores);

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvDeudores);

            this.Controls.Add(marginPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDeudores()
        {
            _todosDeudores = _clienteRepo.ObtenerTodos().Where(c => c.Saldo > 0).ToList();
            FiltrarDatos();
        }

        private void FiltrarDatos()
        {
            if (_todosDeudores == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();
            var filtrados = _todosDeudores;

            if (!string.IsNullOrEmpty(filtro))
            {
                filtrados = _todosDeudores.Where(c => 
                    (c.Nombre != null && c.Nombre.ToLower().Contains(filtro)) ||
                    (c.Telefono != null && c.Telefono.ToLower().Contains(filtro))
                ).ToList();
            }

            dgvDeudores.DataSource = filtrados;
            if (dgvDeudores.Columns["Id"] != null) dgvDeudores.Columns["Id"].Visible = false;

            lblConteo.Text = $"Total de registros: {filtrados.Count}";
        }

        private void BtnAbonar_Click(object sender, EventArgs e)
        {
            if (dgvDeudores.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.");
                return;
            }

            var cliente = (Cliente)dgvDeudores.CurrentRow.DataBoundItem;

            string input = Interaction.InputBox($"Cliente: {cliente.Nombre}\nDeuda Actual: {cliente.Saldo:C}\nIngrese monto a abonar:", "Abonar a Crédito", "0");
            
            if (decimal.TryParse(input, out decimal abono) && abono > 0)
            {
                if (abono > cliente.Saldo)
                {
                    MessageBox.Show("El abono no puede ser mayor a la deuda.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cliente.Saldo -= abono;
                try
                {
                    _clienteRepo.Actualizar(cliente);
                    
                    // Aquí opcionalmente podrías agregar un registro en CajaMovimientos si el abono entra directo a caja.
                    CajaRepository cajaRepo = new CajaRepository();
                    var sesion = cajaRepo.ObtenerSesionAbierta();
                    if (sesion != null)
                    {
                        cajaRepo.ActualizarEfectivoEsperado(sesion.Id, abono);
                        cajaRepo.RegistrarMovimientoCaja(new CajaMovimiento
                        {
                            CajaSesionId = sesion.Id,
                            Tipo = "INGRESO",
                            Importe = abono,
                            Concepto = $"Abono Cliente: {cliente.Nombre}",
                            UsuarioId = sesion.UsuarioAperturaId, // Asumir usuario actual
                            Fecha = DateTime.Now
                        });
                    }

                    MessageBox.Show("Abono registrado con éxito.");
                    CargarDeudores();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al registrar abono: " + ex.Message);
                }
            }
        }
    }
}
