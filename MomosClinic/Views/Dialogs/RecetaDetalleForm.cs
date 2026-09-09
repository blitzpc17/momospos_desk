using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class RecetaDetalleForm : Form
    {
        private Receta _receta;
        private Consulta _consulta;
        private Paciente _paciente;

        private DataGridView dgvDetalles;
        private TextBox txtIndicaciones;

        public RecetaDetalleForm(int recetaId, int consultaId, int pacienteId)
        {
            var recetaRepo = new RecetaRepository();
            var consultaRepo = new ConsultaRepository();
            var pacienteRepo = new PacienteRepository();

            _receta = recetaRepo.ObtenerCompleta(recetaId);
            _consulta = consultaRepo.ObtenerPorId(consultaId);
            _paciente = pacienteRepo.ObtenerPorId(pacienteId);

            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Text = "Detalle de Receta";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BackgroundColor;

            Label lblTitulo = new Label { Text = "📋 Detalle de la Receta", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 20), ForeColor = Theme.PrimaryColor };
            this.Controls.Add(lblTitulo);

            Label lblPaciente = new Label { Text = $"Paciente: {_paciente?.NombreCompleto ?? "N/A"}", Font = Theme.FontSubtitle, AutoSize = true, Location = new Point(20, 60), ForeColor = Theme.TextDark };
            Label lblFecha = new Label { Text = $"Fecha: {_receta?.FechaEmision.ToString("dd/MM/yyyy HH:mm")}", Font = Theme.FontSubtitle, AutoSize = true, Location = new Point(20, 85), ForeColor = Theme.TextDark };
            
            this.Controls.Add(lblPaciente);
            this.Controls.Add(lblFecha);

            dgvDetalles = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(740, 230),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToResizeColumns = true,
                ScrollBars = ScrollBars.Both
            };
            Theme.StyleDataGridView(dgvDetalles);
            this.Controls.Add(dgvDetalles);

            Label lblIndicaciones = new Label { Text = "Indicaciones Generales:", Font = Theme.FontSubtitle, AutoSize = true, Location = new Point(20, 370), ForeColor = Theme.TextDark };
            this.Controls.Add(lblIndicaciones);

            txtIndicaciones = new TextBox
            {
                Location = new Point(20, 400),
                Size = new Size(740, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = Theme.FontNormal
            };
            this.Controls.Add(txtIndicaciones);

            Button btnCerrar = new Button { Text = "Cerrar", Location = new Point(640, 515), Width = 120, Height = 35 };
            Theme.StyleButton(btnCerrar, Theme.SecondaryColor);
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);
        }

        private void CargarDatos()
        {
            if (_receta != null)
            {
                dgvDetalles.DataSource = _receta.Detalles;
                if (dgvDetalles.Columns.Contains("Id")) { dgvDetalles.Columns["Id"].Visible = false; dgvDetalles.Columns["Id"].Frozen = true; }
                if (dgvDetalles.Columns.Contains("RecetaId")) { dgvDetalles.Columns["RecetaId"].Visible = false; dgvDetalles.Columns["RecetaId"].Frozen = true; }
                if (dgvDetalles.Columns.Contains("ProductoId")) { dgvDetalles.Columns["ProductoId"].Visible = false; dgvDetalles.Columns["ProductoId"].Frozen = true; }

                if (dgvDetalles.Columns.Contains("NombreMedicamento"))
                {
                    dgvDetalles.Columns["NombreMedicamento"].Width = 250;
                    dgvDetalles.Columns["NombreMedicamento"].Frozen = true;
                }
                if (dgvDetalles.Columns.Contains("Dosis")) dgvDetalles.Columns["Dosis"].Width = 100;
                if (dgvDetalles.Columns.Contains("Frecuencia")) dgvDetalles.Columns["Frecuencia"].Width = 100;
                if (dgvDetalles.Columns.Contains("Duracion")) dgvDetalles.Columns["Duracion"].Width = 100;
                if (dgvDetalles.Columns.Contains("Cantidad")) dgvDetalles.Columns["Cantidad"].Width = 80;
                if (dgvDetalles.Columns.Contains("InstruccionesEspeciales")) dgvDetalles.Columns["InstruccionesEspeciales"].Width = 200;

                txtIndicaciones.Text = _receta.IndicacionesGenerales;
            }
        }
    }
}
