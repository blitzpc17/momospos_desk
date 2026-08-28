using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using momospos.Views.Dialogs;

namespace momospos.Views
{
    public class VentasView : UserControl
    {
        private TextBox txtCodigoBarras;
        private Button btnAgregarAlCarrito;
        private DataGridView dgvCarrito;
        private Label lblTotal;
        private Button btnCobrar;
        private Button btnCancelar;
        private Button btnBuscarBuscador;
        private Button btnCortesiaManual;
        private PictureBox pbImagenProducto;

        private ProductoRepository _productoRepository;
        private VentaRepository _ventaRepository;
        private CajaRepository _cajaRepository;
        private PromocionRepository _promocionRepository;
        private ClienteRepository _clienteRepo;

        private List<VentaDetalle> _carrito;
        private Usuario _usuarioActual;
        private CajaSesion _sesionActual;
        
        public VentasView(Usuario usuario, CajaSesion sesion)
        {
            _usuarioActual = usuario;
            _sesionActual = sesion;
            
            _productoRepository = new ProductoRepository();
            _ventaRepository = new VentaRepository();
            _cajaRepository = new CajaRepository();
            _promocionRepository = new PromocionRepository();
            _clienteRepo = new ClienteRepository();
            _carrito = new List<VentaDetalle>();

            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // --- TOP BAR (Búsqueda y Acciones) ---
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            
            Label lblCodigo = new Label { Text = "Buscar:", Font = new Font("Segoe UI", 12), ForeColor = Color.DimGray, Location = new Point(20, 27), AutoSize = true };
            txtCodigoBarras = new TextBox { Location = new Point(90, 24), Width = 320, Font = new Font("Segoe UI", 15) };
            txtCodigoBarras.KeyDown += TxtCodigoBarras_KeyDown;

            btnAgregarAlCarrito = new Button { Text = "Agregar (Enter)", Location = new Point(420, 20), Width = 130, Height = 40 };
            Theme.StyleButton(btnAgregarAlCarrito, Theme.PrimaryColor);
            btnAgregarAlCarrito.Click += BtnAgregarAlCarrito_Click;

            btnBuscarBuscador = new Button { Text = "🔍 Buscar (F3)", Location = new Point(560, 20), Width = 130, Height = 40 };
            Theme.StyleButton(btnBuscarBuscador, Theme.SecondaryColor);
            btnBuscarBuscador.Click += BtnBuscarBuscador_Click;

            // Acciones secundarias (Botones tipo Outline pero con texto completo para claridad)
            Button btnRetiro = new Button { Text = "💸 Retiro (F4)", Location = new Point(700, 20), Width = 130, Height = 40 };
            Theme.StyleButton(btnRetiro, Color.White, Theme.DangerColor);
            btnRetiro.Click += (s, e) => AbrirRetiro();

            Button btnPausar = new Button { Text = "⏸️ Pausar (F6)", Location = new Point(840, 20), Width = 130, Height = 40 };
            Theme.StyleButton(btnPausar, Color.White, Theme.WarningColor);
            btnPausar.Click += BtnPausarVenta_Click;

            Button btnRecuperar = new Button { Text = "▶️ Recuperar (F7)", Location = new Point(980, 20), Width = 140, Height = 40 };
            Theme.StyleButton(btnRecuperar, Color.White, Color.Teal);
            btnRecuperar.Click += BtnRecuperarVenta_Click;
            
            // Sombra inferior
            Panel shadowTop = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(230, 230, 230) };

            topPanel.Controls.Add(lblCodigo);
            topPanel.Controls.Add(txtCodigoBarras);
            topPanel.Controls.Add(btnAgregarAlCarrito);
            topPanel.Controls.Add(btnBuscarBuscador);
            topPanel.Controls.Add(btnRetiro);
            topPanel.Controls.Add(btnPausar);
            topPanel.Controls.Add(btnRecuperar);
            topPanel.Controls.Add(shadowTop);

