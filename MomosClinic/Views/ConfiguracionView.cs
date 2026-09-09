using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using momospos.Views;

namespace MomosClinic.Views
{
    public class ConfiguracionView : UserControl
    {
        private momospos.Repositories.ConfiguracionRepository _repo;

        private TextBox txtClinicName;
        private NumericUpDown numAlertMinutos;
        private PictureBox pbLogo;
        private PictureBox pbBanner;
        
        private string rutaLogoTemporal = null;
        private string rutaBannerTemporal = null;

        // Receta
        private PictureBox pbLogoReceta;
        private string rutaLogoRecetaTemporal = null;
        private PictureBox pbMarcaAguaReceta;
        private string rutaMarcaAguaRecetaTemporal = null;
        private Panel pnlColorBase;
        private CheckBox chkRecetaAColor;
        
        // Clinica params
        private DateTimePicker dtpHoraApertura;
        private DateTimePicker dtpHoraCierre;
        private NumericUpDown nudDuracionCita;
        private CheckBox chkAplicaTurnos;
        private ComboBox cbMedicoPorDefecto;

        public ConfiguracionView()
        {
            _repo = new momospos.Repositories.ConfiguracionRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;
            this.Padding = new Padding(20);

            Label lblTitle = new Label { Text = "Configuración de la Clínica", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblTitle);

            TabControl tabControl = new TabControl
            {
                Location = new Point(20, 80),
                Width = 800,
                Height = 550,
                Font = Theme.FontNormal
            };
            this.Controls.Add(tabControl);

            TabPage tabGeneral = new TabPage("General");
            tabGeneral.BackColor = Color.White;
            tabGeneral.Padding = new Padding(20);
            tabControl.TabPages.Add(tabGeneral);

            TabPage tabHorarios = new TabPage("Horarios y Citas");
            tabHorarios.BackColor = Color.White;
            tabHorarios.Padding = new Padding(20);
            tabControl.TabPages.Add(tabHorarios);

            TabPage tabReceta = new TabPage("Receta Clínica");
            tabReceta.BackColor = Color.White;
            tabReceta.Padding = new Padding(20);
            tabControl.TabPages.Add(tabReceta);

            int y = 20;

            // Nombre de la Clínica
            Label lblName = new Label { Text = "Nombre de la Clínica / Médico:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            tabGeneral.Controls.Add(lblName);
            y += 30;
            txtClinicName = new TextBox { Font = Theme.FontNormal, Location = new Point(20, y), Width = 400 };
            tabGeneral.Controls.Add(txtClinicName);

            y += 40;

            // Alerta de Citas
            Label lblAlert = new Label { Text = "Avisar próxima cita antes de (Minutos):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            tabGeneral.Controls.Add(lblAlert);
            y += 30;
            numAlertMinutos = new NumericUpDown { Font = Theme.FontNormal, Location = new Point(20, y), Width = 100, Minimum = 1, Maximum = 120, Value = 15 };
            tabGeneral.Controls.Add(numAlertMinutos);

            y += 40;

            // Logo
            Label lblLogo = new Label { Text = "Logo de la Clínica (Se recomienda imagen cuadrada):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            tabGeneral.Controls.Add(lblLogo);
            y += 30;
            pbLogo = new PictureBox { Location = new Point(20, y), Size = new Size(100, 100), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            tabGeneral.Controls.Add(pbLogo);

            Button btnCambiarLogo = new Button { Text = "Cambiar Logo", Location = new Point(140, y + 30), Width = 150, Height = 40 };
            Theme.StyleButton(btnCambiarLogo, Theme.PrimaryColor);
            btnCambiarLogo.Click += (s, e) => SeleccionarImagen(pbLogo, out rutaLogoTemporal);
            tabGeneral.Controls.Add(btnCambiarLogo);

            y += 120;

            // Banner
            Label lblBanner = new Label { Text = "Banner del Inicio de Sesión (Se recomienda imagen vertical/rectangular):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            tabGeneral.Controls.Add(lblBanner);
            y += 30;
            pbBanner = new PictureBox { Location = new Point(20, y), Size = new Size(150, 200), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            tabGeneral.Controls.Add(pbBanner);

            Button btnCambiarBanner = new Button { Text = "Cambiar Banner", Location = new Point(190, y + 80), Width = 150, Height = 40 };
            Theme.StyleButton(btnCambiarBanner, Theme.PrimaryColor);
            btnCambiarBanner.Click += (s, e) => SeleccionarImagen(pbBanner, out rutaBannerTemporal);
            tabGeneral.Controls.Add(btnCambiarBanner);

            // Segunda Columna de configuraciones -> Ahora en tabHorarios
            int y2 = 20;

            tabHorarios.Controls.Add(new Label { Text = "Hora de Apertura:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y2) });
            y2 += 30;
            dtpHoraApertura = new DateTimePicker { Location = new Point(20, y2), Width = 150, Font = Theme.FontNormal, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            tabHorarios.Controls.Add(dtpHoraApertura);
            y2 += 40;

            tabHorarios.Controls.Add(new Label { Text = "Hora de Cierre:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y2) });
            y2 += 30;
            dtpHoraCierre = new DateTimePicker { Location = new Point(20, y2), Width = 150, Font = Theme.FontNormal, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            tabHorarios.Controls.Add(dtpHoraCierre);
            y2 += 40;

            tabHorarios.Controls.Add(new Label { Text = "Duración Promedio Cita (mins):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y2) });
            y2 += 30;
            nudDuracionCita = new NumericUpDown { Location = new Point(20, y2), Width = 150, Font = Theme.FontNormal, Minimum = 5, Maximum = 120, Value = 30 };
            tabHorarios.Controls.Add(nudDuracionCita);
            y2 += 40;

            chkAplicaTurnos = new CheckBox { Text = "Aplicar Turnos Médicos (Pacientes sin cita previa)", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y2) };
            tabHorarios.Controls.Add(chkAplicaTurnos);
            y2 += 40;

            tabHorarios.Controls.Add(new Label { Text = "Médico por Defecto:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y2) });
            y2 += 30;
            cbMedicoPorDefecto = new ComboBox { Location = new Point(20, y2), Width = 300, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            
            var repoMed = new MomosClinic.Repositories.MedicoRepository();
            var medicos = new System.Collections.Generic.List<MomosClinic.Models.Medico> { new MomosClinic.Models.Medico { Id = 0, NombreCompleto = "Libre / Sin Asignar" } };
            medicos.AddRange(repoMed.ObtenerActivos());
            
            cbMedicoPorDefecto.DataSource = medicos;
            cbMedicoPorDefecto.DisplayMember = "NombreCompleto";
            cbMedicoPorDefecto.ValueMember = "Id";
            tabHorarios.Controls.Add(cbMedicoPorDefecto);

            // Tab Receta
            int ry = 20;
            tabReceta.Controls.Add(new Label { Text = "Logo Principal (Encabezado):", Font = Theme.FontSubtitle, Location = new Point(20, ry), AutoSize = true });
            pbLogoReceta = new PictureBox { Location = new Point(20, ry + 30), Size = new Size(150, 150), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            Button btnSubirLogoReceta = new Button { Text = "Subir Logo", Location = new Point(190, ry + 140), Width = 120, Height = 40 };
            Theme.StyleButton(btnSubirLogoReceta, Theme.SecondaryColor);
            btnSubirLogoReceta.Click += (s, e) => SeleccionarImagen(pbLogoReceta, out rutaLogoRecetaTemporal);
            tabReceta.Controls.Add(pbLogoReceta);
            tabReceta.Controls.Add(btnSubirLogoReceta);

            tabReceta.Controls.Add(new Label { Text = "Marca de Agua (Centro):", Font = Theme.FontSubtitle, Location = new Point(380, ry), AutoSize = true });
            pbMarcaAguaReceta = new PictureBox { Location = new Point(380, ry + 30), Size = new Size(150, 150), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            Button btnSubirMarcaAguaReceta = new Button { Text = "Subir Marca", Location = new Point(550, ry + 140), Width = 120, Height = 40 };
            Theme.StyleButton(btnSubirMarcaAguaReceta, Theme.SecondaryColor);
            btnSubirMarcaAguaReceta.Click += (s, e) => SeleccionarImagen(pbMarcaAguaReceta, out rutaMarcaAguaRecetaTemporal);
            tabReceta.Controls.Add(pbMarcaAguaReceta);
            tabReceta.Controls.Add(btnSubirMarcaAguaReceta);

            ry += 200;
            tabReceta.Controls.Add(new Label { Text = "Colores y Diseño:", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.PrimaryColor, Location = new Point(20, ry), AutoSize = true });
            ry += 40;

            chkRecetaAColor = new CheckBox { Text = "Imprimir en Color (Títulos y Formatos)", Location = new Point(20, ry), AutoSize = true, Font = Theme.FontSubtitle, Checked = true };
            tabReceta.Controls.Add(chkRecetaAColor);

            ry += 40;
            tabReceta.Controls.Add(new Label { Text = "Color Base (Rx, Títulos):", Font = Theme.FontSubtitle, Location = new Point(20, ry + 5), AutoSize = true });
            pnlColorBase = new Panel { Location = new Point(250, ry), Size = new Size(40, 40), BackColor = Color.Blue, BorderStyle = BorderStyle.FixedSingle };
            Button btnElegirColor = new Button { Text = "Elegir Color", Location = new Point(310, ry), Width = 120, Height = 40 };
            Theme.StyleButton(btnElegirColor, Theme.SecondaryColor);
            btnElegirColor.Click += (s, e) => {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.Color = pnlColorBase.BackColor;
                    if (cd.ShowDialog() == DialogResult.OK) pnlColorBase.BackColor = cd.Color;
                }
            };
            tabReceta.Controls.Add(pnlColorBase);
            tabReceta.Controls.Add(btnElegirColor);

            // Botón Guardar (Fuera del tab para que siempre sea visible)
            Button btnGuardar = new Button { Text = "💾 Guardar Cambios", Location = new Point(20, 650), Width = 200, Height = 50 };
            Theme.StyleButton(btnGuardar, Theme.SuccessColor, Color.White, new Font("Segoe UI", 12, FontStyle.Bold));
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);
        }

        private void CargarDatos()
        {
            txtClinicName.Text = _repo.ObtenerValor("ClinicName");
            
            string alertaStr = _repo.ObtenerValor("AlertaMinutosCita");
            if (int.TryParse(alertaStr, out int mins))
            {
                numAlertMinutos.Value = mins;
            }

            string logo = _repo.ObtenerValor("ClinicLogo");
            if (!string.IsNullOrWhiteSpace(logo) && File.Exists(logo))
            {
                pbLogo.Image = CargarImagenSinBloquear(logo);
                rutaLogoTemporal = logo;
            }

            string banner = _repo.ObtenerValor("ClinicBanner");
            if (!string.IsNullOrWhiteSpace(banner) && File.Exists(banner))
            {
                pbBanner.Image = CargarImagenSinBloquear(banner);
                rutaBannerTemporal = banner;
            }

            var confs = _repo.ObtenerTodas();
            if (confs.ContainsKey("HoraAperturaClinica") && confs["HoraAperturaClinica"] != null && DateTime.TryParse(confs["HoraAperturaClinica"], out DateTime hrApe))
                dtpHoraApertura.Value = hrApe;
            if (confs.ContainsKey("HoraCierreClinica") && confs["HoraCierreClinica"] != null && DateTime.TryParse(confs["HoraCierreClinica"], out DateTime hrCie))
                dtpHoraCierre.Value = hrCie;
            if (confs.ContainsKey("DuracionPromedioCitaMinutos") && confs["DuracionPromedioCitaMinutos"] != null && int.TryParse(confs["DuracionPromedioCitaMinutos"], out int dur))
                nudDuracionCita.Value = dur;
            if (confs.ContainsKey("AplicaTurnosMedicos") && confs["AplicaTurnosMedicos"] != null)
                chkAplicaTurnos.Checked = confs["AplicaTurnosMedicos"] == "true" || confs["AplicaTurnosMedicos"] == "True";
            if (confs.ContainsKey("MedicoPorDefectoId") && confs["MedicoPorDefectoId"] != null && int.TryParse(confs["MedicoPorDefectoId"], out int medId))
                cbMedicoPorDefecto.SelectedValue = medId;

            string logoReceta = _repo.ObtenerValor("RutaLogoReceta");
            if (!string.IsNullOrWhiteSpace(logoReceta) && File.Exists(logoReceta))
            {
                pbLogoReceta.Image = CargarImagenSinBloquear(logoReceta);
                rutaLogoRecetaTemporal = logoReceta;
            }

            string marcaReceta = _repo.ObtenerValor("RutaMarcaAguaReceta");
            if (!string.IsNullOrWhiteSpace(marcaReceta) && File.Exists(marcaReceta))
            {
                pbMarcaAguaReceta.Image = CargarImagenSinBloquear(marcaReceta);
                rutaMarcaAguaRecetaTemporal = marcaReceta;
            }

            if (confs.ContainsKey("RecetaColorBase") && !string.IsNullOrEmpty(confs["RecetaColorBase"]))
            {
                try { pnlColorBase.BackColor = ColorTranslator.FromHtml(confs["RecetaColorBase"]); } catch { }
            }

            if (confs.ContainsKey("RecetaAColor") && confs["RecetaAColor"] != null)
                chkRecetaAColor.Checked = confs["RecetaAColor"] == "true";
            else
                chkRecetaAColor.Checked = true;
        }

        /// <summary>
        /// Carga una imagen en memoria sin mantener el archivo bloqueado,
        /// lo cual es indispensable para poder copiar/sobreescribir el archivo después.
        /// </summary>
        private Image CargarImagenSinBloquear(string ruta)
        {
            byte[] bytes = File.ReadAllBytes(ruta);
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        private void SeleccionarImagen(PictureBox pb, out string rutaTemporal)
        {
            rutaTemporal = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Selecciona una imagen";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    rutaTemporal = ofd.FileName;
                    // Usamos MemoryStream para no bloquear el archivo original
                    pb.Image = CargarImagenSinBloquear(rutaTemporal);
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                _repo.GuardarValor("ClinicName", txtClinicName.Text.Trim());
                _repo.GuardarValor("AlertaMinutosCita", numAlertMinutos.Value.ToString());

                _repo.GuardarValor("HoraAperturaClinica", dtpHoraApertura.Value.ToString("HH:mm:ss"));
                _repo.GuardarValor("HoraCierreClinica", dtpHoraCierre.Value.ToString("HH:mm:ss"));
                _repo.GuardarValor("DuracionPromedioCitaMinutos", nudDuracionCita.Value.ToString());
                _repo.GuardarValor("AplicaTurnosMedicos", chkAplicaTurnos.Checked ? "true" : "false");
                if (cbMedicoPorDefecto.SelectedValue != null)
                    _repo.GuardarValor("MedicoPorDefectoId", cbMedicoPorDefecto.SelectedValue.ToString());

                // Copiar imágenes a la carpeta de recursos de la aplicación para que no se pierdan
                string appDir = Path.Combine(Application.StartupPath, "Resources", "ClinicConfig");
                if (!Directory.Exists(appDir)) Directory.CreateDirectory(appDir);

                if (!string.IsNullOrWhiteSpace(rutaLogoTemporal))
                {
                    string ext = Path.GetExtension(rutaLogoTemporal);
                    string dest = Path.Combine(appDir, "logo" + ext);
                    if (rutaLogoTemporal != dest)
                    {
                        File.Copy(rutaLogoTemporal, dest, true);
                        _repo.GuardarValor("ClinicLogo", dest);
                    }
                }

                if (!string.IsNullOrWhiteSpace(rutaBannerTemporal))
                {
                    string ext = Path.GetExtension(rutaBannerTemporal);
                    string dest = Path.Combine(appDir, "banner" + ext);
                    if (rutaBannerTemporal != dest)
                    {
                        File.Copy(rutaBannerTemporal, dest, true);
                        _repo.GuardarValor("ClinicBanner", dest);
                    }
                }

                if (!string.IsNullOrWhiteSpace(rutaLogoRecetaTemporal))
                {
                    string ext = Path.GetExtension(rutaLogoRecetaTemporal);
                    string dest = Path.Combine(appDir, "logo_receta" + ext);
                    if (rutaLogoRecetaTemporal != dest) File.Copy(rutaLogoRecetaTemporal, dest, true);
                    _repo.GuardarValor("RutaLogoReceta", dest);
                    _repo.GuardarValor("RecetaLogoBase64", Convert.ToBase64String(File.ReadAllBytes(dest)));
                }

                if (!string.IsNullOrWhiteSpace(rutaMarcaAguaRecetaTemporal))
                {
                    string ext = Path.GetExtension(rutaMarcaAguaRecetaTemporal);
                    string dest = Path.Combine(appDir, "marca_agua_receta" + ext);
                    if (rutaMarcaAguaRecetaTemporal != dest) File.Copy(rutaMarcaAguaRecetaTemporal, dest, true);
                    _repo.GuardarValor("RutaMarcaAguaReceta", dest);
                    _repo.GuardarValor("RecetaMarcaAguaBase64", Convert.ToBase64String(File.ReadAllBytes(dest)));
                }

                _repo.GuardarValor("RecetaColorBase", ColorTranslator.ToHtml(pnlColorBase.BackColor));
                _repo.GuardarValor("RecetaAColor", chkRecetaAColor.Checked ? "true" : "false");

                CustomMessageBox.Show("Configuración guardada exitosamente.\n\nNota: Algunos cambios (como el nombre en la barra superior) aplicarán al reiniciar el sistema.", "Éxito");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error guardando configuración: " + ex.Message, "Error");
            }
        }
    }
}
