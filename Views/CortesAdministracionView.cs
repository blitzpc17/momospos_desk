using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using static momospos.Repositories.CajaRepository;

namespace momospos.Views
{
    public class CortesAdministracionView : UserControl
    {
        private Usuario _usuarioActual;
        
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFin;
        private Button btnBuscar;
        private Button btnCorteZ;
        
        private DataGridView dgvMaestro; // Días
        private DataGridView dgvDetalle; // Turnos del día seleccionado

        private List<ResumenCorteDiaDTO> _resumenDias;
        private List<CorteHistorialDTO> _turnosDiaSeleccionado;

        public CortesAdministracionView(Usuario usuario)
        {
            _usuarioActual = usuario;
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Theme.BackgroundColor;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            // --- Header Panel ---
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            
            Label lblTitulo = new Label
            {
                Text = "💰 Cortes de Caja",
                Font = Theme.FontTitle,
                AutoSize = true,
                Location = new Point(15, 25),
                ForeColor = Theme.PrimaryColor
            };
            
            pnlHeader.Controls.Add(lblTitulo);

            // Filtros de fecha
            Label lblInicio = new Label { Text = "Desde:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(350, 30) };
            dtpInicio = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130, Font = Theme.FontNormal, Location = new Point(410, 27) };
            dtpInicio.Value = DateTime.Now.AddDays(-7); // Últimos 7 días por defecto

            Label lblFin = new Label { Text = "Hasta:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(560, 30) };
            dtpFin = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130, Font = Theme.FontNormal, Location = new Point(620, 27) };

            btnBuscar = new Button { Text = "🔍 Filtrar", Width = 100, Height = 35, Location = new Point(770, 25) };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor, Theme.TextLight, Theme.FontNormal);
            btnBuscar.Click += BtnBuscar_Click;
            
            btnCorteZ = new Button { Text = "📆 Enviar Corte Z", Width = 180, Height = 35, Location = new Point(880, 25), Enabled = false };
            Theme.StyleButton(btnCorteZ, Theme.PrimaryColor, Theme.TextLight, Theme.FontNormal);
            btnCorteZ.Click += BtnCorteZ_Click;

            pnlHeader.Controls.Add(lblInicio);
            pnlHeader.Controls.Add(dtpInicio);
            pnlHeader.Controls.Add(lblFin);
            pnlHeader.Controls.Add(dtpFin);
            pnlHeader.Controls.Add(btnBuscar);
            pnlHeader.Controls.Add(btnCorteZ);
            
            // --- Split Container ---
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200, // Menos altura para el resumen por días
                Margin = new Padding(0, 10, 0, 0)
            };

            // Maestro
            GroupBox gbMaestro = new GroupBox
            {
                Text = "Resumen por Días",
                Font = Theme.FontNormal,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            
            dgvMaestro = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                MultiSelect = false
            };
            Theme.StyleDataGridView(dgvMaestro);
            dgvMaestro.SelectionChanged += DgvMaestro_SelectionChanged;
            gbMaestro.Controls.Add(dgvMaestro);
            
            // Detalle
            GroupBox gbDetalle = new GroupBox
            {
                Text = "Turnos del Día Seleccionado",
                Font = Theme.FontNormal,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            
            dgvDetalle = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                MultiSelect = false
            };
            Theme.StyleDataGridView(dgvDetalle);
            
