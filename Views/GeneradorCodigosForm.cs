using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using ZXing;
using momospos.Models;
using momospos.Repositories;
using System.Linq;
using System.ComponentModel;

namespace momospos.Views
{
    public class GeneradorCodigosForm : Form
    {
        private DataGridView dgvProductos;
        private TextBox txtPapelAncho;
        private TextBox txtPapelAlto;
        private TextBox txtEtiquetaAncho;
        private TextBox txtEtiquetaAlto;
        private Button btnBuscarProducto;
        private Button btnVistaPrevia;
        private Button btnImprimir;

        private BindingList<EtiquetaItem> listaEtiquetas = new BindingList<EtiquetaItem>();

        public class EtiquetaItem
        {
            public Producto Producto { get; set; }
            public string CodigoBarras { get { return Producto?.CodigoBarras; } }
            public string Nombre { get { return Producto?.Nombre; } }
            public int Cantidad { get; set; }
        }

        public GeneradorCodigosForm()
        {
            BuildUI();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "Generador de Códigos de Barras";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BackgroundColor;

            // Paneles Principales
            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 350, Padding = new Padding(20) };
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Panel Izquierdo: Configuraciones
            Label lblConfiguracion = new Label { Text = "Configuración", Font = Theme.FontSubtitle, Location = new Point(20, 20), AutoSize = true, ForeColor = Theme.TextDark };
            leftPanel.Controls.Add(lblConfiguracion);

            // Tamaño Papel
            GroupBox gbPapel = new GroupBox { Text = "Tamaño Papel (mm)", Location = new Point(20, 60), Width = 310, Height = 100, Font = Theme.FontNormal, ForeColor = Theme.TextDark };
            Label lblPapelAncho = new Label { Text = "Ancho:", Location = new Point(20, 30), AutoSize = true };
            txtPapelAncho = new TextBox { Location = new Point(90, 27), Width = 80, Text = "210" }; // A4 width
            Label lblPapelAlto = new Label { Text = "Alto:", Location = new Point(180, 30), AutoSize = true };
            txtPapelAlto = new TextBox { Location = new Point(220, 27), Width = 70, Text = "297" }; // A4 height
            
            gbPapel.Controls.Add(lblPapelAncho);
            gbPapel.Controls.Add(txtPapelAncho);
            gbPapel.Controls.Add(lblPapelAlto);
            gbPapel.Controls.Add(txtPapelAlto);
            leftPanel.Controls.Add(gbPapel);

            // Tamaño Etiqueta
            GroupBox gbEtiqueta = new GroupBox { Text = "Tamaño Etiqueta (mm)", Location = new Point(20, 170), Width = 310, Height = 100, Font = Theme.FontNormal, ForeColor = Theme.TextDark };
            Label lblEtiqAncho = new Label { Text = "Ancho:", Location = new Point(20, 30), AutoSize = true };
            txtEtiquetaAncho = new TextBox { Location = new Point(90, 27), Width = 80, Text = "50" };
            Label lblEtiqAlto = new Label { Text = "Alto:", Location = new Point(180, 30), AutoSize = true };
            txtEtiquetaAlto = new TextBox { Location = new Point(220, 27), Width = 70, Text = "30" };
            
            gbEtiqueta.Controls.Add(lblEtiqAncho);
            gbEtiqueta.Controls.Add(txtEtiquetaAncho);
            gbEtiqueta.Controls.Add(lblEtiqAlto);
            gbEtiqueta.Controls.Add(txtEtiquetaAlto);
            leftPanel.Controls.Add(gbEtiqueta);

            // Botones de acción
            btnVistaPrevia = new Button { Text = "👀 Vista Previa", Location = new Point(20, 290), Width = 310, Height = 45 };
            Theme.StyleButton(btnVistaPrevia, Theme.SecondaryColor);
            btnVistaPrevia.Click += BtnVistaPrevia_Click;

            btnImprimir = new Button { Text = "🖨️ Imprimir", Location = new Point(20, 345), Width = 310, Height = 45 };
            Theme.StyleButton(btnImprimir, Theme.PrimaryColor);
            btnImprimir.Click += BtnImprimir_Click;

            leftPanel.Controls.Add(btnVistaPrevia);
            leftPanel.Controls.Add(btnImprimir);

