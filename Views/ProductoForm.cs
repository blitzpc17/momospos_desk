using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using System.Collections.Generic;

using momospos.Views.Dialogs;

namespace momospos.Views
{
    public class ProductoForm : Form
    {
        private TextBox txtCodigoBarras;
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private TextBox txtPrecioCompra;
        private TextBox txtPrecioVenta;
        private TextBox txtStockActual;
        private TextBox txtStockMinimo;
        private ComboBox cbCategoria;
        private ComboBox cbUnidadMedida;
        private CheckBox chkPrecioFijo;
        private CheckBox chkAplicaCaducidad;
        private CheckBox chkRequiereReceta;
        private TextBox txtSustanciaActiva;
        private TextBox txtClaveProducto;
        private TextBox txtCodigoProveedor;
        private TextBox txtPrecioMayoreo;
        private TextBox txtCantidadMayoreo;
        private PictureBox pbImagen;
        private Button btnSubirImagen;
        private string rutaImagenTemporal;
        
        private Button btnGuardar;
        private Button btnCancelar;

        private ProductoRepository _productoRepo;
        private CategoriaRepository _categoriaRepo;
        private UnidadMedidaRepository _unidadRepo;
        private ConfiguracionRepository _configRepo;

        public Producto ProductoRegistrado { get; private set; }

        private Producto _productoEditando;

        public ProductoForm(Producto producto = null)
        {
            _productoRepo = new ProductoRepository();
            _categoriaRepo = new CategoriaRepository();
            _unidadRepo = new UnidadMedidaRepository();
            _configRepo = new ConfiguracionRepository();
            _productoEditando = producto;

            BuildUI();
            Theme.SetIcon(this);
            CargarCombos();

            if (_productoEditando != null)
            {
                this.Text = "Editar Producto";
                CargarDatosEdicion();
            }
        }

