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

            Panel pnlSettings = new Panel
            {
                Location = new Point(20, 80),
                Width = 800,
                Height = 600,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlSettings);

            int y = 20;

            // Nombre de la Clínica
            Label lblName = new Label { Text = "Nombre de la Clínica / Médico:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            pnlSettings.Controls.Add(lblName);
            y += 30;
            txtClinicName = new TextBox { Font = Theme.FontNormal, Location = new Point(20, y), Width = 400 };
            pnlSettings.Controls.Add(txtClinicName);

            y += 40;

            // Alerta de Citas
            Label lblAlert = new Label { Text = "Avisar próxima cita antes de (Minutos):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            pnlSettings.Controls.Add(lblAlert);
            y += 30;
            numAlertMinutos = new NumericUpDown { Font = Theme.FontNormal, Location = new Point(20, y), Width = 100, Minimum = 1, Maximum = 120, Value = 15 };
            pnlSettings.Controls.Add(numAlertMinutos);

            y += 40;

            // Logo
            Label lblLogo = new Label { Text = "Logo de la Clínica (Se recomienda imagen cuadrada):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            pnlSettings.Controls.Add(lblLogo);
            y += 30;
            pbLogo = new PictureBox { Location = new Point(20, y), Size = new Size(100, 100), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            pnlSettings.Controls.Add(pbLogo);

            Button btnCambiarLogo = new Button { Text = "Cambiar Logo", Location = new Point(140, y + 30), Width = 150, Height = 40 };
            Theme.StyleButton(btnCambiarLogo, Theme.PrimaryColor);
            btnCambiarLogo.Click += (s, e) => SeleccionarImagen(pbLogo, out rutaLogoTemporal);
            pnlSettings.Controls.Add(btnCambiarLogo);

            y += 120;

            // Banner
            Label lblBanner = new Label { Text = "Banner del Inicio de Sesión (Se recomienda imagen vertical/rectangular):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(20, y) };
            pnlSettings.Controls.Add(lblBanner);
            y += 30;
            pbBanner = new PictureBox { Location = new Point(20, y), Size = new Size(150, 200), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            pnlSettings.Controls.Add(pbBanner);

            Button btnCambiarBanner = new Button { Text = "Cambiar Banner", Location = new Point(190, y + 80), Width = 150, Height = 40 };
            Theme.StyleButton(btnCambiarBanner, Theme.PrimaryColor);
            btnCambiarBanner.Click += (s, e) => SeleccionarImagen(pbBanner, out rutaBannerTemporal);
            pnlSettings.Controls.Add(btnCambiarBanner);

            // Botón Guardar
            Button btnGuardar = new Button { Text = "💾 Guardar Cambios", Location = new Point(580, 530), Width = 200, Height = 50 };
            Theme.StyleButton(btnGuardar, Theme.SuccessColor, Color.White, new Font("Segoe UI", 12, FontStyle.Bold));
            btnGuardar.Click += BtnGuardar_Click;
            pnlSettings.Controls.Add(btnGuardar);
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

                MessageBox.Show("Configuración guardada exitosamente.\n\nNota: Algunos cambios (como el nombre en la barra superior) aplicarán al reiniciar el sistema.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando configuración: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