            // Panel Principal: Lista de Productos
            Label lblProductos = new Label { Text = "Productos Seleccionados", Font = Theme.FontSubtitle, Location = new Point(20, 20), AutoSize = true, ForeColor = Theme.TextDark };
            btnBuscarProducto = new Button { Text = "+ Añadir Producto", Location = new Point(450, 15), Width = 150, Height = 35 };
            Theme.StyleButton(btnBuscarProducto, Theme.PrimaryColor);
            btnBuscarProducto.Click += BtnBuscarProducto_Click;

            dgvProductos = new DataGridView
            {
                Location = new Point(20, 60),
                Width = 700,
                Height = 550,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false
            };
            Theme.StyleDataGridView(dgvProductos);
            dgvProductos.AllowUserToAddRows = false;
            
            var colCodigo = new DataGridViewTextBoxColumn { Name = "Codigo", HeaderText = "Código Barras", DataPropertyName = "CodigoBarras", ReadOnly = true, Width = 150 };
            var colNombre = new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Producto", DataPropertyName = "Nombre", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colCantidad = new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cantidad", DataPropertyName = "Cantidad", Width = 100 };
            var colEliminar = new DataGridViewButtonColumn { Name = "Eliminar", HeaderText = "", Text = "🗑", UseColumnTextForButtonValue = true, Width = 50 };

            dgvProductos.Columns.AddRange(colCodigo, colNombre, colCantidad, colEliminar);
            dgvProductos.CellContentClick += DgvProductos_CellContentClick;
            dgvProductos.DataSource = listaEtiquetas;

            mainPanel.Controls.Add(lblProductos);
            mainPanel.Controls.Add(btnBuscarProducto);
            mainPanel.Controls.Add(dgvProductos);

