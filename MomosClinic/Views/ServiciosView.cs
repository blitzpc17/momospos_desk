using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Repositories;
using momospos.Views;

namespace MomosClinic.Views
{
    public class ServiciosView : UserControl
    {
        private DataGridView _grid;
        private ServiciosRepository _repo;
        private Panel _pnlForm;
        
        private TextBox txtId;
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private NumericUpDown numPrecio;
        private CheckBox chkActivo;

        public ServiciosView()
        {
            _repo = new ServiciosRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(20) };
            
            Label lblTitle = new Label { Text = "📋 Catálogo de Servicios Médicos", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 25) };
            topPanel.Controls.Add(lblTitle);

            Button btnNuevo = new Button { Text = "➕ Nuevo Servicio", Location = new Point(480, 25), Width = 180, Height = 40 };
            Theme.StyleButton(btnNuevo, Theme.SuccessColor, Color.White, Theme.FontNormalBold);
            btnNuevo.Click += (s, e) => LimpiarFormulario();
            topPanel.Controls.Add(btnNuevo);
            
            this.Controls.Add(topPanel);

            // Panel Principal Dividido (Grilla Izquierda, Formulario Derecha)
            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = (int)(this.Width * 0.6), // Will adjust on resize
                BackColor = Theme.BackgroundColor
            };
            
            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(split);
            this.Controls.Add(marginPanel);
            
            // Grilla
            _grid = new DataGridView();
            _grid.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(_grid);
            _grid.SelectionChanged += Grid_SelectionChanged;
            
            Panel gridMargin = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 10, 0) };
            gridMargin.Controls.Add(_grid);
            split.Panel1.Controls.Add(gridMargin);

            // Formulario Derecho (Card)
            _pnlForm = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(30) };
            
            TableLayoutPanel tlpForm = new TableLayoutPanel 
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 10,
                AutoScroll = true
            };
            _pnlForm.Controls.Add(tlpForm);

            Label lblFormTitle = new Label { Text = "📝 Detalles del Servicio", Font = Theme.FontTitle, ForeColor = Theme.PrimaryColor, AutoSize = true, Margin = new Padding(0, 0, 0, 25) };
            tlpForm.Controls.Add(lblFormTitle);

            txtId = new TextBox { Visible = false };
            tlpForm.Controls.Add(txtId);

            tlpForm.Controls.Add(new Label { Text = "Nombre del Servicio:", Font = Theme.FontNormalBold, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });
            txtNombre = new TextBox { Font = Theme.FontNormal, Width = 350, Margin = new Padding(0, 0, 0, 15) };
            tlpForm.Controls.Add(txtNombre);

            tlpForm.Controls.Add(new Label { Text = "Descripción / Notas:", Font = Theme.FontNormalBold, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });
            txtDescripcion = new TextBox { Font = Theme.FontNormal, Width = 350, Multiline = true, Height = 100, Margin = new Padding(0, 0, 0, 15) };
            tlpForm.Controls.Add(txtDescripcion);

            tlpForm.Controls.Add(new Label { Text = "Precio (Honorarios):", Font = Theme.FontNormalBold, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });
            numPrecio = new NumericUpDown { Font = Theme.FontNormal, Width = 150, DecimalPlaces = 2, Maximum = 999999, Margin = new Padding(0, 0, 0, 15) };
            tlpForm.Controls.Add(numPrecio);

            chkActivo = new CheckBox { Text = "Servicio Activo (Visible para recetar)", Font = Theme.FontNormal, AutoSize = true, Checked = true, Margin = new Padding(0, 15, 0, 25) };
            tlpForm.Controls.Add(chkActivo);

            Button btnGuardar = new Button { Text = "💾 Guardar Servicio", Width = 200, Height = 45, Margin = new Padding(0, 10, 0, 0) };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Color.White, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;
            tlpForm.Controls.Add(btnGuardar);

            split.Panel2.Controls.Add(_pnlForm);
            
            topPanel.BringToFront();
            marginPanel.BringToFront();
        }

        private void CargarDatos()
        {
            var datos = _repo.ObtenerTodos();
            _grid.DataSource = null; // Reset binding
            _grid.DataSource = datos;
            
            if (_grid.Columns.Contains("Id")) _grid.Columns["Id"].Visible = false;
            if (_grid.Columns.Contains("Descripcion")) _grid.Columns["Descripcion"].Visible = false;
            
            if (_grid.Columns.Contains("PrecioVenta"))
                _grid.Columns["PrecioVenta"].DefaultCellStyle.Format = "C2";
        }

        private void LimpiarFormulario()
        {
            txtId.Text = "0";
            txtNombre.Clear();
            txtDescripcion.Clear();
            numPrecio.Value = 0;
            chkActivo.Checked = true;
            _grid.ClearSelection();
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count > 0)
            {
                var row = _grid.SelectedRows[0];
                txtId.Text = row.Cells["Id"].Value.ToString();
                txtNombre.Text = row.Cells["Nombre"].Value.ToString();
                txtDescripcion.Text = row.Cells["Descripcion"].Value?.ToString() ?? "";
                numPrecio.Value = Convert.ToDecimal(row.Cells["PrecioVenta"].Value);
                chkActivo.Checked = Convert.ToBoolean(row.Cells["Activo"].Value);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                momospos.Views.CustomMessageBox.Show("El nombre del servicio es obligatorio.");
                return;
            }

            var srv = new ServicioMedico
            {
                Id = int.Parse(txtId.Text),
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                PrecioVenta = numPrecio.Value,
                Activo = chkActivo.Checked
            };

            try
            {
                _repo.Guardar(srv);
                momospos.Views.CustomMessageBox.Show("Servicio guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}
