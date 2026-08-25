using System;
using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views
{
    public static class CustomMessageBox
    {
        public static DialogResult Show(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            Form form = new Form();
            form.Text = title;
            form.Size = new Size(450, 220);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.None;
            form.BackColor = Theme.BackgroundColor;
            form.ShowInTaskbar = false;

            // Borde sutil
            form.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, form.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
            };

            // Barra superior
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Theme.PrimaryColor };
            Label lblTitle = new Label { 
                Text = title, 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            topBar.Controls.Add(lblTitle);
            form.Controls.Add(topBar);

            // Contenido
            Label lblMessage = new Label { 
                Text = message, 
                Font = new Font("Segoe UI", 12), 
                ForeColor = Theme.TextDark,
                Location = new Point(30, 70),
                AutoSize = true,
                MaximumSize = new Size(390, 0),
                TextAlign = ContentAlignment.TopCenter
            };
            form.Controls.Add(lblMessage);

            // Ajustar altura de la ventana
            int neededHeight = lblMessage.PreferredHeight + 150; 
            form.Size = new Size(450, Math.Max(220, neededHeight));

            // Panel inferior para botones
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            
            DialogResult result = DialogResult.None;

            if (buttons == MessageBoxButtons.OK)
            {
                Button btnOk = new Button { Text = "Aceptar", Width = 120, Height = 40, Location = new Point(165, 10) };
                Theme.StyleButton(btnOk, Theme.PrimaryColor);
                btnOk.Click += (s, e) => { result = DialogResult.OK; form.Close(); };
                bottomPanel.Controls.Add(btnOk);
            }
            else if (buttons == MessageBoxButtons.YesNo)
            {
                Button btnYes = new Button { Text = "Sí", Width = 100, Height = 40, Location = new Point(110, 10) };
                Theme.StyleButton(btnYes, Theme.PrimaryColor);
                btnYes.Click += (s, e) => { result = DialogResult.Yes; form.Close(); };
                
                Button btnNo = new Button { Text = "No", Width = 100, Height = 40, Location = new Point(240, 10) };
                Theme.StyleButton(btnNo, Theme.SecondaryColor);
                btnNo.Click += (s, e) => { result = DialogResult.No; form.Close(); };
                
                bottomPanel.Controls.Add(btnYes);
                bottomPanel.Controls.Add(btnNo);
            }

            form.Controls.Add(bottomPanel);
            
            // Permitir mover la ventana desde la barra
            bool dragging = false;
            Point dragCursorPoint = Point.Empty;
            Point dragFormPoint = Point.Empty;
            
            topBar.MouseDown += (s, e) => { dragging = true; dragCursorPoint = Cursor.Position; dragFormPoint = form.Location; };
            topBar.MouseMove += (s, e) => {
                if (dragging)
                {
                    Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                    form.Location = Point.Add(dragFormPoint, new Size(dif));
                }
            };
            topBar.MouseUp += (s, e) => { dragging = false; };
            lblTitle.MouseDown += (s, e) => { topBar.Invoke(new Action(() => topBar.Focus())); dragging = true; dragCursorPoint = Cursor.Position; dragFormPoint = form.Location; };
            lblTitle.MouseMove += (s, e) => {
                if (dragging)
                {
                    Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                    form.Location = Point.Add(dragFormPoint, new Size(dif));
                }
            };
            lblTitle.MouseUp += (s, e) => { dragging = false; };

            form.ShowDialog();
            return result;
        }
    }
}
