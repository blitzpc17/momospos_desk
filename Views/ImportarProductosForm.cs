using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                        
                        MessageBox.Show("Plantilla generada exitosamente. Llénela sin modificar los nombres de las columnas en la primera fila.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al generar plantilla:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSeleccionarArchivo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivos de Excel (*.xlsx)|*.xlsx";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        lblRuta.Text = Path.GetFileName(ofd.FileName);
                        _productosAImportar.Clear();

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
                            MessageBox.Show($"Se detectaron {_productosAImportar.Count} productos listos para importar.", "Análisis Completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            btnImportar.Enabled = false;
                            MessageBox.Show("No se detectaron productos válidos en el archivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        btnImportar.Enabled = false;
                        MessageBox.Show($"Error al leer el archivo Excel:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            if (_productosAImportar.Count == 0) return;

            var result = MessageBox.Show($"¿Está seguro de que desea importar {_productosAImportar.Count} productos? Si los códigos de barras ya existen, sus datos serán actualizados.", "Confirmar Importación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    btnImportar.Enabled = false;
                    btnSeleccionarArchivo.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;

                    _productoRepo.ImportarMasivo(_productosAImportar);
                    
                    MessageBox.Show("Importación completada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al importar a la base de datos:\n{ex.Message}", "Error de Importación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnImportar.Enabled = true;
                    btnSeleccionarArchivo.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }
    }
}
