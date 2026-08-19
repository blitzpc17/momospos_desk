using System;
using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views.Dialogs
{
    public class RecetaMedicaForm : Form
    {
        private TextBox txtNombreMedico;
        private TextBox txtCedula;
        private Button btnAceptar;
        private Button btnCancelar;

        public string NombreMedico { get; private set; }
        public string Cedula { get; private set; }
        public bool RecetaRetenida { get; private set; }
        public string RecetaRutaImagen { get; private set; }

        private CheckBox chkRetenida;
        private Button btnAdjuntar;
        private Button btnWebcam;
        private Label lblArchivoAdjunto;

        public RecetaMedicaForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Datos de Receta Médica";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label
            {
                Text = "📝 Registro de Receta Médica",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Theme.PrimaryColor,
                AutoSize = true,
                Location = new Point(30, 20)
            };

            Label lblInfo = new Label
            {
                Text = "Al menos uno de los productos requiere receta médica obligatoria (e.g. Antibiótico). Por favor capture los datos del médico para poder continuar.",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 60),
                Size = new Size(420, 45)
            };

            Label lblNombre = new Label { Text = "Nombre del Médico:", Location = new Point(30, 115), AutoSize = true, Font = Theme.FontNormal };
            txtNombreMedico = new TextBox { Location = new Point(30, 140), Width = 420, Font = new Font("Segoe UI", 12) };

            Label lblCedula = new Label { Text = "Cédula Profesional:", Location = new Point(30, 185), AutoSize = true, Font = Theme.FontNormal };
            txtCedula = new TextBox { Location = new Point(30, 210), Width = 420, Font = new Font("Segoe UI", 12) };

            chkRetenida = new CheckBox { Text = "¿La receta se retiene en farmacia?", Font = Theme.FontNormal, Location = new Point(30, 250), AutoSize = true };
            
            btnAdjuntar = new Button { Text = "📎 Adjuntar Archivo", Location = new Point(30, 280), Width = 150, Height = 35 };
            Theme.StyleButton(btnAdjuntar, Color.Teal);
            btnAdjuntar.Click += BtnAdjuntar_Click;

            btnWebcam = new Button { Text = "📷 Usar Webcam", Location = new Point(190, 280), Width = 150, Height = 35 };
            Theme.StyleButton(btnWebcam, Color.DarkOrange);
            btnWebcam.Click += BtnWebcam_Click;

            lblArchivoAdjunto = new Label { Text = "Ningún archivo o foto seleccionado", Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(350, 290), AutoSize = true };

            btnAceptar = new Button { Text = "Aceptar", Location = new Point(220, 340), Width = 110, Height = 40 };
            Theme.StyleButton(btnAceptar, Theme.PrimaryColor);
            btnAceptar.Click += BtnAceptar_Click;

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(340, 340), Width = 110, Height = 40 };
            Theme.StyleButton(btnCancelar, Theme.DangerColor);
            btnCancelar.Click += BtnCancelar_Click;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombreMedico);
            this.Controls.Add(lblCedula);
            this.Controls.Add(txtCedula);
            this.Controls.Add(chkRetenida);
            this.Controls.Add(btnAdjuntar);
            this.Controls.Add(btnWebcam);
            this.Controls.Add(lblArchivoAdjunto);
            this.Controls.Add(btnAceptar);
            this.Controls.Add(btnCancelar);

            this.Size = new Size(500, 450);
        }

        private void BtnAdjuntar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.pdf)|*.jpg;*.jpeg;*.png;*.pdf";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    RecetaRutaImagen = ofd.FileName;
                    lblArchivoAdjunto.Text = System.IO.Path.GetFileName(ofd.FileName);
                }
            }
        }

        private void BtnWebcam_Click(object sender, EventArgs e)
        {
            var webcamForm = new WebcamForm();
            if (webcamForm.ShowDialog() == DialogResult.OK && webcamForm.ImagenCapturada != null)
            {
                try
                {
                    string targetDir = System.IO.Path.Combine(momospos.Helpers.ConfiguracionHelper.ObtenerRutaRecursos(), "Recetas");
                    if (!System.IO.Directory.Exists(targetDir))
                    {
                        System.IO.Directory.CreateDirectory(targetDir);
                    }
                    string fileName = $"RecetaWebcam_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}.jpg";
                    string targetPath = System.IO.Path.Combine(targetDir, fileName);
                    
                    webcamForm.ImagenCapturada.Save(targetPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    RecetaRutaImagen = targetPath;
                    lblArchivoAdjunto.Text = "Foto capturada con éxito";
                }
                catch (Exception ex)
                {
                    CustomDialog.ShowWarning("Error al guardar la foto: " + ex.Message);
                }
            }
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreMedico.Text))
            {
                CustomDialog.ShowWarning("El nombre del médico es obligatorio.");
                txtNombreMedico.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                CustomDialog.ShowWarning("La cédula profesional del médico es obligatoria.");
                txtCedula.Focus();
                return;
            }

            NombreMedico = txtNombreMedico.Text.Trim();
            Cedula = txtCedula.Text.Trim();
            RecetaRetenida = chkRetenida.Checked;

            if (RecetaRetenida && string.IsNullOrEmpty(RecetaRutaImagen))
            {
                CustomDialog.ShowWarning("Para medicamento controlado con receta retenida, ES OBLIGATORIO adjuntar o tomar la foto de la receta como comprobante legal.");
                return;
            }

            // Copy file to local directory if one was selected (and it's not already in the target dir like the webcam ones)
            string expectedBaseDir = System.IO.Path.Combine(momospos.Helpers.ConfiguracionHelper.ObtenerRutaRecursos(), "Recetas");
            if (!string.IsNullOrEmpty(RecetaRutaImagen) && !RecetaRutaImagen.StartsWith(expectedBaseDir))
            {
                try
                {
                    string targetDir = expectedBaseDir;
                    if (!System.IO.Directory.Exists(targetDir))
                    {
                        System.IO.Directory.CreateDirectory(targetDir);
                    }
                    string extension = System.IO.Path.GetExtension(RecetaRutaImagen);
                    string fileName = $"Receta_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
                    string targetPath = System.IO.Path.Combine(targetDir, fileName);
                    System.IO.File.Copy(RecetaRutaImagen, targetPath, true);
                    RecetaRutaImagen = targetPath; // Guardamos la ruta permanente
                }
                catch (Exception ex)
                {
                    CustomDialog.ShowWarning("No se pudo copiar el archivo a la carpeta de recetas local: " + ex.Message);
                    // Seguimos con la ruta original en su defecto
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
