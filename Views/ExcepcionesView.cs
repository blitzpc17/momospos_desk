using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using System.Linq;

namespace momospos.Views
{
    public class ExcepcionesView : UserControl
    {
        private DataGridView dgvExcepciones;
        private RichTextBox rtbStackTrace;
        private DateTimePicker dtpFecha;
        private ExcepcionRepository _repo;

        public ExcepcionesView()
        {
            _repo = new ExcepcionRepository();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // HEADER
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "⚠️ Bitácora de Excepciones", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            topPanel.Controls.Add(lblTitulo);

            Label lblFiltro = new Label { Text = "Fecha:", Font = Theme.FontNormal, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(350, 25) };
            topPanel.Controls.Add(lblFiltro);

            dtpFecha = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(410, 22), Width = 150, Font = Theme.FontNormal };
            dtpFecha.ValueChanged += DtpFecha_ValueChanged;
            topPanel.Controls.Add(dtpFecha);

            Button btnActualizar = new Button { Text = "🔄 Actualizar", Height = 40, Width = 120, Location = new Point(580, 15) };
            Theme.StyleButton(btnActualizar, Theme.PrimaryColor);
            btnActualizar.Click += DtpFecha_ValueChanged;
            topPanel.Controls.Add(btnActualizar);

            this.Controls.Add(topPanel);

            // GRID PANEL
            Panel gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 10) };
            dgvExcepciones = new DataGridView { Dock = DockStyle.Fill };
            Theme.StyleDataGridView(dgvExcepciones);
            dgvExcepciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvExcepciones.SelectionChanged += DgvExcepciones_SelectionChanged;
            gridPanel.Controls.Add(dgvExcepciones);

            this.Controls.Add(gridPanel);

            // DETALLE PANEL
            Panel detailPanel = new Panel { Dock = DockStyle.Bottom, Height = 250, Padding = new Padding(20, 10, 20, 20) };
            
            Label lblDetalle = new Label { Text = "Detalle Técnico (StackTrace):", Font = Theme.FontNormalBold, ForeColor = Theme.TextDark, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0,0,0,10) };
            detailPanel.Controls.Add(lblDetalle);

            rtbStackTrace = new RichTextBox 
            { 
                Dock = DockStyle.Fill, 
                Font = new Font("Consolas", 10), 
                BackColor = Color.WhiteSmoke, 
                ReadOnly = true 
            };
            detailPanel.Controls.Add(rtbStackTrace);

            this.Controls.Add(detailPanel);
            
            // Reordenar Z-Index para que Dock funcione bien
            gridPanel.BringToFront();   // Dock.Fill al frente (se dibuja al final en el centro)
            detailPanel.SendToBack();   // Dock.Bottom se envía atrás (se ancla abajo)
            topPanel.SendToBack();      // Dock.Top se envía atrás (se ancla arriba)

            CargarDatos();
        }

        private void DtpFecha_ValueChanged(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            var excepciones = _repo.ObtenerPorFecha(dtpFecha.Value);
            dgvExcepciones.DataSource = excepciones;

            if (dgvExcepciones.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in dgvExcepciones.Columns) col.Visible = false;

                dgvExcepciones.Columns["Id"].Visible = true;
                dgvExcepciones.Columns["Id"].Width = 60;

                dgvExcepciones.Columns["FechaHora"].Visible = true;
                dgvExcepciones.Columns["FechaHora"].HeaderText = "Hora";
                dgvExcepciones.Columns["FechaHora"].DefaultCellStyle.Format = "hh:mm tt";
                dgvExcepciones.Columns["FechaHora"].Width = 100;

                dgvExcepciones.Columns["UsuarioNombre"].Visible = true;
                dgvExcepciones.Columns["UsuarioNombre"].HeaderText = "Usuario";
                dgvExcepciones.Columns["UsuarioNombre"].Width = 150;

                dgvExcepciones.Columns["Modulo"].Visible = true;
                dgvExcepciones.Columns["Modulo"].HeaderText = "Módulo";
                dgvExcepciones.Columns["Modulo"].Width = 150;

                dgvExcepciones.Columns["Mensaje"].Visible = true;
                dgvExcepciones.Columns["Mensaje"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            
            rtbStackTrace.Text = "";
        }

        private void DgvExcepciones_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvExcepciones.SelectedRows.Count > 0)
            {
                var row = dgvExcepciones.SelectedRows[0];
                var stack = row.Cells["StackTrace"].Value?.ToString() ?? "No hay StackTrace disponible.";
                rtbStackTrace.Text = stack;
            }
            else
            {
                rtbStackTrace.Text = "";
            }
        }
    }
}
