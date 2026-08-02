using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;
using Microsoft.VisualBasic;

namespace momospos.Views
{
    public class ComprasView : UserControl
    {
        private TextBox txtCodigo;
        private Label lblNombreProducto;
        private Label lblStockActual;
        private TextBox txtCantidadEntrada;
        private TextBox txtCostoUnitario;
        private Button btnGuardar;
        private Button btnBuscar;

        private ProductoRepository _productoRepo;
        private Producto _productoSeleccionado;

        public ComprasView()
        {
            _productoRepo = new ProductoRepository();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "📦 Entrada de Inventario (Compras)", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            topPanel.Controls.Add(lblTitulo);

            Panel contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };

            int startY = 40;
            int marginY = 60;
            int labelX = 40;
            int inputX = 280;

            contentPanel.Controls.Add(new Label { Text = "Código de Barras:", Font = Theme.FontTitle, Location = new Point(labelX, startY), AutoSize = true });
            txtCodigo = new TextBox { Location = new Point(inputX, startY), Width = 300, Font = Theme.FontTitle };
            txtCodigo.KeyDown += TxtCodigo_KeyDown;
            
            btnBuscar = new Button { Text = "🔍 Buscar (F3)", Location = new Point(inputX + 320, startY - 2), Width = 150, Height = 35 };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor);
            btnBuscar.Click += BtnBuscar_Click;

            contentPanel.Controls.Add(txtCodigo);
            contentPanel.Controls.Add(btnBuscar);
            
            startY += marginY;
            
            lblNombreProducto = new Label { Text = "Producto: ---", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.PrimaryColor, Location = new Point(labelX, startY), AutoSize = true };
            contentPanel.Controls.Add(lblNombreProducto);
            
            startY += 40;
            lblStockActual = new Label { Text = "Stock Actual: 0", Font = Theme.FontNormal, ForeColor = Color.Gray, Location = new Point(labelX, startY), AutoSize = true };
            contentPanel.Controls.Add(lblStockActual);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "Cantidad a Ingresar:", Font = Theme.FontTitle, Location = new Point(labelX, startY), AutoSize = true });
            txtCantidadEntrada = new TextBox { Location = new Point(inputX, startY), Width = 150, Font = Theme.FontTitle, Enabled = false };
            txtCantidadEntrada.KeyPress += ValidarNumeros;
            contentPanel.Controls.Add(txtCantidadEntrada);

            startY += marginY;

            contentPanel.Controls.Add(new Label { Text = "Costo Unitario ($):", Font = Theme.FontTitle, Location = new Point(labelX, startY), AutoSize = true });
            txtCostoUnitario = new TextBox { Location = new Point(inputX, startY), Width = 150, Font = Theme.FontTitle, Enabled = false };
            txtCostoUnitario.KeyPress += ValidarNumeros;
            contentPanel.Controls.Add(txtCostoUnitario);

            startY += marginY + 20;

            btnGuardar = new Button { Text = "REGISTRAR ENTRADA", Location = new Point(inputX, startY), Width = 250, Height = 50, Enabled = false };
            Theme.StyleButton(btnGuardar, Theme.SuccessColor, Theme.TextLight, Theme.FontTitle);
            btnGuardar.Click += BtnGuardar_Click;
            contentPanel.Controls.Add(btnGuardar);

            this.Controls.Add(contentPanel);
            this.Controls.Add(topPanel);
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            var formBuscador = new BuscadorProductoForm();
            if (formBuscador.ShowDialog() == DialogResult.OK && formBuscador.ProductoSeleccionado != null)
            {
                SeleccionarProducto(formBuscador.ProductoSeleccionado);
            }
        }

        private void TxtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BuscarProducto(txtCodigo.Text);
            }
            if (e.KeyCode == Keys.F3)
            {
                btnBuscar.PerformClick();
            }
        }

        private void BuscarProducto(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;
            var prod = _productoRepo.ObtenerPorCodigo(codigo);
            if (prod != null)
            {
                SeleccionarProducto(prod);
            }
            else
            {
                MessageBox.Show("Producto no encontrado.");
            }
        }

        private void SeleccionarProducto(Producto prod)
        {
            _productoSeleccionado = prod;
            lblNombreProducto.Text = $"Producto: {prod.Nombre}";
            lblStockActual.Text = $"Stock Actual: {prod.StockActual}";
            txtCostoUnitario.Text = prod.PrecioCompra.ToString("0.##");
            
            txtCantidadEntrada.Enabled = true;
            txtCostoUnitario.Enabled = true;
            btnGuardar.Enabled = true;

            txtCodigo.Clear();
            txtCantidadEntrada.Focus();
        }

        private void ValidarNumeros(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_productoSeleccionado == null) return;

            decimal.TryParse(txtCantidadEntrada.Text, out decimal cantidad);
            decimal.TryParse(txtCostoUnitario.Text, out decimal costo);

            if (cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida mayor a cero.");
                return;
            }

            try
            {
                _productoRepo.AgregarStock(_productoSeleccionado.Id, cantidad, costo);
                MessageBox.Show("Stock actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Reiniciar campos
                _productoSeleccionado = null;
                lblNombreProducto.Text = "Producto: ---";
                lblStockActual.Text = "Stock Actual: 0";
                txtCantidadEntrada.Clear();
                txtCostoUnitario.Clear();
                
                txtCantidadEntrada.Enabled = false;
                txtCostoUnitario.Enabled = false;
                btnGuardar.Enabled = false;
                txtCodigo.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}
