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
            this.Padding = new Padding(20);

            Label lblTitle = new Label { Text = "Catálogo de Servicios Médicos", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblTitle);

            // Botón Nuevo
            Button btnNuevo = new Button { Text = "+ Nuevo Servicio", Location = new Point(20, 80), Width = 180, Height = 40 };
            Theme.StyleButton(btnNuevo, Theme.SuccessColor, Color.White, new Font("Segoe UI", 11, FontStyle.Bold));
            btnNuevo.Click += (s, e) => LimpiarFormulario();
            this.Controls.Add(btnNuevo);

            // Panel Principal Dividido (Grilla Izquierda, Formulario Derecha)
            SplitContainer split = new SplitContainer
            {
                Location = new Point(20, 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Width = this.Width - 40,
                Height = this.Height - 150,
                SplitterDistance = (int)((this.Width - 40) * 0.6)
            };
            this.Controls.Add(split);
            split.BringToFront();

            // Grilla
            _grid = new DataGridView();
            _grid.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(_grid);
            _grid.SelectionChanged += Grid_SelectionChanged;
            split.Panel1.Controls.Add(_grid);

            // Formulario
            _pnlForm = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(15) };
            split.Panel2.Controls.Add(_pnlForm);

            int y = 20;
            Label lblFormTitle = new Label { Text = "Detalles del Servicio", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Theme.PrimaryColor, AutoSize = true, Location = new Point(15, y) };
            _pnlForm.Controls.Add(lblFormTitle);

            y += 40;
            txtId = new TextBox { Visible = false };
            _pnlForm.Controls.Add(txtId);

            _pnlForm.Controls.Add(new Label { Text = "Nombre del Servicio:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(15, y) });
            y += 25;
            txtNombre = new TextBox { Font = Theme.FontNormal, Location = new Point(15, y), Width = 300 };
            _pnlForm.Controls.Add(txtNombre);

            y += 40;
            _pnlForm.Controls.Add(new Label { Text = "Descripción / Notas:", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(15, y) });
            y += 25;
            txtDescripcion = new TextBox { Font = Theme.FontNormal, Location = new Point(15, y), Width = 300, Multiline = true, Height = 80 };
            _pnlForm.Controls.Add(txtDescripcion);

            y += 95;
            _pnlForm.Controls.Add(new Label { Text = "Precio (Honorarios):", Font = Theme.FontNormalBold, AutoSize = true, Location = new Point(15, y) });
            y += 25;
            numPrecio = new NumericUpDown { Font = Theme.FontNormal, Location = new Point(15, y), Width = 150, DecimalPlaces = 2, Maximum = 999999 };
            _pnlForm.Controls.Add(numPrecio);

            y += 40;
            chkActivo = new CheckBox { Text = "Servicio Activo (Visible para recetar)", Font = Theme.FontNormal, Location = new Point(15, y), Width = 300, Checked = true };
            _pnlForm.Controls.Add(chkActivo);

            y += 50;
            Button btnGuardar = new Button { Text = "💾 Guardar Servicio", Location = new Point(15, y), Width = 200, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Color.White, new Font("Segoe UI", 11, FontStyle.Bold));
            btnGuardar.Click += BtnGuardar_Click;
            _pnlForm.Controls.Add(btnGuardar);
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
                MessageBox.Show("El nombre del servicio es obligatorio.");
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
                MessageBox.Show("Servicio guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}
