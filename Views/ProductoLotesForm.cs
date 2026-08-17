using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using momospos.Models;
using momospos.Repositories;
using momospos.Views.Dialogs;

namespace momospos.Views
{
    public class ProductoLotesForm : Form
    {
        private Producto _producto;
        private ProductoRepository _repo;
        private List<ProductoLote> _lotes;

        private DataGridView dgvLotes;
        private TextBox txtNumeroLote;
        private DateTimePicker dtpCaducidad;
        private TextBox txtStock;
        private Button btnGuardar;
        private Button btnEliminar;
        private ProductoLote _loteEditando = null;

        public ProductoLotesForm(Producto producto)
        {
            _producto = producto;
            _repo = new ProductoRepository();
            BuildUI();
            CargarLotes();
            Theme.SetIcon(this);
        }

        private void BuildUI()
        {
            this.Text = "Gestión de Lotes - " + _producto.Nombre;
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Lotes de: " + _producto.Nombre, Font = Theme.FontTitle, ForeColor = Theme.TextLight, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            Panel formPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
            
            formPanel.Controls.Add(new Label { Text = "Núm. Lote:", Location = new Point(20, 20), AutoSize = true, Font = Theme.FontNormal });
            txtNumeroLote = new TextBox { Location = new Point(100, 17), Width = 120, Font = Theme.FontNormal };
            formPanel.Controls.Add(txtNumeroLote);

            formPanel.Controls.Add(new Label { Text = "Caducidad:", Location = new Point(230, 20), AutoSize = true, Font = Theme.FontNormal });
            dtpCaducidad = new DateTimePicker { Location = new Point(310, 17), Width = 120, Font = Theme.FontNormal, Format = DateTimePickerFormat.Short };
            formPanel.Controls.Add(dtpCaducidad);

            formPanel.Controls.Add(new Label { Text = "Stock:", Location = new Point(440, 20), AutoSize = true, Font = Theme.FontNormal });
            txtStock = new TextBox { Location = new Point(490, 17), Width = 80, Font = Theme.FontNormal };
            formPanel.Controls.Add(txtStock);

            btnGuardar = new Button { Text = "Guardar", Location = new Point(100, 55), Width = 100, Height = 35 };
            Theme.StyleButton(btnGuardar, Theme.SuccessColor);
            btnGuardar.Click += BtnGuardar_Click;
            formPanel.Controls.Add(btnGuardar);

            Button btnLimpiar = new Button { Text = "Limpiar", Location = new Point(210, 55), Width = 100, Height = 35 };
            Theme.StyleButton(btnLimpiar, Color.Gray);
            btnLimpiar.Click += (s, e) => LimpiarFormulario();
            formPanel.Controls.Add(btnLimpiar);

            btnEliminar = new Button { Text = "Eliminar", Location = new Point(320, 55), Width = 100, Height = 35, Enabled = false };
            Theme.StyleButton(btnEliminar, Theme.DangerColor);
            btnEliminar.Click += BtnEliminar_Click;
            formPanel.Controls.Add(btnEliminar);

            this.Controls.Add(formPanel);

            dgvLotes = new DataGridView { Dock = DockStyle.Fill };
            Theme.StyleDataGridView(dgvLotes);
            dgvLotes.CellClick += DgvLotes_CellClick;
            this.Controls.Add(dgvLotes);
        }

        private void CargarLotes()
        {
            _lotes = _repo.ObtenerLotesPorProducto(_producto.Id);
            dgvLotes.DataSource = null;
            dgvLotes.DataSource = _lotes;

            if (dgvLotes.Columns["Id"] != null) dgvLotes.Columns["Id"].Visible = false;
            if (dgvLotes.Columns["ProductoId"] != null) dgvLotes.Columns["ProductoId"].Visible = false;
            if (dgvLotes.Columns["CreadoEn"] != null) dgvLotes.Columns["CreadoEn"].Visible = false;

            if (dgvLotes.Columns["NumeroLote"] != null) dgvLotes.Columns["NumeroLote"].HeaderText = "Núm. Lote";
            if (dgvLotes.Columns["FechaCaducidad"] != null) dgvLotes.Columns["FechaCaducidad"].HeaderText = "Caducidad";
            if (dgvLotes.Columns["StockActual"] != null) dgvLotes.Columns["StockActual"].HeaderText = "Stock";

            dgvLotes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNumeroLote.Text))
            {
                CustomDialog.ShowWarning("El número de lote es obligatorio.");
                return;
            }

            if (!decimal.TryParse(txtStock.Text, out decimal stock))
            {
                CustomDialog.ShowWarning("El stock debe ser un número válido.");
                return;
            }

            ProductoLote lote = _loteEditando ?? new ProductoLote();
            lote.ProductoId = _producto.Id;
            lote.NumeroLote = txtNumeroLote.Text.Trim();
            lote.FechaCaducidad = dtpCaducidad.Value.Date;
            lote.StockActual = stock;

            try
            {
                _repo.GuardarLote(lote);
                LimpiarFormulario();
                CargarLotes();
            }
            catch(Exception ex)
            {
                CustomDialog.ShowError("Error al guardar lote:\n" + ex.Message);
            }
        }

        private void DgvLotes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _loteEditando = dgvLotes.Rows[e.RowIndex].DataBoundItem as ProductoLote;
                if (_loteEditando != null)
                {
                    txtNumeroLote.Text = _loteEditando.NumeroLote;
                    if (_loteEditando.FechaCaducidad.HasValue) dtpCaducidad.Value = _loteEditando.FechaCaducidad.Value;
                    txtStock.Text = _loteEditando.StockActual.ToString();
                    btnEliminar.Enabled = true;
                }
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_loteEditando != null)
            {
                if (MessageBox.Show("¿Seguro que desea eliminar este lote?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _repo.EliminarLote(_loteEditando.Id, _producto.Id);
                    LimpiarFormulario();
                    CargarLotes();
                }
            }
        }

        private void LimpiarFormulario()
        {
            _loteEditando = null;
            txtNumeroLote.Clear();
            txtStock.Clear();
            dtpCaducidad.Value = DateTime.Now;
            btnEliminar.Enabled = false;
        }
    }
}
