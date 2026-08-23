using System.Drawing;
using System.Windows.Forms;

namespace momospos.Views
{
    public static class Theme
    {
        // Paleta de Colores
        public static readonly Color PrimaryColor = Color.FromArgb(0, 120, 215);    // Azul Moderno
        public static readonly Color SecondaryColor = Color.FromArgb(50, 50, 60);  // Gris Elegante
        public static readonly Color BackgroundColor = Color.FromArgb(248, 249, 250); // Gris casi blanco
        public static readonly Color TextDark = Color.FromArgb(40, 40, 40);
        public static readonly Color TextLight = Color.White;
        
        public static readonly Color SuccessColor = Color.FromArgb(16, 185, 129);   // Esmeralda
        public static readonly Color DangerColor = Color.FromArgb(239, 68, 68);    // Rojo Suave
        public static readonly Color WarningColor = Color.FromArgb(245, 158, 11);  // Naranja Suave

        // Fuentes
        public static readonly Font FontTitle = new Font("Segoe UI", 16, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 14, FontStyle.Bold);
        public static readonly Font FontNormal = new Font("Segoe UI", 11, FontStyle.Regular);
        public static readonly Font FontNormalBold = new Font("Segoe UI", 11, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 9, FontStyle.Regular);
        
        // Icono para formularios
        public static void SetIcon(Form form)
        {
            try
            {
                GenerateLogoIfMissing();
                string iconPath = System.IO.Path.Combine(Application.StartupPath, "Resources", "logo2.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    form.Icon = new Icon(iconPath);
                }
                else
                {
                    // Intentar buscar en la ruta de desarrollo
                    string devPath = System.IO.Path.Combine(Application.StartupPath, "..", "..", "Resources", "logo2.ico");
                    if (System.IO.File.Exists(devPath))
                        form.Icon = new Icon(devPath);
                }
            }
            catch { }
        }

        public static string GetLogoPath()
        {
            GenerateLogoIfMissing();
            string pngPath = System.IO.Path.Combine(Application.StartupPath, "Resources", "logo_drawer2.png");
            if (System.IO.File.Exists(pngPath)) return pngPath;
            
            string devPath = System.IO.Path.Combine(Application.StartupPath, "..", "..", "Resources", "logo_drawer2.png");
            if (System.IO.File.Exists(devPath)) return devPath;

            return null;
        }

        public static string GetLoginLogoPath()
        {
            GenerateLogoIfMissing();
            string pngPath = System.IO.Path.Combine(Application.StartupPath, "Resources", "logo_login2.png");
            if (System.IO.File.Exists(pngPath)) return pngPath;
            return null;
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void GenerateLogoIfMissing()
        {
            try
            {
                string resDir = System.IO.Path.Combine(Application.StartupPath, "Resources");
                if (!System.IO.Directory.Exists(resDir)) System.IO.Directory.CreateDirectory(resDir);
                
                string drawerPngPath = System.IO.Path.Combine(resDir, "logo_drawer2.png");
                string loginPngPath = System.IO.Path.Combine(resDir, "logo_login2.png");
                string icoPath = System.IO.Path.Combine(resDir, "logo2.ico");

                if (System.IO.File.Exists(drawerPngPath) && System.IO.File.Exists(icoPath) && System.IO.File.Exists(loginPngPath))
                    return;

                // 1. Generar icono cuadrado (logo.ico)
                using (Bitmap bmpIco = new Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmpIco))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        g.Clear(Color.Transparent);

                        using (var p = GetRoundedRect(new Rectangle(12, 12, 232, 232), 40))
                            g.FillPath(new SolidBrush(PrimaryColor), p);
                        
                        // Letra "M" centrada
                        using (Font font = new Font("Segoe UI", 120, FontStyle.Bold))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            // Ajuste manual de Y para Segoe UI para que quede visualmente en el centro
                            g.DrawString("M", font, Brushes.White, new Rectangle(0, -15, 256, 256), sf);
                        }

                        // Carrito de compras pequeño como badge
                        using (Font emoji = new Font("Segoe UI Emoji", 45))
                        {
                            g.DrawString("🛒", emoji, Brushes.White, new Point(145, 145));
                        }
                    }

                    using (System.IO.FileStream fs = new System.IO.FileStream(icoPath, System.IO.FileMode.Create))
                    {
                        Icon.FromHandle(bmpIco.GetHicon()).Save(fs);
                    }
                }

