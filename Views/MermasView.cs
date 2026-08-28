using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using System.Configuration;
using Dapper;
using Npgsql;
using System.Data;

namespace momospos.Views
{
    public class MermasView : UserControl
    {
        private TextBox txtCodigoBarras;
        private Button btnBuscar;
        private Label lblNombreProducto;
        private Label lblStockActual;
        private TextBox txtCantidadMerma;
        private TextBox txtMotivo;
        private Button btnRegistrar;

        private ProductoRepository _productoRepo;
        private Producto _productoSeleccionado;
        private Usuario _usuarioActual;

        public MermasView(Usuario usuarioActual)
        {
            _usuarioActual = usuarioActual;
            _productoRepo = new ProductoRepository();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "🗑️ Control de Mermas y Ajustes", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            topPanel.Controls.Add(lblTitulo);

            Panel contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };

            int startY = 40;
            int marginY = 50;

            contentPanel.Controls.Add(new Label { Text = "Código de Barras:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtCodigoBarras = new TextBox { Location = new Point(40, startY + 30), Width = 250, Font = new Font("Segoe UI", 16) };
            txtCodigoBarras.KeyDown += TxtCodigoBarras_KeyDown;
            contentPanel.Controls.Add(txtCodigoBarras);

            btnBuscar = new Button { Text = "🔍 Buscar", Location = new Point(310, startY + 28), Width = 120, Height = 36 };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor);
            btnBuscar.Click += BtnBuscar_Click;
            contentPanel.Controls.Add(btnBuscar);

            startY += marginY * 2;

            lblNombreProducto = new Label { Text = "Producto: -", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(40, startY), AutoSize = true };
            contentPanel.Controls.Add(lblNombreProducto);

            startY += marginY;

            lblStockActual = new Label { Text = "Stock Actual: -", Font = Theme.FontSubtitle, ForeColor = Color.Gray, Location = new Point(40, startY), AutoSize = true };
            contentPanel.Controls.Add(lblStockActual);

            startY += marginY + 20;

            contentPanel.Controls.Add(new Label { Text = "Cantidad a mermar/descontar:", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtCantidadMerma = new TextBox { Location = new Point(40, startY + 30), Width = 250, Font = new Font("Segoe UI", 16, FontStyle.Bold), TextAlign = HorizontalAlignment.Right, Enabled = false };
            txtCantidadMerma.KeyPress += ValidarNumeros;
            contentPanel.Controls.Add(txtCantidadMerma);

            startY += marginY * 2;

            contentPanel.Controls.Add(new Label { Text = "Motivo (Ej. Roto, Caducado):", Font = Theme.FontSubtitle, Location = new Point(40, startY), AutoSize = true });
            txtMotivo = new TextBox { Location = new Point(40, startY + 30), Width = 400, Font = new Font("Segoe UI", 14), Enabled = false };
            contentPanel.Controls.Add(txtMotivo);

            startY += marginY * 2;

            btnRegistrar = new Button { Text = "Registrar Baja", Location = new Point(40, startY), Width = 250, Height = 50, Enabled = false };
            Theme.StyleButton(btnRegistrar, Theme.DangerColor, Theme.TextLight, Theme.FontTitle);
            btnRegistrar.Click += BtnRegistrar_Click;
            contentPanel.Controls.Add(btnRegistrar);

            this.Controls.Add(contentPanel);
            this.Controls.Add(topPanel);
        }

        private void TxtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                BuscarProducto();
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            var formBuscador = new BuscadorProductoForm();
            if (formBuscador.ShowDialog() == DialogResult.OK && formBuscador.ProductoSeleccionado != null)
            {
                var prod = formBuscador.ProductoSeleccionado;
                _productoSeleccionado = prod;
                txtCodigoBarras.Text = prod.CodigoBarras;
                lblNombreProducto.Text = "Producto: " + prod.Nombre;
                lblStockActual.Text = "Stock Actual: " + prod.StockActual.ToString("N2");
                
                txtCantidadMerma.Enabled = true;
                txtMotivo.Enabled = true;
                btnRegistrar.Enabled = true;
                txtCantidadMerma.Focus();
            }
        }

        private void BuscarProducto()
        {
            string codigo = txtCodigoBarras.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;

            var prod = _productoRepo.ObtenerPorCodigo(codigo);
            if (prod != null)
            {
                _productoSeleccionado = prod;
                lblNombreProducto.Text = "Producto: " + prod.Nombre;
                lblStockActual.Text = "Stock Actual: " + prod.StockActual.ToString("N2");
                
                txtCantidadMerma.Enabled = true;
                txtMotivo.Enabled = true;
                btnRegistrar.Enabled = true;
                txtCantidadMerma.Focus();
            }
            else
            {
                momospos.Views.CustomMessageBox.Show("Producto no encontrado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Limpiar();
            }
        }

        private void ValidarNumeros(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true;
        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            if (_productoSeleccionado == null) return;

            if (!decimal.TryParse(txtCantidadMerma.Text, out decimal cantidad) || cantidad <= 0)
            {
                momospos.Views.CustomMessageBox.Show("Ingrese una cantidad válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cantidad > _productoSeleccionado.StockActual)
            {
                momospos.Views.CustomMessageBox.Show("No puede mermar más del stock actual disponible.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string motivo = txtMotivo.Text.Trim();
            if (string.IsNullOrEmpty(motivo))
            {
                momospos.Views.CustomMessageBox.Show("Ingrese un motivo para la merma.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (IDbConnection db = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    db.Open();
                    using (var tran = db.BeginTransaction())
                    {
                        // 1. Restar stock
                        db.Execute("UPDATE Productos SET StockActual = StockActual - @Cantidad WHERE Id = @Id", 
                                   new { Cantidad = cantidad, Id = _productoSeleccionado.Id }, tran);

                        // 2. Registrar movimiento
                        string sqlMov = @"INSERT INTO InventarioMovimientos (ProductoId, Tipo, Cantidad, Fecha, UsuarioId, Observaciones) 
                                          VALUES (@ProductoId, 'MERMA', @Cantidad, CURRENT_TIMESTAMP, @UsuarioId, @Observaciones)";
                        db.Execute(sqlMov, new { ProductoId = _productoSeleccionado.Id, Cantidad = cantidad, UsuarioId = _usuarioActual.Id, Observaciones = motivo }, tran);

                        tran.Commit();
                    }
                }

                momospos.Views.CustomMessageBox.Show("Merma registrada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Limpiar();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al registrar merma: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Limpiar()
        {
            _productoSeleccionado = null;
            txtCodigoBarras.Clear();
            lblNombreProducto.Text = "Producto: -";
            lblStockActual.Text = "Stock Actual: -";
            txtCantidadMerma.Clear();
            txtMotivo.Clear();
            txtCantidadMerma.Enabled = false;
            txtMotivo.Enabled = false;
            btnRegistrar.Enabled = false;
            txtCodigoBarras.Focus();
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtCodigoBarras.Focus();
        }
    }
}