            // Agregar botón de imprimir
            DataGridViewButtonColumn btnImprimirCol = new DataGridViewButtonColumn
            {
                Name = "Imprimir",
                HeaderText = "Impresión",
                Text = "🖨️",
                UseColumnTextForButtonValue = true,
                Width = 70,
                FlatStyle = FlatStyle.Flat,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            dgvDetalle.Columns.Add(btnImprimirCol);
            
            // Agregar botón de PDF
            DataGridViewButtonColumn btnPdfCol = new DataGridViewButtonColumn
            {
                Name = "Pdf",
                HeaderText = "PDF",
                Text = "📄",
                UseColumnTextForButtonValue = true,
                Width = 50,
                FlatStyle = FlatStyle.Flat,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            dgvDetalle.Columns.Add(btnPdfCol);
            
            // Agregar botón de Anotar Ajuste
            DataGridViewButtonColumn btnAnotarCol = new DataGridViewButtonColumn
            {
                Name = "Anotar",
                HeaderText = "Anotar",
                Text = "📝 Anotar",
                UseColumnTextForButtonValue = true,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            dgvDetalle.Columns.Add(btnAnotarCol);
            
            dgvDetalle.CellClick += DgvDetalle_CellClick;
            dgvDetalle.CellPainting += DgvDetalle_CellPainting;
            
            gbDetalle.Controls.Add(dgvDetalle);
            
            splitContainer.Panel1.Controls.Add(gbMaestro);
            splitContainer.Panel2.Controls.Add(gbDetalle);
            splitContainer.Panel1.Padding = new Padding(0, 10, 0, 5);
            splitContainer.Panel2.Padding = new Padding(0, 5, 0, 0);

            this.Controls.Add(splitContainer);
            this.Controls.Add(pnlHeader);
            
            this.Load += (s, e) => CargarDatosMaestro();
        }

        private void CargarDatosMaestro()
        {
            try
            {
                var inicio = dtpInicio.Value.Date;
                var fin = dtpFin.Value.Date.AddDays(1).AddSeconds(-1);

                var cajaRepo = new CajaRepository();
                _resumenDias = cajaRepo.ObtenerResumenCortesPorDias(inicio, fin);
                
                dgvMaestro.DataSource = null;
                dgvMaestro.DataSource = _resumenDias;
                
                if (dgvMaestro.Columns["Fecha"] != null) dgvMaestro.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
                if (dgvMaestro.Columns["FondoTotal"] != null) dgvMaestro.Columns["FondoTotal"].DefaultCellStyle.Format = "C2";
                if (dgvMaestro.Columns["SumaEsperada"] != null) dgvMaestro.Columns["SumaEsperada"].DefaultCellStyle.Format = "C2";
                if (dgvMaestro.Columns["SumaContada"] != null) dgvMaestro.Columns["SumaContada"].DefaultCellStyle.Format = "C2";
                if (dgvMaestro.Columns["Diferencia"] != null) dgvMaestro.Columns["Diferencia"].DefaultCellStyle.Format = "C2";
                
                AjustarAlineacion(dgvMaestro);

                if (_resumenDias.Count == 0)
                {
                    dgvDetalle.DataSource = null;
                    btnCorteZ.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al cargar el resumen de días: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvMaestro_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMaestro.SelectedRows.Count > 0)
            {
                if (dgvMaestro.SelectedRows[0].DataBoundItem is ResumenCorteDiaDTO diaSeleccionado)
                {
                    CargarDetalleTurnos(diaSeleccionado.Fecha);
                    btnCorteZ.Enabled = true;
                }
            }
        }

        private void CargarDetalleTurnos(DateTime fecha)
        {
            try
            {
                var inicio = fecha.Date;
                var fin = fecha.Date.AddDays(1).AddSeconds(-1);
                
                var cajaRepo = new CajaRepository();
                _turnosDiaSeleccionado = cajaRepo.ObtenerReporteCortes(inicio, fin);
                
                dgvDetalle.DataSource = null;
                dgvDetalle.DataSource = _turnosDiaSeleccionado;
                
                if (dgvDetalle.Columns["SesionId"] != null) dgvDetalle.Columns["SesionId"].Visible = false; // Ocultar ID
                if (dgvDetalle.Columns["CajaId"] != null) dgvDetalle.Columns["CajaId"].Visible = false; // Ocultar ID de Caja
                if (dgvDetalle.Columns["Estado"] != null) dgvDetalle.Columns["Estado"].Visible = false;
                
                if (dgvDetalle.Columns["NombreCajero"] != null) { dgvDetalle.Columns["NombreCajero"].HeaderText = "Cajero"; dgvDetalle.Columns["NombreCajero"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }
                if (dgvDetalle.Columns["FechaApertura"] != null) dgvDetalle.Columns["FechaApertura"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                if (dgvDetalle.Columns["FechaCierre"] != null) dgvDetalle.Columns["FechaCierre"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                if (dgvDetalle.Columns["FondoInicial"] != null) dgvDetalle.Columns["FondoInicial"].DefaultCellStyle.Format = "C2";
                if (dgvDetalle.Columns["EfectivoEsperado"] != null) dgvDetalle.Columns["EfectivoEsperado"].DefaultCellStyle.Format = "C2";
                if (dgvDetalle.Columns["EfectivoContado"] != null) dgvDetalle.Columns["EfectivoContado"].DefaultCellStyle.Format = "C2";
                if (dgvDetalle.Columns["Diferencia"] != null) dgvDetalle.Columns["Diferencia"].DefaultCellStyle.Format = "C2";
                
                if (dgvDetalle.Columns["Anotar"] != null)
                {
                    dgvDetalle.Columns["Anotar"].DisplayIndex = dgvDetalle.Columns.Count - 3;
                }
                
                if (dgvDetalle.Columns["Imprimir"] != null)
                {
                    dgvDetalle.Columns["Imprimir"].DisplayIndex = dgvDetalle.Columns.Count - 2;
                }

                if (dgvDetalle.Columns["Pdf"] != null)
                {
                    dgvDetalle.Columns["Pdf"].DisplayIndex = dgvDetalle.Columns.Count - 1;
                }
                
                AjustarAlineacion(dgvDetalle);
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al cargar el detalle del día: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvDetalle_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string colName = dgvDetalle.Columns[e.ColumnIndex].Name;
                if (colName == "Imprimir" || colName == "Pdf" || colName == "Anotar")
                {
                    e.PaintBackground(e.CellBounds, true);
                    
                    Rectangle rect = e.CellBounds;
                    rect.Inflate(-4, -4); // Margen interior

                    // Definir colores
                    Color bgColor = Theme.PrimaryColor; // Azul
                    Color textColor = Theme.TextLight;
                    string text = "🖨️";

                    if (colName == "Pdf") 
                    {
                        bgColor = Color.FromArgb(220, 53, 69); // Rojo PDF
                        text = "📄";
                    }
                    else if (colName == "Anotar") 
                    {
                        bgColor = Theme.WarningColor; // Naranja/Amarillo
                        textColor = Theme.TextDark;
                        text = "📝 Anotar";
                    }
                    
                    // Efecto Hover / Selección
                    if ((e.State & DataGridViewElementStates.Selected) != 0)
                    {
                        rect.Inflate(1, 1);
                    }

                    // Dibujar borde y fondo
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int r = 6; // Radio del borde
                        path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                        path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                        path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                        path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                        path.CloseFigure();

                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (Brush brush = new SolidBrush(bgColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }

                    // Dibujar texto
                    TextRenderer.DrawText(e.Graphics, text, Theme.FontNormal, rect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
            }
        }

        private void DgvDetalle_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var row = dgvDetalle.Rows[e.RowIndex];
                if (!(row.DataBoundItem is CorteHistorialDTO corteItem)) return;

                if (dgvDetalle.Columns[e.ColumnIndex].Name == "Imprimir")
                {
                    var dlg = momospos.Views.CustomMessageBox.Show($"¿Deseas reimprimir el ticket del corte del cajero {corteItem.NombreCajero} del día {corteItem.FechaCierre:dd/MM/yyyy}?", "Reimprimir Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                else if (dgvDetalle.Columns[e.ColumnIndex].Name == "Pdf")
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Archivos PDF (*.pdf)|*.pdf";
                        sfd.FileName = $"Corte_{corteItem.NombreCajero}_{corteItem.FechaCierre:yyyyMMdd_HHmm}.pdf";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                var cajaRepo = new CajaRepository();
                                var sesion = cajaRepo.ObtenerSesionPorId(corteItem.SesionId);
                                if (sesion != null)
                                {
                                    var printer = new CortePrinter(sesion, corteItem.NombreCajero, false);
                                    printer.ImprimirComoPdf(sfd.FileName);
                                    momospos.Views.CustomMessageBox.Show("PDF generado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            catch (Exception ex)
                            {
                                momospos.Views.CustomMessageBox.Show("Error al generar PDF: " + ex.Message, "Error");
                            }
                        }
                    }
                }
                else if (dgvDetalle.Columns[e.ColumnIndex].Name == "Anotar")
                {
                    string defaultVal = corteItem.Observaciones ?? "";
                    using (var frm = new momospos.Views.Dialogs.CustomInputBoxForm($"Ingresa las observaciones/ajustes para este turno:\n\n*Esto no modificará los montos, solo dejará un registro histórico.", "Anotación de Turno", defaultVal))
                    {
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                var cajaRepo = new CajaRepository();
                                cajaRepo.ActualizarObservacionesSesion(corteItem.SesionId, frm.InputValue);
                                corteItem.Observaciones = frm.InputValue;
                                momospos.Views.CustomMessageBox.Show("Observaciones guardadas correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                momospos.Views.CustomMessageBox.Show("Error al guardar observaciones: " + ex.Message, "Error");
                            }
                        }
                    }
                }
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            CargarDatosMaestro();
        }

        private void BtnCorteZ_Click(object sender, EventArgs e)
        {
            if (dgvMaestro.SelectedRows.Count == 0) return;
            
            if (dgvMaestro.SelectedRows[0].DataBoundItem is ResumenCorteDiaDTO diaSeleccionado)
            {
                try
                {
                    var fecha = diaSeleccionado.Fecha;
                    var cajaRepo = new CajaRepository();
                    var resumen = cajaRepo.ObtenerResumenCorteDia(fecha);

                    if (resumen.TotalCortes == 0)
                    {
                        momospos.Views.CustomMessageBox.Show($"No hay cortes registrados para el día {fecha:dd/MM/yyyy}.", "Corte Z", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string msg = $"--- CORTE Z DEL DÍA {fecha:dd/MM/yyyy} ---\n\n" +
                                 $"Total de Cortes en el Día: {resumen.TotalCortes}\n" +
                                 $"Suma Fondo Inicial: {resumen.FondoTotal:C}\n" +
                                 $"Suma Efectivo Esperado: {resumen.SumaEsperada:C}\n" +
                                 $"Suma Efectivo Contado: {resumen.SumaContada:C}\n" +
                                 $"Diferencia Total: {resumen.SumaDiferencia:C}\n\n" +
                                 $"¿Desea enviar este resumen (Corte Z) por correo al administrador?";

                    var result = momospos.Views.CustomMessageBox.Show(msg, "Resumen Corte Z", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        EnviarCorteZPorCorreo(fecha, resumen);
                    }
                }
                catch (Exception ex)
                {
                    momospos.Views.CustomMessageBox.Show($"Error al generar Corte Z: {ex.Message}");
                }
            }
        }
        
        private void EnviarCorteZPorCorreo(DateTime fecha, (int TotalCortes, decimal SumaEsperada, decimal SumaContada, decimal SumaDiferencia, decimal FondoTotal) resumen)
        {
            try
            {
                var configRepo = new ConfiguracionRepository();
                string emisor = configRepo.ObtenerValor("EmailEmisor");
                string pass = configRepo.ObtenerValor("EmailPassword");
                string destino = configRepo.ObtenerValor("EmailDestino");
                string negocio = configRepo.ObtenerValor("NombreNegocio");

                if (!string.IsNullOrEmpty(emisor) && !string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(destino))
                {
                    using (var mail = new System.Net.Mail.MailMessage())
                    {
                        mail.From = new System.Net.Mail.MailAddress(emisor, negocio);
                        mail.To.Add(destino);
                        mail.Subject = $"Corte Z del Día - {fecha:dd/MM/yyyy}";
                        mail.Body = $"Resumen del Corte Z para el día {fecha:dd/MM/yyyy}:\n\n" +
                                    $"Total de Cortes en el día: {resumen.TotalCortes}\n" +
                                    $"Fondo Total: {resumen.FondoTotal:C}\n" +
                                    $"Suma Efectivo Esperado: {resumen.SumaEsperada:C}\n" +
                                    $"Suma Efectivo Contado (Físico): {resumen.SumaContada:C}\n" +
                                    $"Diferencia Total (Sobrante/Faltante): {resumen.SumaDiferencia:C}\n\n" +
                                    $"Generado por: {_usuarioActual.Nombre} a las {DateTime.Now:HH:mm}";

                        using (var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.Credentials = new System.Net.NetworkCredential(emisor, pass);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                    momospos.Views.CustomMessageBox.Show("Corte Z enviado exitosamente al administrador.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    momospos.Views.CustomMessageBox.Show("Faltan configurar las credenciales de correo en Configuración > Correo / Notificaciones.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al enviar correo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void AjustarAlineacion(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                Type t = col.ValueType;
                if (t != null)
                {
                    // Desempaquetar Nullable (ej. decimal?)
                    t = Nullable.GetUnderlyingType(t) ?? t;

                    if (t == typeof(decimal) || t == typeof(int) || t == typeof(double))
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    else
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    }
                }
                
                // Excepciones para columnas de botones
                if (col.Name == "Imprimir" || col.Name == "Pdf" || col.Name == "Anotar")
                {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }
    }
}