                // Función auxiliar para dibujar logos horizontales
                void DrawHorizontalLogo(string path, bool isLogin)
                {
                    using (Bitmap bmp = new Bitmap(240, 70, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            g.Clear(Color.Transparent);

                            Brush boxBrush = isLogin ? Brushes.White : new SolidBrush(PrimaryColor);
                            Brush textBrush = isLogin ? new SolidBrush(PrimaryColor) : Brushes.White;
                            
                            // Cuadro redondeado
                            using (var p = GetRoundedRect(new Rectangle(0, 10, 50, 50), 12))
                                g.FillPath(boxBrush, p);
                                
                            // Letra "M" centrada en el cuadro
                            using (Font fM = new Font("Segoe UI", 28, FontStyle.Bold))
                            {
                                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                                g.DrawString("M", fM, textBrush, new Rectangle(0, 7, 50, 50), sf);
                            }

                            // Carrito pequeño sobre el cuadro
                            using (Font emoji = new Font("Segoe UI Emoji", 12))
                            {
                                g.DrawString("🛒", emoji, textBrush, new Point(28, 40));
                            }
                                
                            // Texto "Momo's POS"
                            using (Font f1 = new Font("Segoe UI", 20, FontStyle.Regular))
                            using (Font f2 = new Font("Segoe UI", 20, FontStyle.Bold))
                            {
                                SizeF s1 = g.MeasureString("Momo's", f1, new PointF(0,0), StringFormat.GenericTypographic);
                                
                                g.DrawString("Momo's", f1, Brushes.White, 55, 15, StringFormat.GenericTypographic);
                                
                                Brush posBrush = isLogin ? new SolidBrush(Color.FromArgb(120, 210, 255)) : new SolidBrush(PrimaryColor);
                                g.DrawString("POS", f2, posBrush, 55 + s1.Width + 5, 15, StringFormat.GenericTypographic);
                            }
                        }
                        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                // 2. Generar logo para el Drawer
                DrawHorizontalLogo(drawerPngPath, false);
                
                // 3. Generar logo para el Login
                DrawHorizontalLogo(loginPngPath, true);


            }
            catch { }
        }
        
        // Estilización de botones
        public static void StyleButton(Button btn, Color backColor, Color? foreColor = null, Font font = null)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = foreColor ?? TextLight;
            btn.Font = font ?? FontNormal;
            btn.Cursor = Cursors.Hand;

            // Para bordes redondeados
            btn.Paint -= Btn_PaintRounded;
            btn.Paint += Btn_PaintRounded;
            
            // Forzar repintado si entra/sale el mouse (Hover)
            btn.MouseEnter -= Btn_MouseHoverRepaint;
            btn.MouseEnter += Btn_MouseHoverRepaint;
            btn.MouseLeave -= Btn_MouseHoverRepaint;
            btn.MouseLeave += Btn_MouseHoverRepaint;
        }

        private static void Btn_MouseHoverRepaint(object sender, System.EventArgs e)
        {
            if (sender is Button btn) btn.Invalidate();
        }

        private static void Btn_PaintRounded(object sender, PaintEventArgs e)
        {
            if (!(sender is Button btn)) return;
            
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Color clearColor = BackgroundColor;
            Control parent = btn.Parent;
            while (parent != null)
            {
                if (parent.BackColor != Color.Transparent)
                {
                    clearColor = parent.BackColor;
                    break;
                }
                parent = parent.Parent;
            }
            e.Graphics.Clear(clearColor);
            
            int radius = 8; // Radio de bordes
            using (var path = GetRoundedRect(new Rectangle(0, 0, btn.Width - 1, btn.Height - 1), radius))
            {
                Color bgColor = btn.BackColor;
                if (btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)))
                {
                    bgColor = ControlPaint.Light(bgColor, 0.1f);
                }
                
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                if (btn.BackColor == Color.White)
                {
                    using (Pen pen = new Pen(btn.ForeColor, 2f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // Estilización de DataGrid
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = BackgroundColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(230, 230, 230); // Línea muy sutil
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252); // Azul muy claro para selección
            dgv.DefaultCellStyle.SelectionForeColor = TextDark;
            dgv.DefaultCellStyle.Font = FontNormal;
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.Padding = new Padding(5);
            
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextDark;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontNormalBold;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgv.ColumnHeadersHeight = 45;
            
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 35;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Evento para formatear dinámicamente monedas y cantidades
            dgv.CellFormatting -= Dgv_CellFormatting; // Evitar suscripciones múltiples
            dgv.CellFormatting += Dgv_CellFormatting;
        }

        private static void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.ColumnIndex < 0) return;

            string colName = dgv.Columns[e.ColumnIndex].Name.ToLower();

            // Si es columna de cantidad numérica genérica (Prioridad sobre 'total' por CantidadTotal)
            if (colName.Contains("cantidad") || colName.Contains("stock"))
            {
                if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal val))
                {
                    e.Value = val.ToString("N2");
                    e.FormattingApplied = true;
                }
            }
            // Si es columna de moneda
            else if (colName.Contains("precio") || colName.Contains("total") || colName.Contains("importe") || 
                colName.Contains("pagado") || colName.Contains("cambio") || colName.Contains("saldo") || colName.Contains("limite") || colName.Contains("ganancia"))
            {
                if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal val))
                {
                    e.Value = val.ToString("C2");
                    e.FormattingApplied = true;
                }
            }
            // Si es columna de cantidad numérica genérica
            else if (colName.Contains("cantidad") || colName.Contains("stock"))
            {
                if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal val))
                {
                    e.Value = val.ToString("N2");
                    e.FormattingApplied = true;
                }
            }
        }
    }
}