            this.Controls.Add(mainPanel);
            this.Controls.Add(leftPanel);
        }

        private void BtnBuscarProducto_Click(object sender, EventArgs e)
        {
            List<Producto> preseleccionados = listaEtiquetas.Select(x => x.Producto).ToList();
            var form = new BuscadorProductoForm(preseleccionados);
            if (form.ShowDialog() == DialogResult.OK && form.ProductosMultiSeleccionados != null)
            {
                var nuevosIds = form.ProductosMultiSeleccionados.Select(p => p.Id).ToList();

                // Eliminar los desmarcados
                for (int i = listaEtiquetas.Count - 1; i >= 0; i--)
                {
                    if (!nuevosIds.Contains(listaEtiquetas[i].Producto.Id))
                    {
                        listaEtiquetas.RemoveAt(i);
                    }
                }

                // Agregar los nuevos marcados
                foreach (var prod in form.ProductosMultiSeleccionados)
                {
                    if (!listaEtiquetas.Any(x => x.Producto.Id == prod.Id))
                    {
                        if (string.IsNullOrEmpty(prod.CodigoBarras))
                        {
                            MessageBox.Show($"El producto '{prod.Nombre}' no tiene un código de barras. No será agregado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                        listaEtiquetas.Add(new EtiquetaItem { Producto = prod, Cantidad = 1 });
                    }
                }
            }
        }

        private void DgvProductos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvProductos.Columns["Eliminar"].Index)
            {
                listaEtiquetas.RemoveAt(e.RowIndex);
                dgvProductos.Refresh();
            }
        }

        // --- Lógica de Impresión ---
        private int impresosActuales = 0;
        private List<EtiquetaItem> listaAImprimir = new List<EtiquetaItem>();

        private PrintDocument ConfigurarImpresion()
        {
            float papelAnchoMm = float.Parse(txtPapelAncho.Text);
            float papelAltoMm = float.Parse(txtPapelAlto.Text);

            PrintDocument pd = new PrintDocument();
            
            // Convertir mm a centésimas de pulgada (1 inch = 25.4 mm)
            int widthH = (int)((papelAnchoMm / 25.4f) * 100);
            int heightH = (int)((papelAltoMm / 25.4f) * 100);
            
            pd.DefaultPageSettings.PaperSize = new PaperSize("Custom", widthH, heightH);
            pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50); // 0.5 inch margins

            pd.PrintPage += Pd_PrintPage;
            pd.BeginPrint += Pd_BeginPrint;

            return pd;
        }

        private void Pd_BeginPrint(object sender, PrintEventArgs e)
        {
            impresosActuales = 0;
            listaAImprimir = new List<EtiquetaItem>();
            foreach (var item in listaEtiquetas)
            {
                for (int i = 0; i < item.Cantidad; i++)
                {
                    listaAImprimir.Add(item);
                }
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (listaAImprimir.Count == 0)
            {
                e.HasMorePages = false;
                return;
            }

            float etiAnchoMm = float.Parse(txtEtiquetaAncho.Text);
            float etiAltoMm = float.Parse(txtEtiquetaAlto.Text);

            // Convertir de mm a hundredths of inch para cálculos y gráficos
            float etiAnchoH = (etiAnchoMm / 25.4f) * 100;
            float etiAltoH = (etiAltoMm / 25.4f) * 100;

            float offsetX = e.MarginBounds.Left;
            float offsetY = e.MarginBounds.Top;
            
            float pageRight = e.MarginBounds.Right;
            float pageBottom = e.MarginBounds.Bottom;

            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = (int)etiAnchoH * 3, // Multiplicar para resolución de la imagen generada
                    Height = (int)(etiAltoH * 0.7f * 3), // Dejamos espacio para el texto abajo
                    Margin = 0,
                    PureBarcode = true // No imprimir texto, lo dibujaremos nosotros
                }
            };

            Pen borderPen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            Font textFont = new Font("Arial", 8);
            Brush textBrush = Brushes.Black;

            while (impresosActuales < listaAImprimir.Count)
            {
                if (offsetX + etiAnchoH > pageRight)
                {
                    offsetX = e.MarginBounds.Left;
                    offsetY += etiAltoH;
                }

                if (offsetY + etiAltoH > pageBottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                var item = listaAImprimir[impresosActuales];
                
                // Dibujar Borde
                RectangleF rectEtiq = new RectangleF(offsetX, offsetY, etiAnchoH, etiAltoH);
                e.Graphics.DrawRectangle(borderPen, rectEtiq.X, rectEtiq.Y, rectEtiq.Width, rectEtiq.Height);

                // Generar y Dibujar Código
                try
                {
                    Bitmap barcodeBitmap = writer.Write(item.CodigoBarras);
                    float pX = offsetX + 5; // padding
                    float pY = offsetY + 5;
                    float drawW = etiAnchoH - 10;
                    float drawH = etiAltoH - 25; // espacio para textos

                    e.Graphics.DrawImage(barcodeBitmap, pX, pY, drawW, drawH);
                    
                    // Dibujar textos (Nombre Producto y Codigo de Barras)
                    StringFormat format = new StringFormat { Alignment = StringAlignment.Center };
                    
                    // Nombre recortado
                    string nombreLimpio = item.Nombre.Length > 20 ? item.Nombre.Substring(0, 17) + "..." : item.Nombre;
                    e.Graphics.DrawString(nombreLimpio, textFont, textBrush, new RectangleF(offsetX, offsetY + etiAltoH - 25, etiAnchoH, 12), format);
                    e.Graphics.DrawString(item.CodigoBarras, textFont, textBrush, new RectangleF(offsetX, offsetY + etiAltoH - 12, etiAnchoH, 12), format);
                }
                catch (Exception)
                {
                    e.Graphics.DrawString("Error Código", textFont, Brushes.Red, offsetX + 5, offsetY + 5);
                }

                offsetX += etiAnchoH;
                impresosActuales++;
            }

            e.HasMorePages = false;
        }

        private void BtnVistaPrevia_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            PrintDocument pd = ConfigurarImpresion();
            PrintPreviewDialog ppd = new PrintPreviewDialog
            {
                Document = pd,
                Width = 800,
                Height = 600,
                Text = "Vista Previa de Códigos",
                ShowIcon = false
            };
            ppd.ShowDialog();
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            PrintDocument pd = ConfigurarImpresion();
            PrintDialog pdi = new PrintDialog { Document = pd };
            if (pdi.ShowDialog() == DialogResult.OK)
            {
                pd.Print();
            }
        }

        private bool Validar()
        {
            if (listaEtiquetas.Count == 0)
            {
                MessageBox.Show("Agregue al menos un producto a la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!float.TryParse(txtPapelAncho.Text, out _) || !float.TryParse(txtPapelAlto.Text, out _))
            {
                MessageBox.Show("Las dimensiones del papel deben ser numéricas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!float.TryParse(txtEtiquetaAncho.Text, out _) || !float.TryParse(txtEtiquetaAlto.Text, out _))
            {
                MessageBox.Show("Las dimensiones de la etiqueta deben ser numéricas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
}