        private void CargarDatosEdicion()
        {
            txtCodigoBarras.Text = _productoEditando.CodigoBarras;
            txtNombre.Text = _productoEditando.Nombre;
            txtDescripcion.Text = _productoEditando.Descripcion;
            txtPrecioCompra.Text = _productoEditando.PrecioCompra.ToString("N2");
            txtPrecioVenta.Text = _productoEditando.PrecioVenta.ToString("N2");
            txtStockActual.Text = _productoEditando.StockActual.ToString("N2");
            txtStockMinimo.Text = _productoEditando.StockMinimo.ToString("N2");

            if (_productoEditando.CategoriaId.HasValue)
                cbCategoria.SelectedValue = _productoEditando.CategoriaId.Value;
            
            if (_productoEditando.UnidadMedidaId.HasValue)
                cbUnidadMedida.SelectedValue = _productoEditando.UnidadMedidaId.Value;

            chkPrecioFijo.Checked = _productoEditando.PrecioFijo;
            
            if (chkAplicaCaducidad != null) chkAplicaCaducidad.Checked = _productoEditando.AplicaCaducidad;
            if (chkRequiereReceta != null) chkRequiereReceta.Checked = _productoEditando.RequiereReceta;
            if (txtSustanciaActiva != null) txtSustanciaActiva.Text = _productoEditando.SustanciaActiva;
            
            txtClaveProducto.Text = _productoEditando.ClaveProducto;
            txtCodigoProveedor.Text = _productoEditando.CodigoProveedor;
            txtPrecioMayoreo.Text = _productoEditando.PrecioMayoreo.ToString("N2");
            txtCantidadMayoreo.Text = _productoEditando.CantidadMayoreo.ToString("N2");
            rutaImagenTemporal = _productoEditando.RutaImagen;
            
            if (!string.IsNullOrEmpty(_productoEditando.RutaImagen) && System.IO.File.Exists(_productoEditando.RutaImagen))
            {
                try
                {
                    using (var fs = new System.IO.FileStream(_productoEditando.RutaImagen, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        pbImagen.Image = Image.FromStream(fs);
                    }
                }
                catch { }
            }
        }

        private void BuildUI()
        {
            this.Text = "Nuevo Producto";
            this.Size = new Size(800, 800); 
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Información del Producto", Font = Theme.FontTitle, ForeColor = Theme.TextLight, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);

            int startY = 80;
            int marginY = 40;
            int labelX = 30;
            int inputX = 180;
            int inputWidth = 270;
            
            int rightColX = 500;

            // Código de Barras
            this.Controls.Add(new Label { Text = "Código de Barras:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtCodigoBarras = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtCodigoBarras);
            startY += marginY;

            // Nombre
            this.Controls.Add(new Label { Text = "Nombre:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtNombre = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtNombre);
            startY += marginY;

            // Clave Producto
            this.Controls.Add(new Label { Text = "Clave Producto:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtClaveProducto = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtClaveProducto);
            startY += marginY;
            
            // Codigo Proveedor
            this.Controls.Add(new Label { Text = "Código Proveedor:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtCodigoProveedor = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtCodigoProveedor);
            startY += marginY;

            // Descripción
            this.Controls.Add(new Label { Text = "Descripción:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtDescripcion = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
            this.Controls.Add(txtDescripcion);
            startY += marginY;

            // Categoria
            this.Controls.Add(new Label { Text = "Categoría:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            cbCategoria = new ComboBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategoria.SelectedIndexChanged += CbCategoria_SelectedIndexChanged;
            this.Controls.Add(cbCategoria);
            startY += marginY;

            // Unidad Medida
            this.Controls.Add(new Label { Text = "Unidad de Medida:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            cbUnidadMedida = new ComboBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cbUnidadMedida);
            startY += marginY;



            // Precio Fijo
            chkPrecioFijo = new CheckBox { Text = "Precio Fijo", Font = Theme.FontNormal, Location = new Point(inputX, startY), Width = inputWidth + 20, Checked = true };
            this.Controls.Add(chkPrecioFijo);
            startY += marginY;

            // Precio Compra
            this.Controls.Add(new Label { Text = "Precio Compra ($):", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtPrecioCompra = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal, Text = "0" };
            this.Controls.Add(txtPrecioCompra);
            startY += marginY;

            // Precio Venta
            this.Controls.Add(new Label { Text = "Precio Venta ($):", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtPrecioVenta = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal, Text = "0" };
            this.Controls.Add(txtPrecioVenta);
            startY += marginY;

            // Precio Mayoreo
            this.Controls.Add(new Label { Text = "Precio Mayoreo ($):", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtPrecioMayoreo = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal };
            this.Controls.Add(txtPrecioMayoreo);
            
            this.Controls.Add(new Label { Text = "a partir de:", Font = Theme.FontNormal, Location = new Point(inputX + 110, startY + 3), AutoSize = true });
            txtCantidadMayoreo = new TextBox { Location = new Point(inputX + 200, startY), Width = 70, Font = Theme.FontNormal };
            this.Controls.Add(txtCantidadMayoreo);
            startY += marginY;

            // Stock Actual
            this.Controls.Add(new Label { Text = "Stock Actual:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtStockActual = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal, Text = "0" };
            this.Controls.Add(txtStockActual);
            startY += marginY;

            // Stock Minimo
            this.Controls.Add(new Label { Text = "Stock Mínimo:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtStockMinimo = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal, Text = "0" };
            this.Controls.Add(txtStockMinimo);
            startY += marginY;

            string valFarmacia = _configRepo.ObtenerValor("GiroFarmaceutico");
            bool isFarmacia = !string.IsNullOrEmpty(valFarmacia) && valFarmacia.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            
            if (isFarmacia)
            {
                chkAplicaCaducidad = new CheckBox { Text = "Aplica Caducidad (Manejar Lotes)", Font = Theme.FontNormal, Location = new Point(inputX, startY), Width = inputWidth + 50, Checked = false };
                this.Controls.Add(chkAplicaCaducidad);
                startY += marginY;

                chkRequiereReceta = new CheckBox { Text = "Requiere Receta Médica", Font = Theme.FontNormal, Location = new Point(inputX, startY), Width = inputWidth + 50, Checked = false };
                this.Controls.Add(chkRequiereReceta);
                startY += marginY;

                this.Controls.Add(new Label { Text = "Sustancia Activa (DCI):", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
                txtSustanciaActiva = new TextBox { Location = new Point(inputX, startY), Width = inputWidth, Font = Theme.FontNormal };
                this.Controls.Add(txtSustanciaActiva);
                startY += marginY;
            }

            // --- Right Column (Image) ---
            int imgStartY = 120;
            Label lblImagen = new Label { Text = "Imagen del Producto:", Font = Theme.FontSubtitle, Location = new Point(rightColX, imgStartY), AutoSize = true };
            this.Controls.Add(lblImagen);

            pbImagen = new PictureBox { 
                Location = new Point(rightColX, imgStartY + 30), 
                Size = new Size(250, 250), 
                SizeMode = PictureBoxSizeMode.Zoom, 
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(pbImagen);

            btnSubirImagen = new Button { Text = "Subir Imagen...", Location = new Point(rightColX + 50, imgStartY + 290), Width = 150, Height = 35 };
            Theme.StyleButton(btnSubirImagen, Color.Teal, Color.White, new Font("Segoe UI", 10));
            btnSubirImagen.Click += BtnSubirImagen_Click;
            this.Controls.Add(btnSubirImagen);

            // Botones
            btnGuardar = new Button { Text = "Guardar Producto", Location = new Point(inputX, startY + 10), Width = 160, Height = 40 };
            Theme.StyleButton(btnGuardar, Theme.SuccessColor);
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            btnCancelar = new Button { Text = "Cancelar", Location = new Point(inputX + 170, startY + 10), Width = 100, Height = 40 };
            Theme.StyleButton(btnCancelar, Color.Gray);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancelar);

            this.Controls.Add(topPanel);
        }

        private void CbCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isServicio = cbCategoria.Text.ToUpper().Contains("SERVICIO");
            txtStockActual.Enabled = !isServicio;
            txtStockMinimo.Enabled = !isServicio;
            
            if (chkPrecioFijo != null) chkPrecioFijo.Checked = !isServicio;
            
            if (isServicio)
            {
                txtStockActual.Text = "0";
                txtStockMinimo.Text = "0";

                if (cbUnidadMedida != null)
                {
                    foreach (UnidadMedida u in cbUnidadMedida.Items)
                    {
                        if (u.Nombre.Equals("SIN UNIDAD DE MEDIDA", StringComparison.OrdinalIgnoreCase))
                        {
                            cbUnidadMedida.SelectedItem = u;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (cbUnidadMedida != null)
                {
                    foreach (UnidadMedida u in cbUnidadMedida.Items)
                    {
                        if (u.Nombre.Equals("Pieza", StringComparison.OrdinalIgnoreCase))
                        {
                            cbUnidadMedida.SelectedItem = u;
                            break;
                        }
                    }
                }
            }
        }

        private void CargarCombos()
        {
            try
            {
                var categorias = _categoriaRepo.ObtenerTodas();
                cbCategoria.DataSource = categorias;
                cbCategoria.DisplayMember = "Nombre";
                cbCategoria.ValueMember = "Id";

                var unidades = _unidadRepo.ObtenerTodas();
                unidades.Insert(0, new UnidadMedida { Id = 0, Nombre = "SIN UNIDAD DE MEDIDA" });
                cbUnidadMedida.DataSource = unidades;
                cbUnidadMedida.DisplayMember = "Nombre";
                cbUnidadMedida.ValueMember = "Id";

                if (_productoEditando == null)
                {
                    // Seleccionar 'Pieza' por defecto para nuevos productos
                    foreach (UnidadMedida u in cbUnidadMedida.Items)
                    {
                        if (u.Nombre.Equals("Pieza", StringComparison.OrdinalIgnoreCase))
                        {
                            cbUnidadMedida.SelectedItem = u;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError("Error al cargar categorías o unidades:\n" + ex.Message);
            }
        }

        private void BtnSubirImagen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string baseDir = momospos.Helpers.ConfiguracionHelper.ObtenerRutaRecursos();
                        string targetDir = System.IO.Path.Combine(baseDir, "ImagenesProductos");
                        if (!System.IO.Directory.Exists(targetDir))
                            System.IO.Directory.CreateDirectory(targetDir);

                        string ext = System.IO.Path.GetExtension(ofd.FileName);
                        string fileName = $"prod_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 6)}{ext}";
                        string targetPath = System.IO.Path.Combine(targetDir, fileName);

                        // Resize image to max 400x400 to save space
                        using (Image original = Image.FromFile(ofd.FileName))
                        {
                            int newWidth = original.Width;
                            int newHeight = original.Height;
                            int max = 400;

                            if (original.Width > max || original.Height > max)
                            {
                                float ratioX = (float)max / original.Width;
                                float ratioY = (float)max / original.Height;
                                float ratio = Math.Min(ratioX, ratioY);

                                newWidth = (int)(original.Width * ratio);
                                newHeight = (int)(original.Height * ratio);
                            }

                            using (Bitmap resized = new Bitmap(original, new Size(newWidth, newHeight)))
                            {
                                resized.Save(targetPath, original.RawFormat);
                            }
                        }

                        rutaImagenTemporal = targetPath;
                        
                        using (var fs = new System.IO.FileStream(targetPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            pbImagen.Image = Image.FromStream(fs);
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomDialog.ShowError("No se pudo cargar la imagen: " + ex.Message);
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomDialog.ShowWarning("El Nombre es obligatorio.");
                return;
            }

            // Si los dejaron vacíos, rellenar con 0 por defecto para que no marque error
            if (string.IsNullOrWhiteSpace(txtPrecioCompra.Text)) txtPrecioCompra.Text = "0";
            if (string.IsNullOrWhiteSpace(txtPrecioVenta.Text)) txtPrecioVenta.Text = "0";
            if (string.IsNullOrWhiteSpace(txtStockActual.Text)) txtStockActual.Text = "0";
            if (string.IsNullOrWhiteSpace(txtStockMinimo.Text)) txtStockMinimo.Text = "0";

            if (!decimal.TryParse(txtPrecioCompra.Text, out decimal precioCompra) || 
                !decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) ||
                !decimal.TryParse(txtStockActual.Text, out decimal stockActual) ||
                !decimal.TryParse(txtStockMinimo.Text, out decimal stockMinimo))
            {
                CustomDialog.ShowWarning("Asegúrese de ingresar valores numéricos válidos en Precio y Stock.");
                return;
            }
            
            decimal precioMayoreo = 0;
            decimal cantidadMayoreo = 0;
            if (!string.IsNullOrWhiteSpace(txtPrecioMayoreo.Text)) decimal.TryParse(txtPrecioMayoreo.Text, out precioMayoreo);
            if (!string.IsNullOrWhiteSpace(txtCantidadMayoreo.Text)) decimal.TryParse(txtCantidadMayoreo.Text, out cantidadMayoreo);

            string codigoBarras = string.IsNullOrWhiteSpace(txtCodigoBarras.Text) ? null : txtCodigoBarras.Text.Trim();

            ProductoRegistrado = new Producto
            {
                Id = _productoEditando != null ? _productoEditando.Id : 0,
                CodigoBarras = codigoBarras,
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                PrecioCompra = precioCompra,
                PrecioVenta = precioVenta,
                StockActual = stockActual,
                StockMinimo = stockMinimo,
                EsServicio = cbCategoria.Text.ToUpper().Contains("SERVICIO"),
                PrecioFijo = chkPrecioFijo.Checked,
                AplicaCaducidad = chkAplicaCaducidad != null ? chkAplicaCaducidad.Checked : false,
                RequiereReceta = chkRequiereReceta != null ? chkRequiereReceta.Checked : false,
                SustanciaActiva = txtSustanciaActiva != null ? txtSustanciaActiva.Text.Trim() : "",
                PrecioMayoreo = precioMayoreo,
                CantidadMayoreo = cantidadMayoreo,
                ClaveProducto = txtClaveProducto.Text.Trim(),
                CodigoProveedor = txtCodigoProveedor.Text.Trim(),
                RutaImagen = rutaImagenTemporal,
                CategoriaId = cbCategoria.SelectedValue != null ? (int)cbCategoria.SelectedValue : (int?)null,
                UnidadMedidaId = cbUnidadMedida.SelectedValue != null ? (int)cbUnidadMedida.SelectedValue : (int?)null,
                CreadoEn = _productoEditando != null ? _productoEditando.CreadoEn : DateTime.Now
            };

            try
            {
                _productoRepo.Guardar(ProductoRegistrado);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError("Error al guardar en base de datos:\n" + ex.Message);
            }
        }
    }
}