            // --- BOTTOM BAR (Totales y Cobro) ---
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.White };
            Panel bottomDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(230, 230, 230) };
            
            Panel rightButtonsPanel = new Panel { Dock = DockStyle.Right, Width = 550, BackColor = Color.Transparent };
            Panel leftTotalPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            lblTotal = new Label { 
                Text = "Total: $0.00", 
                Font = new Font("Segoe UI Light", 40), 
                ForeColor = Theme.PrimaryColor, 
                Location = new Point(20, 10),
                AutoSize = true 
            };
            leftTotalPanel.Controls.Add(lblTotal);
            
            btnCobrar = new Button { Text = "COBRAR (F12)", Width = 220, Height = 60, Location = new Point(310, 20) };
            Theme.StyleButton(btnCobrar, Theme.SuccessColor, Theme.TextLight, new Font("Segoe UI", 16, FontStyle.Bold));
            btnCobrar.Click += BtnCobrar_Click;

            btnCancelar = new Button { Text = "CANCELAR", Width = 130, Height = 60, Location = new Point(170, 20) };
            Theme.StyleButton(btnCancelar, Color.White, Theme.DangerColor, Theme.FontSubtitle);
            btnCancelar.Click += BtnCancelar_Click;

            btnCortesiaManual = new Button { Text = "🎁 Cortesía", Width = 130, Height = 60, Location = new Point(30, 20) };
            Theme.StyleButton(btnCortesiaManual, Color.White, Color.DarkMagenta, Theme.FontSubtitle);
            btnCortesiaManual.Click += BtnCortesiaManual_Click;

            rightButtonsPanel.Controls.Add(btnCobrar);
            rightButtonsPanel.Controls.Add(btnCancelar);
            rightButtonsPanel.Controls.Add(btnCortesiaManual);

            bottomPanel.Controls.Add(leftTotalPanel);
            bottomPanel.Controls.Add(rightButtonsPanel);
            bottomPanel.Controls.Add(bottomDivider);

            // --- RIGHT PANEL (Último Artículo) ---
            Panel rightPanel = new Panel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(25), BackColor = Color.White };
            Panel shadowRight = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Color.FromArgb(230, 230, 230) };

            PictureBox pbLogoEmpresa = new PictureBox {
                Dock = DockStyle.Top,
                Height = 100,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            
            // Cargar Logo de Empresa
            var confs = new ConfiguracionRepository().ObtenerTodas();
            if (confs.ContainsKey("RutaLogo") && !string.IsNullOrEmpty(confs["RutaLogo"]) && System.IO.File.Exists(confs["RutaLogo"]))
            {
                try {
                    using (var fs = new System.IO.FileStream(confs["RutaLogo"], System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        pbLogoEmpresa.Image = Image.FromStream(fs);
                    }
                } catch { }
            }

            Panel spacerTop = new Panel { Dock = DockStyle.Top, Height = 20 };

            Label lblProdTitle = new Label { 
                Text = "Último Artículo", 
                Font = new Font("Segoe UI Semibold", 12), 
                ForeColor = Color.DimGray, 
                Dock = DockStyle.Top, 
                TextAlign = ContentAlignment.MiddleCenter, 
                Height = 30 
            };

            Panel pbWrapper = new Panel { 
                Dock = DockStyle.Top, 
                Height = 250, 
                Padding = new Padding(0), 
                BackColor = Color.Transparent 
            }; 
            
            pbImagenProducto = new PictureBox {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            pbWrapper.Controls.Add(pbImagenProducto);

            rightPanel.Controls.Add(pbWrapper);
            rightPanel.Controls.Add(spacerTop);
            rightPanel.Controls.Add(lblProdTitle);
            rightPanel.Controls.Add(shadowRight);

            // --- DATA GRID ---
            dgvCarrito = new DataGridView();
            dgvCarrito.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvCarrito);
            
            dgvCarrito.ReadOnly = false;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvCarrito.CellValidating += DgvCarrito_CellValidating;
            dgvCarrito.CellEndEdit += DgvCarrito_CellEndEdit;
            dgvCarrito.CellContentClick += DgvCarrito_CellContentClick;
            dgvCarrito.CellClick += DgvCarrito_CellClick;
            dgvCarrito.CellBeginEdit += DgvCarrito_CellBeginEdit;
            dgvCarrito.DataBindingComplete += DgvCarrito_DataBindingComplete;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 0) };
            marginPanel.Controls.Add(dgvCarrito);

            this.Controls.Add(marginPanel);
            this.Controls.Add(rightPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void DgvCarrito_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn col in dgvCarrito.Columns)
            {
                if (col.Name == "Cantidad" || col.Name == "PrecioUnitario")
                {
                    col.ReadOnly = false;
                    col.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 220); // Resaltar celda editable
                }
                else if (col.Name == "Quitar")
                {
                    col.ReadOnly = false;
                }
                else
                {
                    col.ReadOnly = true;
                }
            }
        }

        private void DgvCarrito_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dgvCarrito.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                if (!decimal.TryParse(e.FormattedValue.ToString(), out decimal nuevaCantidad) || nuevaCantidad <= 0)
                {
                    dgvCarrito.Rows[e.RowIndex].ErrorText = "Cantidad debe ser un número mayor a cero.";
                    e.Cancel = true;
                    return;
                }

                var detalle = dgvCarrito.Rows[e.RowIndex].DataBoundItem as VentaDetalle;
                if (detalle != null)
                {
                    var p = _productoRepository.ObtenerPorId(detalle.ProductoId);
                    if (p != null && !p.PermiteFraccion && (nuevaCantidad % 1 != 0))
                    {
                        dgvCarrito.Rows[e.RowIndex].ErrorText = "Este producto no permite fracciones.";
                        e.Cancel = true;
                        return;
                    }
                }
                dgvCarrito.Rows[e.RowIndex].ErrorText = "";
            }
            else if (dgvCarrito.Columns[e.ColumnIndex].Name == "PrecioUnitario")
            {
                if (!decimal.TryParse(e.FormattedValue.ToString(), out decimal nuevoPrecio) || nuevoPrecio < 0)
                {
                    dgvCarrito.Rows[e.RowIndex].ErrorText = "El precio debe ser un número válido mayor o igual a cero.";
                    e.Cancel = true;
                    return;
                }
                dgvCarrito.Rows[e.RowIndex].ErrorText = "";
            }
        }

        private void DgvCarrito_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCarrito.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                var detalle = dgvCarrito.Rows[e.RowIndex].DataBoundItem as VentaDetalle;
                if (detalle != null)
                {
                    var p = _productoRepository.ObtenerPorId(detalle.ProductoId);
                    bool usarBascula = momospos.Helpers.ConfiguracionHelper.ObtenerUsarBascula();
                    string unidad = p?.UnidadMedidaAbreviatura?.ToUpper() ?? "";
                    bool esKilo = unidad.Contains("KG") || unidad.Contains("KIL");

                    if (usarBascula && p != null && p.PermiteFraccion && esKilo)
                    {
                        // No permitir edición manual si es pesable por báscula
                        e.Cancel = true;
                    }
                }
            }
        }

        private void DgvCarrito_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCarrito.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                var detalle = dgvCarrito.Rows[e.RowIndex].DataBoundItem as VentaDetalle;
                if (detalle != null)
                {
                    var p = _productoRepository.ObtenerPorId(detalle.ProductoId);
                    bool usarBascula = momospos.Helpers.ConfiguracionHelper.ObtenerUsarBascula();
                    string unidad = p?.UnidadMedidaAbreviatura?.ToUpper() ?? "";
                    bool esKilo = unidad.Contains("KG") || unidad.Contains("KIL");

                    if (usarBascula && p != null && p.PermiteFraccion && esKilo)
                    {
                        string puerto = momospos.Helpers.ConfiguracionHelper.ObtenerPuertoBascula();
                        try
                        {
                            decimal nuevoPeso = momospos.Helpers.BasculaHelper.LeerPeso(puerto);
                            detalle.Cantidad = nuevoPeso;
                            detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                            
                            var configRepo = new ConfiguracionRepository();
                            if (p.AplicaCaducidad && configRepo.ObtenerValor("GiroFarmaceutico") == "true")
                            {
                                detalle.LoteInfo = CalcularLotesAsignados(p.Id, detalle.Cantidad);
                            }

                            this.BeginInvoke(new Action(() => {
                                CalcularPromociones();
                                ActualizarCarritoUI();
                            }));
                        }
                        catch (Exception ex)
                        {
                            CustomDialog.ShowError($"Error al leer la báscula:\n{ex.Message}");
                        }
                    }
                }
            }
        }

        private void DgvCarrito_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvCarrito.Columns[e.ColumnIndex].Name;
            if (colName == "Cantidad" || colName == "PrecioUnitario")
            {
                var detalle = dgvCarrito.Rows[e.RowIndex].DataBoundItem as VentaDetalle;
                if (detalle != null)
                {
                    detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                    
                    var p = _productoRepository.ObtenerPorId(detalle.ProductoId);
                    var configRepo = new ConfiguracionRepository();
                    if (p != null && p.AplicaCaducidad && configRepo.ObtenerValor("GiroFarmaceutico") == "true")
                    {
                        detalle.LoteInfo = CalcularLotesAsignados(p.Id, detalle.Cantidad);
                    }
                    
                    // Usar BeginInvoke para evitar la excepción de llamada reentrante a SetCurrentCellAddressCore
                    this.BeginInvoke(new Action(() => {
                        CalcularPromociones();
                        ActualizarCarritoUI();
                    }));
                }
            }
        }

        private void DgvCarrito_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCarrito.Columns[e.ColumnIndex].Name == "Quitar")
            {
                var detalle = dgvCarrito.Rows[e.RowIndex].DataBoundItem as VentaDetalle;
                if (detalle != null)
                {
                    if (CustomDialog.ShowConfirm($"¿Desea quitar '{detalle.Descripcion}' del carrito?"))
                    {
                        var configRepo = new ConfiguracionRepository();
                        bool reqAuth = configRepo.ObtenerValor("RequerirAutorizacionCancelacion") == "true";
                        
                        if (reqAuth && !_usuarioActual.EsAdmin)
                        {
                            var authForm = new AutorizacionForm($"Eliminar partida: {detalle.Descripcion}");
                            if (authForm.ShowDialog() != DialogResult.OK)
                            {
                                return; // Se canceló la autorización
                            }
                        }

                        _carrito.Remove(detalle);
                        
                        // Usar BeginInvoke para evitar modificar el DataSource mientras procesa el click
                        this.BeginInvoke(new Action(() => {
                            CalcularPromociones();
                            ActualizarCarritoUI();
                        }));
                    }
                }
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (!_carrito.Any())
            {
                CustomDialog.ShowMessage("El carrito está vacío.");
                return;
            }
            
            var configRepo = new ConfiguracionRepository();
            bool reqAuth = configRepo.ObtenerValor("RequerirAutorizacionCancelacion") == "true";
            
            if (reqAuth && !_usuarioActual.EsAdmin)
            {
                var authForm = new AutorizacionForm("Cancelar Venta en Curso");
                if (authForm.ShowDialog() != DialogResult.OK)
                {
                    return; // Se canceló la autorización
                }
            }

            string motivo = CustomDialog.ShowInput("Ingrese el motivo de la cancelación de la venta en curso:", "Cancelar Venta");
            
            if (string.IsNullOrWhiteSpace(motivo))
            {
                CustomDialog.ShowWarning("Debe ingresar un motivo para poder cancelar la venta.");
                return;
            }

            decimal totalEsperado = _carrito.Sum(x => x.Subtotal);

            try
            {
                _ventaRepository.RegistrarVentaAbortada(DateTime.Now, _usuarioActual.Id, totalEsperado, motivo);
                _carrito.Clear();
                ActualizarCarritoUI();
                CustomDialog.ShowMessage("La venta ha sido cancelada y el carrito fue limpiado.", "Venta Cancelada");
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError($"Error al registrar cancelación:\n{ex.Message}");
            }
        }
        private void BtnCortesiaManual_Click(object sender, EventArgs e)
        {
            if (!_carrito.Any())
            {
                CustomDialog.ShowWarning("El carrito está vacío. Agregue productos para aplicar una cortesía.");
                return;
            }

            var configRepo = new ConfiguracionRepository();
            bool reqAuth = configRepo.ObtenerValor("RequerirAutorizacionCancelacion") == "true";
            
            if (reqAuth && !_usuarioActual.EsAdmin)
            {
                var authForm = new AutorizacionForm("Aplicar Cortesía Manual a Venta");
                if (authForm.ShowDialog() != DialogResult.OK)
                {
                    return; // Se canceló la autorización
                }
            }

            string input = CustomDialog.ShowInput("Ingrese el monto de descuento / cortesía a aplicar al total:", "Cortesía Manual", "0.00");
            if (decimal.TryParse(input, out decimal montoDescuento) && montoDescuento > 0)
            {
                decimal totalActual = _carrito.Sum(x => x.Subtotal);
                if (montoDescuento > totalActual)
                {
                    CustomDialog.ShowWarning("El descuento no puede ser mayor al total de la venta.");
                    return;
                }

                // Aplicar el descuento manual proporcionalmente o al primer item, o guardarlo.
                // Como el esquema lo permite, repartimos el DescuentoManual proporcionalmente al subtotal de cada item.
                foreach (var item in _carrito)
                {
                    decimal proporcion = item.Subtotal / totalActual;
                    decimal descAsignado = Math.Round(montoDescuento * proporcion, 2);
                    item.DescuentoManual = descAsignado;
                }

                // Ajuste por redondeos (aplicar diferencia al primer elemento)
                decimal diferencia = montoDescuento - _carrito.Sum(x => x.DescuentoManual);
                if (diferencia != 0 && _carrito.Count > 0)
                {
                    _carrito[0].DescuentoManual += diferencia;
                }

                ActualizarCarritoUI();
                CustomDialog.ShowMessage($"Se ha aplicado una cortesía de ${montoDescuento:N2}.", "Cortesía Aplicada");
            }
        }

        private void BtnBuscarBuscador_Click(object sender, EventArgs e)
        {
            var formBuscador = new BuscadorProductoForm();
            if (formBuscador.ShowDialog() == DialogResult.OK && formBuscador.ProductoSeleccionado != null)
            {
                AgregarProductoSeleccionado(formBuscador.ProductoSeleccionado);
            }
        }

        private void TxtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AgregarProductoAlCarrito(txtCodigoBarras.Text);
            }
        }

        private void BtnAgregarAlCarrito_Click(object sender, EventArgs e) => AgregarProductoAlCarrito(txtCodigoBarras.Text);

        private void AgregarProductoSeleccionado(Producto producto)
        {
            try
            {
                // Stock validation removed to allow selling products without stock

                decimal cantidadAComprar = 1;
                decimal? precioOverride = null;

                if (producto.PermiteFraccion)
                {
                    bool usarBascula = momospos.Helpers.ConfiguracionHelper.ObtenerUsarBascula();
                    bool pesoObtenido = false;

                    string unidad = producto.UnidadMedidaAbreviatura?.ToUpper() ?? "";
                    bool esKilo = unidad.Contains("KG") || unidad.Contains("KIL");

                    if (usarBascula && esKilo)
                    {
                        string puerto = momospos.Helpers.ConfiguracionHelper.ObtenerPuertoBascula();
                        
                        var formBascula = new momospos.Views.Dialogs.CapturaPesoForm(producto, puerto);
                        var dlgResult = formBascula.ShowDialog();
                        
                        if (dlgResult == DialogResult.OK)
                        {
                            cantidadAComprar = formBascula.PesoCapturado;
                            precioOverride = formBascula.PrecioFinal;
                            pesoObtenido = true;
                        }
                        else if (dlgResult == DialogResult.Yes || formBascula.UsarCapturaManual)
                        {
                            // Captura Manual
                            pesoObtenido = false;
                        }
                        else if (dlgResult == DialogResult.Retry || formBascula.IrAConfiguracion)
                        {
                            // Configurar
                            var formConfig = new Form 
                            { 
                                Text = "Configuración", 
                                Size = new System.Drawing.Size(900, 600), 
                                StartPosition = FormStartPosition.CenterParent 
                            };
                            formConfig.Controls.Add(new ConfiguracionView());
                            formConfig.ShowDialog();
                            
                            txtCodigoBarras.Focus();
                            return;
                        }
                        else 
                        {
                            // Cancel
                            txtCodigoBarras.Focus();
                            return;
                        }
                    }

                    if (!pesoObtenido)
                    {
                        string input = CustomDialog.ShowInput($"Ingrese la cantidad/peso de '{producto.Nombre}':", "Venta Fraccionada", "1.00");
                        if (!decimal.TryParse(input, out cantidadAComprar) || cantidadAComprar <= 0) return;
                    }
                }
                
                // Secondary stock validation removed to allow selling products without stock
                
                decimal precioFinal = precioOverride ?? producto.PrecioVenta;
                if (!precioOverride.HasValue && (!producto.PrecioFijo || producto.PrecioVenta == 0))
                {
                    string inputPrecio = CustomDialog.ShowInput($"Ingrese el precio de venta para '{producto.Nombre}':", "Precio Variable / Sin Precio", producto.PrecioVenta.ToString("0.00"));
                    if (!decimal.TryParse(inputPrecio, out precioFinal) || precioFinal < 0) return;
                }

                // Agrupamos por Id y Precio Unitario (por si el mismo producto se cobra a diferente precio)
                var existente = _carrito.FirstOrDefault(x => x.ProductoId == producto.Id && x.PrecioUnitario == precioFinal);
                
                var configRepo = new ConfiguracionRepository();
                bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";

                if (existente != null)
                {
                    existente.Cantidad += cantidadAComprar;
                    existente.Subtotal = existente.Cantidad * existente.PrecioUnitario;
                    if (producto.AplicaCaducidad && isFarmacia)
                    {
                        existente.LoteInfo = CalcularLotesAsignados(producto.Id, existente.Cantidad);
                    }
                }
                else
                {
                    string loteInfoStr = "";
                    if (producto.AplicaCaducidad && isFarmacia)
                    {
                        loteInfoStr = CalcularLotesAsignados(producto.Id, cantidadAComprar);
                    }

                    _carrito.Add(new VentaDetalle
                    {
                        ProductoId = producto.Id,
                        Descripcion = producto.Nombre,
                        Cantidad = cantidadAComprar,
                        PrecioUnitario = precioFinal,
                        Subtotal = cantidadAComprar * precioFinal,
                        LoteInfo = loteInfoStr
                    });
                }
                
                CalcularPromociones();
                ActualizarCarritoUI();
                txtCodigoBarras.Clear();
                
                // Mostrar imagen
                if (!string.IsNullOrEmpty(producto.RutaImagen) && System.IO.File.Exists(producto.RutaImagen))
                {
                    try
                    {
                        using (var fs = new System.IO.FileStream(producto.RutaImagen, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            pbImagenProducto.Image = Image.FromStream(fs);
                        }
                    }
                    catch { pbImagenProducto.Image = null; }
                }
                else
                {
                    pbImagenProducto.Image = null; // Quitar si no tiene
                }
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError($"Error:\n{ex.Message}");
            }
            txtCodigoBarras.Focus();
        }

        private void AgregarProductoAlCarrito(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;

            try
            {
                var producto = _productoRepository.ObtenerPorCodigo(codigo);
                if (producto != null)
                {
                    AgregarProductoSeleccionado(producto);
                }
                else
                {
                    CustomDialog.ShowWarning("Producto no encontrado.");
                    txtCodigoBarras.Focus();
                }
            }
            catch (Exception ex)
            {
                CustomDialog.ShowError($"Error de base de datos:\n{ex.Message}");
            }
        }

        private void ActualizarCarritoUI()
        {
            // Remover el source antes de reasignar
            dgvCarrito.DataSource = null;

            // Envolvemos el carrito en un BindingList para que soporte edición correctamente si es necesario.
            // Aunque List<T> funciona, BindingList suele notificar mejor.
            var bindingList = new System.ComponentModel.BindingList<VentaDetalle>(_carrito);
            dgvCarrito.DataSource = bindingList;
            
            if (dgvCarrito.Columns["Id"] != null) dgvCarrito.Columns["Id"].Visible = false;
            if (dgvCarrito.Columns["VentaId"] != null) dgvCarrito.Columns["VentaId"].Visible = false;
            if (dgvCarrito.Columns["ProductoId"] != null) dgvCarrito.Columns["ProductoId"].Visible = false;

            if (dgvCarrito.Columns["LoteInfo"] != null)
            {
                var configRepo = new ConfiguracionRepository();
                bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";
                dgvCarrito.Columns["LoteInfo"].HeaderText = "Lotes (Caducidad)";
                dgvCarrito.Columns["LoteInfo"].Visible = isFarmacia;
                dgvCarrito.Columns["LoteInfo"].ReadOnly = true;
                dgvCarrito.Columns["LoteInfo"].MinimumWidth = 150;
            }

            // Agregar la columna de eliminar si no existe
            if (dgvCarrito.Columns["Quitar"] == null)
            {
                DataGridViewButtonColumn btnQuitar = new DataGridViewButtonColumn();
                btnQuitar.Name = "Quitar";
                btnQuitar.HeaderText = "";
                btnQuitar.Text = "❌";
                btnQuitar.UseColumnTextForButtonValue = true;
                btnQuitar.Width = 50;
                btnQuitar.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                btnQuitar.FlatStyle = FlatStyle.Flat;
                dgvCarrito.Columns.Add(btnQuitar);
            }
            else
            {
                // Asegurarse de que esté al final
                dgvCarrito.Columns["Quitar"].DisplayIndex = dgvCarrito.Columns.Count - 1;
            }

            decimal total = _carrito.Sum(x => x.Subtotal);
            lblTotal.Text = $"Total: {total:C}";
        }

        private void CalcularPromociones()
        {
            var promocionesActivas = _promocionRepository.ObtenerTodas().Where(p => p.Activo && p.FechaInicio <= DateTime.Now && p.FechaFin >= DateTime.Now).ToList();

            foreach (var item in _carrito)
            {
                var p = _productoRepository.ObtenerPorId(item.ProductoId);
                
                // Inicializamos subtotal natural sin descuentos
                item.Subtotal = item.Cantidad * item.PrecioUnitario;
                item.DescuentoPromo = 0;
                item.NombrePromo = null;

                // 1. Verificar Precio Mayoreo (se convierte en Descuento Promo para que la matemática visual cuadre)
                decimal descuentoMayoreo = 0;
                if (p != null && p.CantidadMayoreo > 0 && item.Cantidad >= p.CantidadMayoreo)
                {
                    if (item.PrecioUnitario > p.PrecioMayoreo)
                    {
                        descuentoMayoreo = (item.PrecioUnitario - p.PrecioMayoreo) * item.Cantidad;
                        item.DescuentoPromo += descuentoMayoreo;
                        item.Subtotal -= descuentoMayoreo;
                        item.NombrePromo = "Mayoreo";
                    }
                }
                
                // 2. Otras Promociones (NxM, Porcentaje)
                var promo = promocionesActivas.FirstOrDefault(pr => pr.ProductoId == item.ProductoId);
                if (promo != null)
                {
                    decimal descuentoCalculado = 0;
                    
                    // Calculamos sobre el precio base original para no distorsionar la oferta
                    if (promo.Tipo == "NxM" && promo.CantidadRequerida > 0)
                    {
                        decimal gruposCompletos = Math.Floor(item.Cantidad / promo.CantidadRequerida);
                        decimal sobrantes = item.Cantidad % promo.CantidadRequerida;
                        decimal cantidadPagadaPorGrupo = promo.CantidadRequerida - promo.CantidadRegalo;
                        
                        decimal cantidadTotalCobrada = (gruposCompletos * cantidadPagadaPorGrupo) + sobrantes;
                        decimal subtotalPromo = cantidadTotalCobrada * item.PrecioUnitario;
                        
                        // Si la promo NxM resulta más barata que el mayoreo + precio normal, se aplica
                        decimal subtotalNatural = item.Cantidad * item.PrecioUnitario;
                        if (subtotalPromo < subtotalNatural)
                        {
                            descuentoCalculado = subtotalNatural - subtotalPromo;
                        }
                    }
                    else if (promo.Tipo == "Porcentaje" && promo.DescuentoPorcentaje > 0)
                    {
                        decimal factor = (100m - promo.DescuentoPorcentaje) / 100m;
                        decimal subtotalPromo = (item.Cantidad * item.PrecioUnitario) * factor;
                        decimal subtotalNatural = item.Cantidad * item.PrecioUnitario;
                        
                        if (subtotalPromo < subtotalNatural)
                        {
                            descuentoCalculado = subtotalNatural - subtotalPromo;
                        }
                    }
                    
                    // Solo aplicamos la promo si es MEJOR descuento que el mayoreo que ya traemos
                    if (descuentoCalculado > descuentoMayoreo)
                    {
                        // Deshacemos el mayoreo y aplicamos la promo
                        item.Subtotal += descuentoMayoreo; // restauramos subtotal
                        item.DescuentoPromo = descuentoCalculado;
                        item.Subtotal -= descuentoCalculado;
                        item.NombrePromo = promo.Nombre;
                    }
                }
                
                // Aplicar DescuentoManual al Subtotal si lo hubiera
                if (item.DescuentoManual > 0)
                {
                    item.Subtotal -= item.DescuentoManual;
                }
            }
        }

        private string CalcularLotesAsignados(int productoId, decimal cantidadSolicitada)
        {
            var lotes = _productoRepository.ObtenerLotesPorProducto(productoId)
                        .Where(l => l.StockActual > 0)
                        .ToList();

            if (!lotes.Any()) return "Sin stock asignado";

            decimal pendiente = cantidadSolicitada;
            List<string> asignaciones = new List<string>();

            foreach(var lote in lotes)
            {
                if (pendiente <= 0) break;
                decimal aTomar = Math.Min(pendiente, lote.StockActual);
                string caducidad = lote.FechaCaducidad.HasValue ? lote.FechaCaducidad.Value.ToString("MMM-yy") : "N/A";
                asignaciones.Add($"L-{lote.NumeroLote} [{aTomar:N0}] ({caducidad})");
                pendiente -= aTomar;
            }

            if (pendiente > 0)
            {
                asignaciones.Add($"Falta: {pendiente:N0}");
            }

            return string.Join(" | ", asignaciones);
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            // Validar que no haya celdas en estado de error
            dgvCarrito.EndEdit();

            if (!_carrito.Any())
            {
                CustomDialog.ShowWarning("El carrito está vacío.");
                return;
            }

            var configRepo = new ConfiguracionRepository();
            bool isFarmacia = configRepo.ObtenerValor("GiroFarmaceutico") == "true";
            
            string medicoNombre = null;
            string medicoCedula = null;
            bool recetaRetenida = false;
            string recetaRutaImagen = null;

            if (isFarmacia)
            {
                // Check if any product requires a prescription
                bool requiereReceta = false;
                foreach(var d in _carrito)
                {
                    var p = _productoRepository.ObtenerPorId(d.ProductoId);
                    if (p != null && p.RequiereReceta)
                    {
                        requiereReceta = true;
                        break;
                    }
                }

                if (requiereReceta)
                {
                    var recetaForm = new RecetaMedicaForm();
                    if (recetaForm.ShowDialog() != DialogResult.OK)
                    {
                        return; // Cancelaron la receta, se aborta el cobro
                    }
                    medicoNombre = recetaForm.NombreMedico;
                    medicoCedula = recetaForm.Cedula;
                    recetaRetenida = recetaForm.RecetaRetenida;
                    recetaRutaImagen = recetaForm.RecetaRutaImagen;
                }
            }

            decimal total = _carrito.Sum(x => x.Subtotal);
            
            var cobroForm = new CobroForm(total);
            if (cobroForm.ShowDialog() == DialogResult.OK)
            {
                decimal pagoEfectivo = cobroForm.PagoEfectivo;
                decimal pagoTarjeta = cobroForm.PagoTarjeta;
                decimal pagoCredito = cobroForm.PagoCredito;
                int? clienteId = cobroForm.ClienteIdSeleccionado;

                decimal pagadoTotal = pagoEfectivo + pagoTarjeta + pagoCredito;
                decimal cambio = cobroForm.Cambio;

                decimal efectivoRealIngresado = pagoEfectivo > total && pagoTarjeta == 0 ? total : pagoEfectivo - cambio;
                if (efectivoRealIngresado < 0) efectivoRealIngresado = 0;

                var venta = new Venta
                {
                    Folio = "V-" + DateTime.Now.ToString("yyMMddHHmmss"),
                    CajaSesionId = _sesionActual.Id,
                    Fecha = DateTime.Now,
                    Total = total,
                    Pagado = pagadoTotal,
                    Cambio = cambio,
                    Estado = "CONFIRMADO",
                    UsuarioId = _usuarioActual.Id,
                    ClienteId = clienteId,
                    MedicoNombre = medicoNombre,
                    MedicoCedula = medicoCedula,
                    RecetaRetenida = recetaRetenida,
                    RecetaRutaImagen = recetaRutaImagen,
                    Detalles = _carrito
                };

                if (pagoEfectivo > 0) venta.Pagos.Add(new VentaPago { MetodoPago = "EFECTIVO", Importe = efectivoRealIngresado, Fecha = DateTime.Now });
                if (pagoTarjeta > 0) venta.Pagos.Add(new VentaPago { MetodoPago = "TARJETA", Importe = pagoTarjeta, Fecha = DateTime.Now });
                if (pagoCredito > 0) venta.Pagos.Add(new VentaPago { MetodoPago = "CREDITO", Importe = pagoCredito, Fecha = DateTime.Now });

                try
                {
                    _ventaRepository.RegistrarVenta(venta);
                    
                    if (efectivoRealIngresado > 0)
                    {
                        _cajaRepository.ActualizarEfectivoEsperado(_sesionActual.Id, efectivoRealIngresado);
                        _cajaRepository.RegistrarMovimientoCaja(new CajaMovimiento
                        {
                            CajaSesionId = _sesionActual.Id,
                            Tipo = "VENTA",
                            Importe = efectivoRealIngresado,
                            Concepto = $"Venta {venta.Folio}",
                            UsuarioId = _usuarioActual.Id,
                            Fecha = DateTime.Now
                        });
                    }

                    CustomDialog.ShowMessage($"Venta registrada con éxito.\nCambio: {cambio:C}", "Venta Completada");
                    
                    TicketPrinter printer = new TicketPrinter(venta);
                    printer.AbrirCajon(); // Se abre si está configurado, haya papel o no.

                    if (CustomDialog.ShowConfirm("¿Desea imprimir el ticket de esta venta?", "Imprimir Ticket"))
                    {
                        printer.Imprimir();
                    }

                    _carrito.Clear();
                    ActualizarCarritoUI();
                }
                catch (Exception ex)
                {
                    CustomDialog.ShowError($"Error al registrar venta:\n{ex.Message}");
                }
            }
        }
        
        public void ProcessF12()
        {
            btnCobrar.PerformClick();
        }

        public void AbrirBuscador()
        {
            btnBuscarBuscador.PerformClick();
        }

        public void AbrirRetiro()
        {
            var form = new GastosForm(_sesionActual, _usuarioActual);
            form.ShowDialog();
            txtCodigoBarras.Focus();
        }

        private void BtnPausarVenta_Click(object sender, EventArgs e)
        {
            if (_carrito.Count == 0) return;

            string nombre = CustomDialog.ShowInput("Ingrese un nombre de referencia para esta venta en espera:", "Pausar Venta", "Turno Mostrador");
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string json = serializer.Serialize(_carrito);

                var repo = new OrdenesCobroRepository();
                repo.Insertar(new OrdenCobro { Referencia = nombre, ModuloOrigen = "MomosPOS", JsonDetalles = json });

                _carrito.Clear();
                ActualizarCarritoUI();
            }
            txtCodigoBarras.Focus();
        }

        private void BtnRecuperarVenta_Click(object sender, EventArgs e)
        {
            var form = new VentasEsperaForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                int id = form.OrdenSeleccionadaId;
                var repo = new OrdenesCobroRepository();
                var orden = repo.ObtenerPendientes().FirstOrDefault(o => o.Id == id);

                if (orden != null)
                {
                    if (_carrito.Count > 0)
                    {
                        var res = momospos.Views.CustomMessageBox.Show("Ya hay productos en el carrito. ¿Desea mezclarlos con la venta recuperada? Si elige NO, el carrito actual se borrará.", "Carrito ocupado", MessageBoxButtons.YesNoCancel);
                        if (res == DialogResult.Cancel) return;
                        if (res == DialogResult.No) _carrito.Clear();
                    }

                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    try 
                    {
                        var detalles = serializer.Deserialize<List<VentaDetalle>>(orden.JsonDetalles);
                        _carrito.AddRange(detalles);
                        repo.ActualizarEstado(id, "COBRADA"); // Se toma del pendiente
                    } 
                    catch(Exception ex)
                    {
                        momospos.Views.CustomMessageBox.Show("Error al recuperar orden: " + ex.Message);
                    }

                    ActualizarCarritoUI();
                }
            }
            txtCodigoBarras.Focus();
        }
    }
}
