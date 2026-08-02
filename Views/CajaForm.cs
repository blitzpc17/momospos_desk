using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;

namespace momospos.Views
{
    public class CajaForm : Form
    {
        private TextBox txtFondo;
        private Button btnAccion;
        private Label lblTitulo;
        
        private CajaRepository _cajaRepo;
        private Usuario _usuarioActual;
        private bool _esApertura;

        public CajaForm(Usuario usuario, bool esApertura)
        {
            _usuarioActual = usuario;
            _esApertura = esApertura;
            _cajaRepo = new CajaRepository();
            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = _esApertura ? "Apertura de Turno" : "Corte de Caja (Cierre)";
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Color themeColor = _esApertura ? Theme.SuccessColor : Theme.DangerColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = themeColor };
            Label lblHeader = new Label { 
                Text = _esApertura ? "🟢 INICIAR TURNO" : "🛑 CERRAR TURNO", 
                Font = Theme.FontTitle, 
                ForeColor = Theme.TextLight, 
                AutoSize = true, 
                Location = new Point(20, 15) 
            };
            topPanel.Controls.Add(lblHeader);

            lblTitulo = new Label { 
                Text = _esApertura ? "Ingrese el Fondo Inicial de la Caja:" : "Ingrese el Efectivo Físico Contado:", 
                Font = Theme.FontNormal,
                ForeColor = Theme.TextDark,
                AutoSize = true, 
                Location = new Point(40, 90) 
            };
            
            txtFondo = new TextBox { 
                Location = new Point(40, 120), 
                Width = 300, 
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right
            };
            txtFondo.KeyPress += TxtFondo_KeyPress;

            btnAccion = new Button { 
                Text = _esApertura ? "APERTURAR CAJA" : "FINALIZAR TURNO", 
                Location = new Point(40, 200), 
                Width = 300, 
                Height = 50
            };
            Theme.StyleButton(btnAccion, themeColor, Theme.TextLight, Theme.FontTitle);
            btnAccion.Click += BtnAccion_Click;

            this.Controls.Add(topPanel);
            this.Controls.Add(lblTitulo);
            this.Controls.Add(txtFondo);
            this.Controls.Add(btnAccion);
        }
        
        private void TxtFondo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void BtnAccion_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtFondo.Text, out decimal cantidad))
            {
                MessageBox.Show("Monto inválido.");
                return;
            }

            try
            {
                if (_esApertura)
                {
                    _cajaRepo.AbrirCaja(new CajaSesion 
                    { 
                        UsuarioAperturaId = _usuarioActual.Id,
                        FondoInicial = cantidad
                    });
                    MessageBox.Show("Caja abierta exitosamente.");
                }
                else
                {
                    var sesionAbierta = _cajaRepo.ObtenerSesionAbierta();
                    if (sesionAbierta != null)
                    {
                        sesionAbierta.UsuarioCierreId = _usuarioActual.Id;
                        sesionAbierta.FechaCierre = DateTime.Now;
                        sesionAbierta.EfectivoContado = cantidad;
                        sesionAbierta.Diferencia = cantidad - sesionAbierta.EfectivoEsperado;
                        
                        _cajaRepo.CerrarCaja(sesionAbierta);
                        
                        string msg = $"Corte realizado.\nEfectivo Esperado: {sesionAbierta.EfectivoEsperado:C}\nContado: {cantidad:C}\nDiferencia: {sesionAbierta.Diferencia:C}";
                        MessageBox.Show(msg, "Corte de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
