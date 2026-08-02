using System;
using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views.Dialogs
{
    public class CustomInputBoxForm : Form
    {
        private TextBox txtInput;
        public string InputValue { get; private set; } = string.Empty;

        public CustomInputBoxForm(string prompt, string title, string defaultValue = "")
        {
            this.Text = title;
            this.Size = new Size(400, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;
            Theme.SetIcon(this);

            // Top Color Bar
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = Theme.PrimaryColor };
            this.Controls.Add(pnlTop);

            // Content
            Label lblPrompt = new Label
            {
                Text = prompt,
                Font = Theme.FontNormal,
                Location = new Point(20, 30),
                AutoSize = true,
                MaximumSize = new Size(340, 0)
            };
            this.Controls.Add(lblPrompt);

            txtInput = new TextBox
            {
                Text = defaultValue,
                Font = Theme.FontNormal,
                Location = new Point(20, 80),
                Width = 340
            };
            // Select all text on load
            this.Shown += (s, e) => { txtInput.Focus(); txtInput.SelectAll(); };
            
            // Enter key to submit
            txtInput.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Submit(); }
                if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; this.DialogResult = DialogResult.Cancel; this.Close(); }
            };
            
            this.Controls.Add(txtInput);

            // Buttons
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            Button btnCancel = new Button { Text = "Cancelar", Width = 100, Height = 40, Margin = new Padding(5, 0, 5, 0) };
            Theme.StyleButton(btnCancel, Color.Gray, Theme.TextLight, Theme.FontNormal);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            
            Button btnOk = new Button { Text = "Aceptar", Width = 100, Height = 40, Margin = new Padding(5, 0, 5, 0) };
            Theme.StyleButton(btnOk, Theme.PrimaryColor, Theme.TextLight, Theme.FontNormal);
            btnOk.Click += (s, e) => Submit();

            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnOk);

            this.Controls.Add(pnlButtons);
        }

        private void Submit()
        {
            InputValue = txtInput.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
