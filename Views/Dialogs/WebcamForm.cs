using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace momospos.Views.Dialogs
{
    public class WebcamForm : Form
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private ComboBox _cbCamaras;
        private PictureBox _picVideo;
        private Button _btnCapturar;
        private Button _btnCancelar;
        private Button _btnReintentar;
        private Button _btnAceptar;

        public Image ImagenCapturada { get; private set; }

        public WebcamForm()
        {
            BuildUI();
            CargarCamaras();
        }

        private void BuildUI()
        {
            this.Text = "Tomar Foto con Webcam";
            this.Size = new Size(640, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label { Text = "📷 Captura de Receta", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.PrimaryColor, AutoSize = true, Location = new Point(20, 20) };
            
            Label lblCamara = new Label { Text = "Cámara:", Font = Theme.FontNormal, AutoSize = true, Location = new Point(20, 65) };
            _cbCamaras = new ComboBox { Location = new Point(100, 62), Width = 300, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            _cbCamaras.SelectedIndexChanged += CbCamaras_SelectedIndexChanged;

            _picVideo = new PictureBox { Location = new Point(20, 100), Size = new Size(580, 350), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };

            _btnCapturar = new Button { Text = "🔴 Tomar Foto", Location = new Point(170, 470), Width = 140, Height = 40 };
            Theme.StyleButton(_btnCapturar, Theme.DangerColor);
            _btnCapturar.Click += BtnCapturar_Click;

            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(330, 470), Width = 100, Height = 40 };
            Theme.StyleButton(_btnCancelar, Theme.SecondaryColor);
            _btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            _btnReintentar = new Button { Text = "🔄 Reintentar", Location = new Point(170, 470), Width = 140, Height = 40, Visible = false };
            Theme.StyleButton(_btnReintentar, Color.DarkOrange);
            _btnReintentar.Click += BtnReintentar_Click;

            _btnAceptar = new Button { Text = "✅ Aceptar Foto", Location = new Point(330, 470), Width = 140, Height = 40, Visible = false };
            Theme.StyleButton(_btnAceptar, Theme.SuccessColor);
            _btnAceptar.Click += BtnAceptar_Click;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblCamara);
            this.Controls.Add(_cbCamaras);
            this.Controls.Add(_picVideo);
            this.Controls.Add(_btnCapturar);
            this.Controls.Add(_btnCancelar);
            this.Controls.Add(_btnReintentar);
            this.Controls.Add(_btnAceptar);
        }

        private void CargarCamaras()
        {
            try
            {
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_videoDevices.Count == 0)
                {
                    momospos.Views.CustomMessageBox.Show("No se detectó ninguna cámara web.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                foreach (FilterInfo device in _videoDevices)
                {
                    _cbCamaras.Items.Add(device.Name);
                }
                _cbCamaras.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al inicializar cámara: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbCamaras_SelectedIndexChanged(object sender, EventArgs e)
        {
            IniciarCamara();
        }

        private void IniciarCamara()
        {
            DetenerCamara();
            if (_cbCamaras.SelectedIndex >= 0)
            {
                _videoSource = new VideoCaptureDevice(_videoDevices[_cbCamaras.SelectedIndex].MonikerString);
                _videoSource.NewFrame += VideoSource_NewFrame;
                _videoSource.Start();
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            _picVideo.Image = frame;
        }

        private void DetenerCamara()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource.NewFrame -= VideoSource_NewFrame;
                _videoSource = null;
            }
        }

        private void BtnCapturar_Click(object sender, EventArgs e)
        {
            if (_picVideo.Image != null)
            {
                ImagenCapturada = (Image)_picVideo.Image.Clone();
                DetenerCamara();
                
                _btnCapturar.Visible = false;
                _btnCancelar.Visible = false;
                _cbCamaras.Enabled = false;

                _btnReintentar.Visible = true;
                _btnAceptar.Visible = true;
            }
        }

        private void BtnReintentar_Click(object sender, EventArgs e)
        {
            ImagenCapturada = null;
            _btnCapturar.Visible = true;
            _btnCancelar.Visible = true;
            _cbCamaras.Enabled = true;

            _btnReintentar.Visible = false;
            _btnAceptar.Visible = false;
            
            IniciarCamara();
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (ImagenCapturada != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DetenerCamara();
            base.OnFormClosing(e);
        }
    }
}
