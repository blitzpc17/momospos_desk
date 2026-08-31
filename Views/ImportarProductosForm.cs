using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using momospos.Models;
using momospos.Repositories;

namespace momospos.Views
{
    public class ImportarProductosForm : Form
    {
        private Button btnDescargarPlantilla;
        private Button btnSeleccionarArchivo;
        private Button btnImportar;
        private DataGridView dgvVistaPrevia;
        private Label lblRuta;
        
        private ProductoRepository _productoRepo;
        private List<Producto> _productosAImportar;

        public ImportarProductosForm()
        {
            _productoRepo = new ProductoRepository();
            _productosAImportar = new List<Producto>();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Importación Masiva de Productos";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            try { this.Icon = new Icon(Path.Combine(Application.StartupPath, "Resources", "logo2.ico")); } catch { }

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "⬆️ Importación Masiva", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            
            btnDescargarPlantilla = new Button { Text = "1. Descargar Plantilla", Location = new Point(20, 70), Width = 180, Height = 40 };
            Theme.StyleButton(btnDescargarPlantilla, Theme.PrimaryColor);
            btnDescargarPlantilla.Click += BtnDescargarPlantilla_Click;

            btnSeleccionarArchivo = new Button { Text = "2. Seleccionar Excel", Location = new Point(220, 70), Width = 180, Height = 40 };
            Theme.StyleButton(btnSeleccionarArchivo, Color.FromArgb(243, 156, 18));
            btnSeleccionarArchivo.Click += BtnSeleccionarArchivo_Click;

            lblRuta = new Label { Text = "Ningún archivo seleccionado...", Font = Theme.FontNormal, ForeColor = Color.Gray, AutoSize = true, Location = new Point(420, 80) };

            btnImportar = new Button { Text = "3. Importar Productos", Location = new Point(680, 70), Width = 180, Height = 40, Enabled = false };
            Theme.StyleButton(btnImportar, Theme.SuccessColor);
            btnImportar.Click += BtnImportar_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(btnDescargarPlantilla);
            topPanel.Controls.Add(btnSeleccionarArchivo);
            topPanel.Controls.Add(lblRuta);
            topPanel.Controls.Add(btnImportar);

            dgvVistaPrevia = new DataGridView();
            dgvVistaPrevia.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvVistaPrevia);
            dgvVistaPrevia.ReadOnly = true;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvVistaPrevia);

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
        }

        private void BtnDescargarPlantilla_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivos de Excel (*.xlsx)|*.xlsx";
                sfd.FileName = "Plantilla_Productos.xlsx";
                
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Productos");
                            
                            // Headers
                            worksheet.Cell(1, 1).Value = "CodigoBarras";
                            worksheet.Cell(1, 2).Value = "Nombre";
                            worksheet.Cell(1, 3).Value = "Descripcion";
                            worksheet.Cell(1, 4).Value = "PrecioCompra";
                            worksheet.Cell(1, 5).Value = "PrecioVenta";
                            worksheet.Cell(1, 6).Value = "StockActual";

                            // Estilo de header
                            var headerRange = worksheet.Range("A1:F1");
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                            // Ejemplo
                            worksheet.Cell(2, 1).Value = "7501234567890";
                            worksheet.Cell(2, 2).Value = "Producto de Ejemplo";
                            worksheet.Cell(2, 3).Value = "Descripción breve del producto";
                            worksheet.Cell(2, 4).Value = 10.50;
                            worksheet.Cell(2, 5).Value = 15.00;
                            worksheet.Cell(2, 6).Value = 100;

                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                        }
                        
                        momospos.Views.CustomMessageBox.Show("Plantilla generada exitosamente. Llénela sin modificar los nombres de las columnas en la primera fila.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        momospos.Views.CustomMessageBox.Show($"Error al generar plantilla:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSeleccionarArchivo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivos de Excel (*.xlsx;*.xls)|*.xlsx;*.xls";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        lblRuta.Text = Path.GetFileName(ofd.FileName);
                        _productosAImportar.Clear();

                        if (Path.GetExtension(ofd.FileName).ToLower() == ".xls")
                        {
                            // Es un archivo TSV con extensión .xls
                            var lines = File.ReadAllLines(ofd.FileName, System.Text.Encoding.Default);
                            bool isFirstRow = true;
                            foreach (var line in lines)
                            {
                                if (string.IsNullOrWhiteSpace(line)) continue;
                                if (isFirstRow)
                                {
                                    isFirstRow = false;
                                    continue;
                                }

                                var cols = line.Split('\t');
                                if (cols.Length < 4) continue; // Mínimo código, nombre, costo, venta

                                Producto p = new Producto();
                                p.CodigoBarras = cols[0].Trim();
                                p.Nombre = cols[1].Trim();
                                p.Descripcion = cols[1].Trim();
                                
                                string pCompraStr = cols[2].Replace("$", "").Replace(",", "").Trim();
                                decimal.TryParse(pCompraStr, out decimal precioCompra);
                                p.PrecioCompra = precioCompra;
                                
                                string pVentaStr = cols[3].Replace("$", "").Replace(",", "").Trim();
                                decimal.TryParse(pVentaStr, out decimal precioVenta);
                                p.PrecioVenta = precioVenta;

                                if (cols.Length > 4)
                                {
                                    string pMayoreoStr = cols[4].Replace("$", "").Replace(",", "").Trim();
                                    decimal.TryParse(pMayoreoStr, out decimal precioMayoreo);
                                    p.PrecioMayoreo = precioMayoreo;
                                }

                                if (cols.Length > 5)
                                {
                                    decimal.TryParse(cols[5].Trim(), out decimal stockActual);
                                    p.StockActual = stockActual;
                                }

                                if (cols.Length > 6)
                                {
                                    decimal.TryParse(cols[6].Trim(), out decimal stockMinimo);
                                    p.StockMinimo = stockMinimo;
                                }

                                if (cols.Length > 7)
                                {
                                    p.CategoriaNombreTemporal = cols[7].Trim();
                                }

                                if (!string.IsNullOrWhiteSpace(p.Nombre) || !string.IsNullOrWhiteSpace(p.CodigoBarras))
                                {
                                    if(string.IsNullOrWhiteSpace(p.Nombre)) p.Nombre = "Sin nombre";
                                    _productosAImportar.Add(p);
                                }
                            }
                        }
                        else
                        {
                            using (var workbook = new XLWorkbook(ofd.FileName))
                            {
                                var worksheet = workbook.Worksheet(1);
                                var rows = worksheet.RangeUsed().RowsUsed();

                                bool isFirstRow = true;
                                foreach (var row in rows)
                                {
                                    if (isFirstRow)
                                    {
                                        isFirstRow = false;
                                        continue; // Saltamos encabezados
                                    }

                                    Producto p = new Producto();
                                    p.CodigoBarras = row.Cell(1).Value.ToString().Trim();
                                    p.Nombre = row.Cell(2).Value.ToString().Trim();
                                    p.Descripcion = row.Cell(3).Value.ToString().Trim();
                                    
                                    decimal precioCompra = 0;
                                    decimal.TryParse(row.Cell(4).Value.ToString(), out precioCompra);
                                    p.PrecioCompra = precioCompra;
                                    
                                    decimal precioVenta = 0;
                                    decimal.TryParse(row.Cell(5).Value.ToString(), out precioVenta);
                                    p.PrecioVenta = precioVenta;

                                    decimal stockActual = 0;
                                    decimal.TryParse(row.Cell(6).Value.ToString(), out stockActual);
                                    p.StockActual = stockActual;

                                    if (!string.IsNullOrWhiteSpace(p.Nombre))
                                    {
                                        _productosAImportar.Add(p);
                                    }
                                }
                            }
                        }

                        dgvVistaPrevia.DataSource = null;
                        dgvVistaPrevia.DataSource = _productosAImportar;

                        // Ocultar columnas innecesarias
                        foreach (DataGridViewColumn col in dgvVistaPrevia.Columns)
                        {
                            if (col.Name == "Id" || col.Name == "CategoriaId" || col.Name == "UnidadMedidaId" || col.Name == "StockMinimo" || col.Name == "EsServicio" || col.Name == "PrecioFijo" || col.Name == "UnidadMedidaNombre" || col.Name == "PermiteFraccion")
                            {
                                col.Visible = false;
                            }
                        }

                        if (_productosAImportar.Count > 0)
                        {
                            btnImportar.Enabled = true;
                            momospos.Views.CustomMessageBox.Show($"Se detectaron {_productosAImportar.Count} productos listos para importar.", "Análisis Completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            btnImportar.Enabled = false;
                            momospos.Views.CustomMessageBox.Show("No se detectaron productos válidos en el archivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        btnImportar.Enabled = false;
                        momospos.Views.CustomMessageBox.Show($"Error al leer el archivo Excel:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnImportar_Click(object sender, EventArgs e)
        {
            if (_productosAImportar.Count == 0) return;

            var result = momospos.Views.CustomMessageBox.Show($"¿Está seguro de que desea importar {_productosAImportar.Count} productos? Si los códigos de barras ya existen, sus datos serán actualizados.", "Confirmar Importación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                btnImportar.Enabled = false;
                btnSeleccionarArchivo.Enabled = false;
                this.Enabled = false;

                Form fLoading = new Form();
                fLoading.Size = new Size(450, 180);
                fLoading.StartPosition = FormStartPosition.CenterParent;
                fLoading.FormBorderStyle = FormBorderStyle.None;
                fLoading.BackColor = Theme.BackgroundColor;
                fLoading.Paint += (s, ev) => 
                {
                    ControlPaint.DrawBorder(ev.Graphics, fLoading.ClientRectangle, Theme.PrimaryColor, 2, ButtonBorderStyle.Solid, Theme.PrimaryColor, 2, ButtonBorderStyle.Solid, Theme.PrimaryColor, 2, ButtonBorderStyle.Solid, Theme.PrimaryColor, 2, ButtonBorderStyle.Solid);
                };

                Label lblTitle = new Label();
                lblTitle.Text = "Importando Productos...";
                lblTitle.Font = Theme.FontTitle;
                lblTitle.ForeColor = Theme.PrimaryColor;
                lblTitle.Dock = DockStyle.Top;
                lblTitle.Height = 60;
                lblTitle.TextAlign = ContentAlignment.MiddleCenter;
                
                Label lblStatus = new Label();
                lblStatus.Text = "Iniciando importación...";
                lblStatus.AutoSize = false;
                lblStatus.TextAlign = ContentAlignment.MiddleCenter;
                lblStatus.Dock = DockStyle.Top;
                lblStatus.Height = 40;
                lblStatus.Font = Theme.FontNormal;
                lblStatus.ForeColor = Theme.TextDark;
                
                Panel pbContainer = new Panel();
                pbContainer.Size = new Size(350, 25);
                pbContainer.Location = new Point((fLoading.Width - pbContainer.Width) / 2, 120);
                pbContainer.BackColor = Color.FromArgb(230, 230, 230);
                pbContainer.BorderStyle = BorderStyle.None;

                Panel pbFill = new Panel();
                pbFill.Size = new Size(0, 25);
                pbFill.Location = new Point(0, 0);
                pbFill.BackColor = Theme.PrimaryColor;
                pbContainer.Controls.Add(pbFill);
                
                fLoading.Controls.Add(pbContainer);
                fLoading.Controls.Add(lblStatus);
                fLoading.Controls.Add(lblTitle);
                
                fLoading.Show(this);

                var progress = new Progress<int>(count =>
                {
                    float percent = _productosAImportar.Count > 0 ? (float)count / _productosAImportar.Count : 0f;
                    pbFill.Width = (int)(pbContainer.Width * percent);
                    lblStatus.Text = $"Importando producto {count} de {_productosAImportar.Count}...";
                });

                List<string> errores = null;
                Exception fatalError = null;

                try
                {
                    errores = await Task.Run(() => _productoRepo.ImportarMasivo(_productosAImportar, progress));
                }
                catch (Exception ex)
                {
                    fatalError = ex;
                }
                finally
                {
                    fLoading.Close();
                    this.Enabled = true;
                }

                if (fatalError != null)
                {
                    momospos.Views.CustomMessageBox.Show($"Ocurrió un error general al importar a la base de datos:\n{fatalError.Message}", "Error de Importación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (errores != null && errores.Count > 0)
                {
                    Form fErrores = new Form();
                    fErrores.Text = "Reporte de Importación";
                    fErrores.Size = new Size(700, 450);
                    fErrores.StartPosition = FormStartPosition.CenterParent;
                    fErrores.Icon = this.Icon;
                    fErrores.BackColor = Theme.BackgroundColor;
                    
                    TextBox txt = new TextBox();
                    txt.Multiline = true;
                    txt.Dock = DockStyle.Fill;
                    txt.ScrollBars = ScrollBars.Both;
                    txt.ReadOnly = true;
                    txt.BackColor = Theme.BackgroundColor;
                    txt.ForeColor = Theme.TextDark;
                    txt.Font = new Font("Consolas", 10);
                    txt.Text = $"Se intentaron importar {_productosAImportar.Count} productos.\r\n" +
                               $"Éxitos: {_productosAImportar.Count - errores.Count}\r\n" +
                               $"Errores: {errores.Count}\r\n\r\n" +
                               $"Detalle de los errores encontrados:\r\n" +
                               new string('-', 50) + "\r\n" + 
                               string.Join("\r\n", errores);
                    
                    fErrores.Controls.Add(txt);
                    fErrores.ShowDialog();
                }
                else
                {
                    momospos.Views.CustomMessageBox.Show("Importación completada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
