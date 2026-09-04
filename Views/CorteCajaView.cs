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
        private Button btnImprimirPreCorte;
        private Button btnGuardarPdfPreCorte;
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

            Button btnDesglose = new Button { Text = "🧮 Desglosar Efectivo", Location = new Point(20, 395), Width = 300, Height = 30 };
            Theme.StyleButton(btnDesglose, Theme.SecondaryColor, Theme.TextLight, new Font("Segoe UI", 10, FontStyle.Bold));
            btnDesglose.Click += (s, e) => {
                var form = new momospos.Views.Dialogs.DesgloseEfectivoForm();
                if (form.ShowDialog() == DialogResult.OK) {
                    txtEfectivoContado.Text = form.TotalEfectivo.ToString("F2");
                }
            };
            resumenPanel.Controls.Add(btnDesglose);

            btnCerrarTurno = new Button { Text = "🔒 CERRAR TURNO", Location = new Point(20, 435), Width = 300, Height = 60 };
            Theme.StyleButton(btnCerrarTurno, Theme.DangerColor, Theme.TextLight, Theme.FontTitle);
            btnCerrarTurno.Click += BtnCerrarTurno_Click;

            btnImprimirPreCorte = new Button { Text = "🖨️ Imprimir Pre-Corte", Location = new Point(20, 505), Width = 145, Height = 40 };
            Theme.StyleButton(btnImprimirPreCorte, Theme.SecondaryColor);
            btnImprimirPreCorte.Click += (s, e) => {
                try {
                    var printer = new CortePrinter(_sesionActual, _usuarioActual.Nombre, true);
                    printer.Imprimir();
                    momospos.Views.CustomMessageBox.Show("Pre-Corte enviado a la impresora.", "Éxito");
                } catch(Exception ex) {
                    momospos.Views.CustomMessageBox.Show($"Error al imprimir:\n{ex.Message}", "Error");
                }
            };

            btnGuardarPdfPreCorte = new Button { Text = "📄 Guardar PDF", Location = new Point(175, 505), Width = 145, Height = 40 };
            Theme.StyleButton(btnGuardarPdfPreCorte, Color.White, Theme.PrimaryColor);
            btnGuardarPdfPreCorte.Click += (s, e) => {
                try {
                    SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = $"CorteCaja_{DateTime.Now:yyyyMMdd_HHmm}.pdf" };
                    if (sfd.ShowDialog() == DialogResult.OK) {
                        var printer = new CortePrinter(_sesionActual, _usuarioActual.Nombre, true);
                        printer.ImprimirComoPdf(sfd.FileName);
                        momospos.Views.CustomMessageBox.Show("PDF guardado correctamente.", "Éxito");
                    }
                } catch(Exception ex) {
                    momospos.Views.CustomMessageBox.Show($"Error al guardar PDF:\n{ex.Message}", "Error");
                }
            };

            resumenPanel.Controls.Add(lblHeaderResumen);
            resumenPanel.Controls.Add(lblFondoInicial);
            resumenPanel.Controls.Add(lblTotalVentas);
            resumenPanel.Controls.Add(lblTotalRetiros);
            resumenPanel.Controls.Add(lblEfectivoEsperado);
            resumenPanel.Controls.Add(lblIngreso);
            resumenPanel.Controls.Add(txtEfectivoContado);
            resumenPanel.Controls.Add(btnCerrarTurno);
            resumenPanel.Controls.Add(btnImprimirPreCorte);
            resumenPanel.Controls.Add(btnGuardarPdfPreCorte);

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
                _sesionActual = _cajaRepo.ObtenerSesionAbierta(momospos.Helpers.ConfiguracionHelper.ObtenerCajaLocalId());
                if (_sesionActual == null) return;

                var movimientos = _cajaRepo.ObtenerMovimientosSesion(_sesionActual.Id).ToList();
                
                decimal ventasEf = movimientos.Where(x => x.Tipo == "VENTA" || x.Tipo == "INGRESO").Sum(x => x.Importe);
                decimal retiros = movimientos.Where(x => x.Tipo == "RETIRO" || x.Tipo == "DEVOLUCION").Sum(x => Math.Abs(x.Importe));

                lblFondoInicial.Text = $"Fondo Inicial: {_sesionActual.FondoInicial:C}";
                lblTotalVentas.Text = $"+ Ingresos: {ventasEf:C}";
                lblTotalRetiros.Text = $"- Retiros/Devol: {retiros:C}";
                
                var configRepo = new momospos.Repositories.ConfiguracionRepository();
                bool corteCiego = configRepo.ObtenerValor("CorteCiego") == "true";
                if (corteCiego)
                {
                    lblEfectivoEsperado.Text = "Efectivo Esperado:\n[Oculto por Seguridad]";
                    btnImprimirPreCorte.Enabled = false;
                    btnGuardarPdfPreCorte.Enabled = false;
                }
                else
                {
                    lblEfectivoEsperado.Text = $"Efectivo Esperado:\n{_sesionActual.EfectivoEsperado:C}";
                }

                dgvMovimientos.DataSource = movimientos;
                if (dgvMovimientos.Columns["Id"] != null) dgvMovimientos.Columns["Id"].Visible = false;
                if (dgvMovimientos.Columns["CajaSesionId"] != null) dgvMovimientos.Columns["CajaSesionId"].Visible = false;
                if (dgvMovimientos.Columns["UsuarioId"] != null) dgvMovimientos.Columns["UsuarioId"].Visible = false;
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show($"Error al cargar datos:\n{ex.Message}");
            }
        }

        private void BtnCerrarTurno_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtEfectivoContado.Text, out decimal cantidadContada))
            {
                momospos.Views.CustomMessageBox.Show("Monto inválido. Ingrese el efectivo físico contado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = momospos.Views.CustomMessageBox.Show($"¿Seguro que desea cerrar el turno con {cantidadContada:C}?", "Cerrar Turno", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _sesionActual.UsuarioCierreId = _usuarioActual.Id;
                    _sesionActual.FechaCierre = DateTime.Now;
                    _sesionActual.EfectivoContado = cantidadContada;
                    _sesionActual.Diferencia = cantidadContada - _sesionActual.EfectivoEsperado;
                    
                    _cajaRepo.CerrarCaja(_sesionActual);
                    
                    var configRepo = new momospos.Repositories.ConfiguracionRepository();
                    bool corteCiego = configRepo.ObtenerValor("CorteCiego") == "true";
                    string msg = "CORTE REALIZADO EXITOSAMENTE\n\n";
                    if (!corteCiego)
                    {
                        msg += $"Efectivo Esperado: {_sesionActual.EfectivoEsperado:C}\n";
                        msg += $"Contado Físico: {cantidadContada:C}\n";
                        msg += $"Diferencia: {_sesionActual.Diferencia:C}";
                    }
                    else
                    {
                        msg += "El corte ha sido registrado en el sistema de manera segura.\nEl administrador podrá revisarlo en los reportes.";
                    }

                    momospos.Views.CustomMessageBox.Show(msg, "Corte de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Enviar correo automático si está configurado
                    EnviarCortePorCorreo(_sesionActual);
                    
                    var dlgResult = momospos.Views.CustomMessageBox.Show("¿Desea imprimir el ticket de corte final antes de cerrar el turno?", "Imprimir Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dlgResult == DialogResult.Yes)
                    {
                        try
                        {
                            var printer = new CortePrinter(_sesionActual, _usuarioActual.Nombre, false);
                            printer.Imprimir();
                        }
                        catch (Exception ex)
                        {
                            momospos.Views.CustomMessageBox.Show($"Error al imprimir el corte final:\n{ex.Message}");
                        }
                    }

                    this.FindForm()?.Close(); // Cierra el MainForm para volver al Login
                }
                catch (Exception ex)
                {
                    momospos.Views.CustomMessageBox.Show($"Error al cerrar caja:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EnviarCortePorCorreo(CajaSesion sesion)
        {
            try
            {
                var configRepo = new momospos.Repositories.ConfiguracionRepository();
                string emisor = configRepo.ObtenerValor("EmailEmisor");
                string pass = configRepo.ObtenerValor("EmailPassword");
                string destino = configRepo.ObtenerValor("EmailDestino");
                string negocio = configRepo.ObtenerValor("NombreNegocio");

                if (!string.IsNullOrEmpty(emisor) && !string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(destino))
                {
                    string tempPdf = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"CorteTurno_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
                    var printer = new CortePrinter(sesion, _usuarioActual.Nombre, false);
                    printer.ImprimirComoPdf(tempPdf);

                    if (System.IO.File.Exists(tempPdf))
                    {
                        using (var mail = new System.Net.Mail.MailMessage())
                        {
                            mail.From = new System.Net.Mail.MailAddress(emisor, negocio);
                            mail.To.Add(destino);
                            mail.Subject = $"Corte de Turno - {DateTime.Now:dd/MM/yyyy HH:mm}";
                            mail.Body = $"Adjunto se envía el reporte de corte de caja del turno finalizado por {_usuarioActual.Nombre} el {DateTime.Now}.";
                            mail.Attachments.Add(new System.Net.Mail.Attachment(tempPdf));

                            using (var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                            {
                                smtp.Credentials = new System.Net.NetworkCredential(emisor, pass);
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo: " + ex.Message);
            }
        }
    }
}
