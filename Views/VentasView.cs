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

        private ProductoRepository _productoRepository;
        private VentaRepository _ventaRepository;
        private CajaRepository _cajaRepository;

        private List<VentaDetalle> _carrito;
        private Usuario _usuarioActual;
        private CajaSesion _sesionActual;
        
        private Dictionary<string, List<VentaDetalle>> _ventasPausadas = new Dictionary<string, List<VentaDetalle>>();

        public VentasView(Usuario usuario, CajaSesion sesion)
        {
            _usuarioActual = usuario;
            _sesionActual = sesion;
            
            _productoRepository = new ProductoRepository();
            _ventaRepository = new VentaRepository();
            _cajaRepository = new CajaRepository();
            _carrito = new List<VentaDetalle>();

            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            
            Label lblCodigo = new Label { Text = "Código de Barras:", Font = Theme.FontTitle, Location = new Point(20, 25), AutoSize = true };
            txtCodigoBarras = new TextBox { Location = new Point(270, 20), Width = 250, Font = new Font("Segoe UI", 16) };
            txtCodigoBarras.KeyDown += TxtCodigoBarras_KeyDown;

            btnAgregarAlCarrito = new Button { Text = "Agregar (Enter)", Location = new Point(530, 20), Width = 125, Height = 40 };
            Theme.StyleButton(btnAgregarAlCarrito, Theme.PrimaryColor);
            btnAgregarAlCarrito.Click += BtnAgregarAlCarrito_Click;

            btnBuscarBuscador = new Button { Text = "🔍 Buscar (F3)", Location = new Point(665, 20), Width = 125, Height = 40 };
            Theme.StyleButton(btnBuscarBuscador, Theme.SecondaryColor);
            btnBuscarBuscador.Click += BtnBuscarBuscador_Click;

            Button btnRetiro = new Button { Text = "💸 Retiro (F4)", Location = new Point(800, 20), Width = 125, Height = 40 };
            Theme.StyleButton(btnRetiro, Theme.DangerColor);
            btnRetiro.Click += (s, e) => AbrirRetiro();

            Button btnPausar = new Button { Text = "⏸️ Pausar (F6)", Location = new Point(935, 20), Width = 125, Height = 40 };
            Theme.StyleButton(btnPausar, Color.DarkOrange);
            btnPausar.Click += (s, e) => PausarVenta();

            Button btnRecuperar = new Button { Text = "▶️ Recuper (F7)", Location = new Point(1070, 20), Width = 125, Height = 40 };
            Theme.StyleButton(btnRecuperar, Color.Teal);
            btnRecuperar.Click += (s, e) => RecuperarVenta();

            topPanel.Controls.Add(lblCodigo);
            topPanel.Controls.Add(txtCodigoBarras);
            topPanel.Controls.Add(btnAgregarAlCarrito);
            topPanel.Controls.Add(btnBuscarBuscador);
            topPanel.Controls.Add(btnRetiro);
            topPanel.Controls.Add(btnPausar);
            topPanel.Controls.Add(btnRecuperar);

            dgvCarrito = new DataGridView();
            dgvCarrito.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvCarrito);
            
            // Configurar DGV para permitir edición directa
            dgvCarrito.ReadOnly = false;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvCarrito.CellValidating += DgvCarrito_CellValidating;
            dgvCarrito.CellEndEdit += DgvCarrito_CellEndEdit;
            dgvCarrito.CellContentClick += DgvCarrito_CellContentClick;
            dgvCarrito.DataBindingComplete += DgvCarrito_DataBindingComplete;

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(20), WrapContents = true };
            lblTotal = new Label { Text = "Total: $0.00", Font = new Font("Segoe UI", 28, FontStyle.Bold), AutoSize = true, ForeColor = Theme.SuccessColor, Margin = new Padding(0, 0, 50, 0) };
            
            btnCobrar = new Button { Text = "COBRAR (F12)", Width = 200, Height = 60, Margin = new Padding(10, 0, 0, 0) };
            Theme.StyleButton(btnCobrar, Theme.SuccessColor, Theme.TextLight, Theme.FontTitle);
            btnCobrar.Click += BtnCobrar_Click;

            btnCancelar = new Button { Text = "🚫 CANCELAR", Width = 200, Height = 60, Margin = new Padding(10, 0, 0, 0) };
            Theme.StyleButton(btnCancelar, Theme.DangerColor, Theme.TextLight, Theme.FontTitle);
            btnCancelar.Click += BtnCancelar_Click;

            bottomPanel.Controls.Add(lblTotal);
            bottomPanel.Controls.Add(btnCobrar);
            bottomPanel.Controls.Add(btnCancelar);

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 0) };
            marginPanel.Controls.Add(dgvCarrito);

            this.Controls.Add(marginPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void DgvCarrito_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn col in dgvCarrito.Columns)
            {
                if (col.Name == "Cantidad")
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
        }

        private void DgvCarrito_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvCarrito.Columns[e.ColumnIndex].Name == "Cantidad")
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
                        _carrito.Remove(detalle);
                        
                        // Usar BeginInvoke para evitar modificar el DataSource mientras procesa el click
                        this.BeginInvoke(new Action(() => {
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
                if (!producto.EsServicio && producto.StockActual <= 0)
                {
                    CustomDialog.ShowWarning($"El producto '{producto.Nombre}' no tiene stock disponible.\nNo se puede agregar a la venta.", "Sin Stock");
                    txtCodigoBarras.Focus();
                    return;
                }

                decimal cantidadAComprar = 1;

                if (producto.PermiteFraccion)
                {
                    bool usarBascula = momospos.Helpers.ConfiguracionHelper.ObtenerUsarBascula();
                    bool pesoObtenido = false;

                    if (usarBascula && producto.UnidadMedidaAbreviatura?.ToUpper() == "KG")
                    {
                        string puerto = momospos.Helpers.ConfiguracionHelper.ObtenerPuertoBascula();
                        try
                        {
                            cantidadAComprar = momospos.Helpers.BasculaHelper.LeerPeso(puerto);
                            pesoObtenido = true;
                        }
                        catch (Exception ex)
                        {
                            if (CustomDialog.ShowConfirm($"Error de báscula:\n{ex.Message}\n\n¿Desea capturar manualmente el peso?\n[SÍ] = Capturar a mano\n[NO] = Configurar báscula", "Báscula no detectada"))
                            {
                                pesoObtenido = false;
                            }
                            else
                            {
                                // El usuario seleccionó NO, abrir la configuración de la báscula
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
                        }
                    }

                    if (!pesoObtenido)
                    {
                        string input = CustomDialog.ShowInput($"Ingrese la cantidad/peso de '{producto.Nombre}':", "Venta Fraccionada", "1.00");
                        if (!decimal.TryParse(input, out cantidadAComprar) || cantidadAComprar <= 0) return;
                    }
                }
                
                if (!producto.EsServicio && cantidadAComprar > producto.StockActual)
                {
                    CustomDialog.ShowWarning($"Solo cuentas con {producto.StockActual:N2} de stock para '{producto.Nombre}'.", "Stock Insuficiente");
                    txtCodigoBarras.Focus();
                    return;
                }
                
                decimal precioFinal = producto.PrecioVenta;
                if (!producto.PrecioFijo || producto.PrecioVenta == 0)
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
                ActualizarCarritoUI();
                txtCodigoBarras.Clear();
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

        public void PausarVenta()
        {
            if (_carrito.Count == 0)
            {
                CustomDialog.ShowWarning("El carrito está vacío.");
                return;
            }

            string nombre = CustomDialog.ShowInput("Ingrese un nombre de referencia para esta venta en espera:", "Pausar Venta", "Cliente " + (_ventasPausadas.Count + 1));
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                if (_ventasPausadas.ContainsKey(nombre))
                {
                    CustomDialog.ShowWarning("Ya existe una venta en espera con ese nombre. Use otro.");
                    return;
                }

                _ventasPausadas.Add(nombre, new List<VentaDetalle>(_carrito));
                _carrito.Clear();
                ActualizarCarritoUI();
                CustomDialog.ShowMessage("Venta pausada con éxito.");
            }
            txtCodigoBarras.Focus();
        }

        public void RecuperarVenta()
        {
            if (_ventasPausadas.Count == 0)
            {
                CustomDialog.ShowMessage("No hay ventas en espera.");
                return;
            }

            if (_carrito.Count > 0)
            {
                CustomDialog.ShowWarning("Primero debe cobrar o pausar la venta actual antes de recuperar otra.", "Carrito Ocupado");
                return;
            }

            var form = new VentasEsperaForm(_ventasPausadas);
            if (form.ShowDialog() == DialogResult.OK)
            {
                string key = form.VentaSeleccionadaId;
                if (_ventasPausadas.ContainsKey(key))
                {
                    _carrito = _ventasPausadas[key];
                    _ventasPausadas.Remove(key);
                    ActualizarCarritoUI();
                }
            }
            txtCodigoBarras.Focus();
        }
    }
}
