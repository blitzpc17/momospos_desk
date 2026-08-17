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
        }

        private void BuildUI()
        {
            this.Text = "Nuevo Producto";
            this.Size = new Size(500, 780); // Aumentar alto para nuevos campos
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
            txtPrecioCompra = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal };
            this.Controls.Add(txtPrecioCompra);
            startY += marginY;

            // Precio Venta
            this.Controls.Add(new Label { Text = "Precio Venta ($):", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtPrecioVenta = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal };
            this.Controls.Add(txtPrecioVenta);
            startY += marginY;

            // Stock Actual
            this.Controls.Add(new Label { Text = "Stock Actual:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtStockActual = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal };
            this.Controls.Add(txtStockActual);
            startY += marginY;

            // Stock Minimo
            this.Controls.Add(new Label { Text = "Stock Mínimo:", Font = Theme.FontNormal, Location = new Point(labelX, startY), AutoSize = true });
            txtStockMinimo = new TextBox { Location = new Point(inputX, startY), Width = 100, Font = Theme.FontNormal };
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

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigoBarras.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomDialog.ShowWarning("Código de Barras y Nombre son obligatorios.");
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

            ProductoRegistrado = new Producto
            {
                Id = _productoEditando != null ? _productoEditando.Id : 0,
                CodigoBarras = txtCodigoBarras.Text.Trim(),
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
