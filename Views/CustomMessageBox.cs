using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace momospos.Views
{
    public class CustomMessageBox : Form
    {
        private Panel panelTitleBar;
        private Label lblTitle;
        private Label lblMessage;
        private Button btnOk;
        private Button btnCancel;
        private Button btnYes;
        private Button btnNo;
        private PictureBox pbIcon;
        private Panel panelButtons;

        private DialogResult _result = DialogResult.None;

        public CustomMessageBox(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();

            lblMessage.Text = text;
            lblTitle.Text = title;

            // Ajustar alto basado en el texto (Simple approach)
            int estimatedHeight = TextRenderer.MeasureText(text, lblMessage.Font, new Size(lblMessage.Width, int.MaxValue), TextFormatFlags.WordBreak).Height;
            if (estimatedHeight > lblMessage.Height)
            {
                this.Height += (estimatedHeight - lblMessage.Height);
            }

            ConfigureButtons(buttons);
            ConfigureIcon(icon);
            StyleComponents();
        }

        private void InitializeComponent()
        {
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            
            this.panelTitleBar.SuspendLayout();
            this.panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = momospos.Views.Theme.PrimaryColor;
            this.panelTitleBar.Controls.Add(this.lblTitle);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(420, 40);
            this.panelTitleBar.TabIndex = 0;
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = momospos.Views.Theme.FontNormalBold;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(43, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Title";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // pbIcon
            // 
            this.pbIcon.Location = new System.Drawing.Point(20, 60);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(48, 48);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIcon.TabIndex = 3;
            this.pbIcon.TabStop = false;
            // 
            // lblMessage
            // 
            this.lblMessage.Font = momospos.Views.Theme.FontNormal;
            this.lblMessage.ForeColor = momospos.Views.Theme.TextDark;
            this.lblMessage.Location = new System.Drawing.Point(85, 60);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(315, 65);
            this.lblMessage.TabIndex = 1;
            this.lblMessage.Text = "Message";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.White;
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Controls.Add(this.btnNo);
            this.panelButtons.Controls.Add(this.btnYes);
            this.panelButtons.Controls.Add(this.btnOk);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 140);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(420, 60);
            this.panelButtons.TabIndex = 2;
            // 
            // Botones
            // 
            this.btnOk.Location = new System.Drawing.Point(310, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(90, 36);
            this.btnOk.Text = "Aceptar";
            this.btnOk.Visible = false;
            this.btnOk.Click += (s, e) => { _result = DialogResult.OK; this.Close(); };
            
            this.btnCancel.Location = new System.Drawing.Point(210, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 36);
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Visible = false;
            this.btnCancel.Click += (s, e) => { _result = DialogResult.Cancel; this.Close(); };

            this.btnYes.Location = new System.Drawing.Point(210, 12);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(90, 36);
            this.btnYes.Text = "Sí";
            this.btnYes.Visible = false;
            this.btnYes.Click += (s, e) => { _result = DialogResult.Yes; this.Close(); };

            this.btnNo.Location = new System.Drawing.Point(310, 12);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(90, 36);
            this.btnNo.Text = "No";
            this.btnNo.Visible = false;
            this.btnNo.Click += (s, e) => { _result = DialogResult.No; this.Close(); };

            // 
            // CustomMessageBox
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(420, 200);
            this.Controls.Add(this.pbIcon);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.panelTitleBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CustomMessageBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            
            this.panelTitleBar.ResumeLayout(false);
            this.panelTitleBar.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);
        }

        private void StyleComponents()
        {
            // Bordes Redondeados (12px)
            GraphicsPath path = new GraphicsPath();
            int r = 12;
            path.AddArc(0, 0, r*2, r*2, 180, 90);
            path.AddArc(this.Width - r*2, 0, r*2, r*2, 270, 90);
            path.AddArc(this.Width - r*2, this.Height - r*2, r*2, r*2, 0, 90);
            path.AddArc(0, this.Height - r*2, r*2, r*2, 90, 90);
            this.Region = new Region(path);

            // Colorear botones basados en la decisión del usuario (PrimaryColor)
            momospos.Views.Theme.StyleButton(btnOk, momospos.Views.Theme.PrimaryColor);
            momospos.Views.Theme.StyleButton(btnYes, momospos.Views.Theme.PrimaryColor);
            momospos.Views.Theme.StyleButton(btnNo, momospos.Views.Theme.SecondaryColor);
            momospos.Views.Theme.StyleButton(btnCancel, momospos.Views.Theme.SecondaryColor);
        }

        private void ConfigureButtons(MessageBoxButtons buttons)
        {
            int margin = 10;
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    btnOk.Visible = true;
                    btnOk.Left = this.Width - btnOk.Width - margin;
                    break;
                case MessageBoxButtons.OKCancel:
                    btnCancel.Visible = true;
                    btnOk.Visible = true;
                    btnOk.Left = this.Width - btnOk.Width - margin;
                    btnCancel.Left = btnOk.Left - btnCancel.Width - margin;
                    break;
                case MessageBoxButtons.YesNo:
                    btnNo.Visible = true;
                    btnYes.Visible = true;
                    btnNo.Left = this.Width - btnNo.Width - margin;
                    btnYes.Left = btnNo.Left - btnYes.Width - margin;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    btnCancel.Visible = true;
                    btnNo.Visible = true;
                    btnYes.Visible = true;
                    btnCancel.Left = this.Width - btnCancel.Width - margin;
                    btnNo.Left = btnCancel.Left - btnNo.Width - margin;
                    btnYes.Left = btnNo.Left - btnYes.Width - margin;
                    break;
            }
        }

        private void ConfigureIcon(MessageBoxIcon icon)
        {
            if (icon == MessageBoxIcon.None)
            {
                pbIcon.Visible = false;
                lblMessage.Left = 20;
                lblMessage.Width = this.Width - 40;
                return;
            }

            // Crear icono plano y moderno
            Bitmap bmp = new Bitmap(48, 48, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Transparent);

                Color bgColor;
                string symbol = "";
                Font symbolFont = new Font("Segoe UI", 24, FontStyle.Bold);

                switch (icon)
                {
                    case MessageBoxIcon.Error:
                        bgColor = momospos.Views.Theme.DangerColor;
                        symbol = "✕";
                        panelTitleBar.BackColor = bgColor;
                        break;
                    case MessageBoxIcon.Warning:
                        bgColor = momospos.Views.Theme.WarningColor;
                        symbol = "!";
                        panelTitleBar.BackColor = bgColor;
                        break;
                    case MessageBoxIcon.Information:
                        bgColor = momospos.Views.Theme.PrimaryColor;
                        symbol = "i";
                        break;
                    case MessageBoxIcon.Question:
                        bgColor = momospos.Views.Theme.PrimaryColor;
                        symbol = "?";
                        break;
                    default:
                        bgColor = momospos.Views.Theme.PrimaryColor;
                        symbol = "";
                        break;
                }

                // Dibujar círculo de fondo
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    g.FillEllipse(brush, new Rectangle(2, 2, 44, 44));
                }

                // Dibujar símbolo centrado
                if (!string.IsNullOrEmpty(symbol))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    // Ajuste vertical muy sutil dependiendo del caracter
                    Rectangle rect = new Rectangle(0, symbol == "✕" ? 2 : 0, 48, 48);
                    g.DrawString(symbol, symbolFont, Brushes.White, rect, sf);
                }
                
                symbolFont.Dispose();
            }

            pbIcon.Image = bmp;
        }

        // Permitir arrastrar la ventana sin bordes
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        public static DialogResult Show(string text, string caption = "MomosPOS", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            DialogResult result = DialogResult.OK; // Default
            
            // Invoke required fix for thread safety
            if (Application.OpenForms.Count > 0 && Application.OpenForms[0].InvokeRequired)
            {
                Application.OpenForms[0].Invoke(new Action(() => {
                    result = ShowInternal(text, caption, buttons, icon);
                }));
                return result;
            }

            return ShowInternal(text, caption, buttons, icon);
        }

        private static DialogResult ShowInternal(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            DialogResult result;
            Form owner = Form.ActiveForm;
            
            if (owner != null && !owner.IsDisposed && owner.Visible)
            {
                using (Form dimForm = new Form())
                {
                    dimForm.BackColor = Color.Black;
                    dimForm.Opacity = 0.5;
                    dimForm.FormBorderStyle = FormBorderStyle.None;
                    dimForm.ShowInTaskbar = false;
                    dimForm.StartPosition = FormStartPosition.Manual;
                    dimForm.Location = owner.Location;
                    dimForm.Size = owner.Size;

                    // Show dimForm before opening the actual MessageBox
                    dimForm.Show(owner);

                    using (CustomMessageBox msgBox = new CustomMessageBox(text, caption, buttons, icon))
                    {
                        msgBox.StartPosition = FormStartPosition.CenterParent;
                        msgBox.ShowDialog(dimForm);
                        result = msgBox._result;
                    }
                    
                    dimForm.Close();
                }
            }
            else
            {
                using (CustomMessageBox msgBox = new CustomMessageBox(text, caption, buttons, icon))
                {
                    msgBox.StartPosition = FormStartPosition.CenterScreen;
                    msgBox.ShowDialog();
                    result = msgBox._result;
                }
            }

            return result;
        }
    }
}
