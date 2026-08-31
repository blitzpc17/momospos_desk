using System;
using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views.Dialogs
{
    public class CustomMessageBoxForm : Form
    {
        private Button btnOk;
        private Button btnYes;
        private Button btnNo;
        private Button btnCancel;
        
        public DialogResult Result { get; private set; } = DialogResult.None;

        public CustomMessageBoxForm(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            this.Text = title;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;
            Theme.SetIcon(this);

            // Icon Panel
            Panel pnlIcon = new Panel { Dock = DockStyle.Left, Width = 80 };
            Label lblIcon = new Label { 
                Font = new Font("Segoe UI Emoji", 36), 
                AutoSize = false, 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleCenter 
            };
            
            Color titleColor = Theme.PrimaryColor;

            switch (icon)
            {
                case MessageBoxIcon.Error:
                    lblIcon.Text = "❌";
                    titleColor = Theme.DangerColor;
                    break;
                case MessageBoxIcon.Warning:
                    lblIcon.Text = "⚠️";
                    titleColor = Theme.WarningColor;
                    break;
                case MessageBoxIcon.Information:
                    lblIcon.Text = "ℹ️";
                    titleColor = Theme.PrimaryColor;
                    break;
                case MessageBoxIcon.Question:
                    lblIcon.Text = "❓";
                    titleColor = Theme.PrimaryColor;
                    break;
                default:
                    lblIcon.Text = "";
                    break;
            }
            pnlIcon.Controls.Add(lblIcon);

            // Top Color Bar
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = titleColor };

            // Content Panel
            Panel pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 30, 20, 20) };
            Label lblMessage = new Label
            {
                Text = message,
                Font = Theme.FontNormal,
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(420, 0)
            };
            pnlContent.Controls.Add(lblMessage);
            
            // Adjust form size EXACTLY to the needed text height
            int neededHeight = lblMessage.PreferredHeight + 160; 
            this.Size = new Size(550, Math.Max(220, neededHeight));

            // Buttons Panel
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            // Configurar botones según enumerador
            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                btnNo = CreateButton("No", Color.Gray, DialogResult.No);
                pnlButtons.Controls.Add(btnNo);

                btnYes = CreateButton("Sí", titleColor, DialogResult.Yes);
                pnlButtons.Controls.Add(btnYes);
            }
            else if (buttons == MessageBoxButtons.OKCancel)
            {
                btnCancel = CreateButton("Cancelar", Color.Gray, DialogResult.Cancel);
                pnlButtons.Controls.Add(btnCancel);

                btnOk = CreateButton("Aceptar", titleColor, DialogResult.OK);
                pnlButtons.Controls.Add(btnOk);
            }
            else // OK por defecto
            {
                btnOk = CreateButton("Aceptar", titleColor, DialogResult.OK);
                pnlButtons.Controls.Add(btnOk);
            }

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlIcon);
            this.Controls.Add(pnlButtons);
            this.Controls.Add(pnlTop);
        }

        private Button CreateButton(string text, Color backColor, DialogResult result)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 100,
                Height = 40,
                Margin = new Padding(5, 0, 5, 0)
            };
            Theme.StyleButton(btn, backColor, Theme.TextLight, Theme.FontNormal);
            btn.Click += (s, e) => { this.Result = result; this.DialogResult = result; this.Close(); };
            return btn;
        }
    }
}
