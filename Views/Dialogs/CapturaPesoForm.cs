using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace momospos.Views.Dialogs
{
    public class CapturaPesoForm : Form
    {
        private Producto _producto;
        private string _puerto;
        
        private Label lblPeso;
        private TextBox txtPrecio;
        private Label lblSubtotal;
        private Label lblStatus;
        
        private Button btnAceptar;
        private Button btnManual;
        private Button btnConfigurar;
        private Button btnCancelar;
        
        private System.Windows.Forms.Timer _timer;
        private SerialPort _serialPort;
        
        public decimal PesoCapturado { get; private set; }
        public decimal PrecioFinal { get; private set; }
        
        // This indicates if the user wants to fallback to manual capture
        public bool UsarCapturaManual { get; private set; }
        // This indicates if the user wants to go to configuration
        public bool IrAConfiguracion { get; private set; }

        public CapturaPesoForm(Producto producto, string puerto)
        {
            _producto = producto;
            _puerto = puerto;
            PesoCapturado = 0;
            PrecioFinal = producto.PrecioVenta;
            UsarCapturaManual = false;
            IrAConfiguracion = false;
            
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Captura de Peso en Báscula";
            this.Size = new Size(800, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true; // To handle F5, F4, Esc, Enter
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Leyendo peso para: " + _producto.Nombre, Font = Theme.FontTitle, ForeColor = Theme.TextLight, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            // Center Panel (Digital Display)
            Panel displayPanel = new Panel { 
                Location = new Point(20, 80), 
                Size = new Size(740, 180), 
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(displayPanel);

            // PESO
            Label lblPesoTitle = new Label { Text = "PESO", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(10, 15) };
            displayPanel.Controls.Add(lblPesoTitle);
            
            lblPeso = new Label { 
                Text = "0.000", 
                Font = new Font("Segoe UI", 54, FontStyle.Bold), 
                ForeColor = Color.Cyan,
                AutoSize = true, 
                Location = new Point(-5, 40) 
            };
            displayPanel.Controls.Add(lblPeso);

            // PRECIO
            Label lblPrecioTitle = new Label { Text = "PRECIO / KG ($)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(300, 15) };
            displayPanel.Controls.Add(lblPrecioTitle);
            
            txtPrecio = new TextBox {
                Text = _producto.PrecioVenta.ToString("0.00"),
                Font = new Font("Segoe UI", 48, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None,
                Location = new Point(300, 55),
                Width = 180,
                TextAlign = HorizontalAlignment.Center
            };
            // Remove Enter beep from textbox
            txtPrecio.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Aceptar(); } };
            displayPanel.Controls.Add(txtPrecio);

            // SUBTOTAL
            Label lblSubtotalTitle = new Label { Text = "SUBTOTAL ($)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(520, 15) };
            displayPanel.Controls.Add(lblSubtotalTitle);
            
            lblSubtotal = new Label { 
                Text = "$0.00", 
                Font = new Font("Segoe UI", 54, FontStyle.Bold), 
                ForeColor = Color.LimeGreen,
                AutoSize = true, 
                Location = new Point(505, 40) 
            };
            displayPanel.Controls.Add(lblSubtotal);

            // Status below display
            lblStatus = new Label { Text = "Conectando con báscula...", Font = new Font("Segoe UI", 12, FontStyle.Italic), ForeColor = Color.Gray, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(20, 275), Size = new Size(740, 30) };
            this.Controls.Add(lblStatus);
            
            // Buttons at the bottom
            int btnY = 320;
            
            btnAceptar = new Button { Text = "Aceptar (Enter)", Location = new Point(40, btnY), Width = 160, Height = 60 };
            Theme.StyleButton(btnAceptar, Theme.SuccessColor);
            btnAceptar.Click += (s, e) => Aceptar();
            this.Controls.Add(btnAceptar);

            btnManual = new Button { Text = "Captura Manual (F5)", Location = new Point(220, btnY), Width = 180, Height = 60 };
            Theme.StyleButton(btnManual, Color.DarkOrange);
            btnManual.Click += (s, e) => { UsarCapturaManual = true; this.DialogResult = DialogResult.Yes; this.Close(); };
            this.Controls.Add(btnManual);

            btnConfigurar = new Button { Text = "Configurar (F4)", Location = new Point(420, btnY), Width = 160, Height = 60 };
            Theme.StyleButton(btnConfigurar, Color.Teal);
            btnConfigurar.Click += (s, e) => { IrAConfiguracion = true; this.DialogResult = DialogResult.Retry; this.Close(); };
            this.Controls.Add(btnConfigurar);
            
            btnCancelar = new Button { Text = "Cancelar (Esc)", Location = new Point(600, btnY), Width = 140, Height = 60 };
            Theme.StyleButton(btnCancelar, Color.Gray);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancelar);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            try
            {
                if (string.IsNullOrWhiteSpace(_puerto))
                    throw new Exception("Puerto no especificado.");
                    
                _serialPort = new SerialPort(_puerto, 9600, Parity.None, 8, StopBits.One);
                _serialPort.ReadTimeout = 400; // Shorter timeout for continuous polling
                _serialPort.WriteTimeout = 200;
                _serialPort.Open();
                
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                
                lblStatus.Text = "Báscula conectada. Esperando peso...";
                lblStatus.ForeColor = Color.Green;
                
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 500;
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error de conexión: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;
            
            try
            {
                _serialPort.Write("P");
                
                // Allow a small amount of time for the scale to answer
                // We use a small thread sleep here, but it's safe because it's only 100ms
                Thread.Sleep(100);
                
                string readData = "";
                for(int i = 0; i < 3; i++)
                {
                    if(_serialPort.BytesToRead > 0)
                    {
                        readData += _serialPort.ReadExisting();
                        if(readData.Contains("\r") || readData.Contains("\n") || readData.ToLower().Contains("kg"))
                            break;
                    }
                    Thread.Sleep(50);
                }

                if (string.IsNullOrWhiteSpace(readData))
                {
                    if (_serialPort.BytesToRead > 0)
                        readData = _serialPort.ReadLine();
                }
                
                if (!string.IsNullOrWhiteSpace(readData))
                {
                    decimal peso = ExtraerPeso(readData);
                    PesoCapturado = peso;
                    
                    // Parse custom price from the textbox
                    decimal precioActual = _producto.PrecioVenta;
                    decimal.TryParse(txtPrecio.Text, out precioActual);
                    
                    // Update UI safely
                    if (peso < 1m)
                    {
                        lblPeso.Text = (peso * 1000m).ToString("N0") + " gr";
                    }
                    else
                    {
                        lblPeso.Text = peso.ToString("N3") + " kg";
                    }
                    lblSubtotal.Text = (peso * precioActual).ToString("C2");
                    lblStatus.Text = "Leyendo peso...";
                    lblStatus.ForeColor = Color.Green;
                }
            }
            catch (TimeoutException)
            {
                // Silently ignore timeouts to avoid interrupting the live feed
                lblStatus.Text = "Esperando respuesta de báscula...";
                lblStatus.ForeColor = Color.Orange;
            }
            catch (Exception ex)
            {
                // Other exceptions (like port closed suddenly)
                lblStatus.Text = "Error al leer: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }
        
        private decimal ExtraerPeso(string rawData)
        {
            Match m = Regex.Match(rawData, @"-?\d+(\.\d+)?");
            if (m.Success)
            {
                if (decimal.TryParse(m.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal peso))
                {
                    return Math.Max(0, peso); // prevent negative weight?
                }
            }
            return PesoCapturado; // Return last known good weight if parse fails
        }
        
        private void Aceptar()
        {
            decimal precio = _producto.PrecioVenta;
            decimal.TryParse(txtPrecio.Text, out precio);
            PrecioFinal = precio;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    try { _serialPort.Close(); } catch { }
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
            
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                Aceptar();
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
            else if (keyData == Keys.F5)
            {
                btnManual.PerformClick();
                return true;
            }
            else if (keyData == Keys.F4)
            {
                btnConfigurar.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
