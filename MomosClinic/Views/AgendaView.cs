using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views
{
    public class AgendaView : UserControl
    {
        private DateTimePicker dtpFechaFiltro;
        private DataGridView dgvCitas;
        private Button btnNuevaCita;
        private Button btnAtender;
        private Button btnCompletar;
        
        private CitaRepository _citaRepo;
        
        public AgendaView()
        {
            _citaRepo = new CitaRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "📅 Agenda de Citas", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            dtpFechaFiltro = new DateTimePicker { Location = new Point(350, 27), Width = 250, Font = Theme.FontNormal, Format = DateTimePickerFormat.Long };
            dtpFechaFiltro.ValueChanged += (s, e) => CargarDatos();

            btnNuevaCita = new Button { Text = "➕ Nueva Cita", Location = new Point(620, 25), Width = 150, Height = 35 };
            Theme.StyleButton(btnNuevaCita, Theme.PrimaryColor);
            btnNuevaCita.Click += BtnNuevaCita_Click;

            btnAtender = new Button { Text = "🩺 Iniciar Consulta", Location = new Point(780, 25), Width = 180, Height = 35 };
            Theme.StyleButton(btnAtender, Theme.SuccessColor);
            btnAtender.Click += BtnAtender_Click;
            
            btnCompletar = new Button { Text = "✅ Completar", Location = new Point(970, 25), Width = 130, Height = 35 };
            Theme.StyleButton(btnCompletar, Theme.SecondaryColor);
            btnCompletar.Click += BtnCompletar_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(dtpFechaFiltro);
            topPanel.Controls.Add(btnNuevaCita);
            topPanel.Controls.Add(btnAtender);
            topPanel.Controls.Add(btnCompletar);

            dgvCitas = new DataGridView();
            dgvCitas.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvCitas);
            dgvCitas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCitas.MultiSelect = false;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvCitas);

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDatos()
        {
            var citas = _citaRepo.ObtenerCitasDelDia(dtpFechaFiltro.Value.Date).ToList();
            dgvCitas.DataSource = citas;
            
            if (dgvCitas.Columns.Count > 0)
            {
                dgvCitas.Columns["Id"].Width = 50;
                dgvCitas.Columns["PacienteId"].Visible = false;
                dgvCitas.Columns["NombrePaciente"].HeaderText = "Paciente";
                dgvCitas.Columns["NombrePaciente"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvCitas.Columns["FechaHora"].HeaderText = "Hora";
                dgvCitas.Columns["FechaHora"].DefaultCellStyle.Format = "hh:mm tt";
                dgvCitas.Columns["FechaHora"].Width = 100;
                dgvCitas.Columns["CreadoEn"].Visible = false;
            }
        }

        private void BtnNuevaCita_Click(object sender, EventArgs e)
        {
            var form = new MomosClinic.Views.Dialogs.CitaForm(dtpFechaFiltro.Value.Date);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _citaRepo.Insertar(form.CitaConfigurada);
                CargarDatos();
            }
        }

        private void BtnAtender_Click(object sender, EventArgs e)
        {
            if (dgvCitas.SelectedRows.Count == 0) return;
            var id = (int)dgvCitas.SelectedRows[0].Cells["Id"].Value;
            var pacienteId = (int)dgvCitas.SelectedRows[0].Cells["PacienteId"].Value;
            var estado = dgvCitas.SelectedRows[0].Cells["Estado"].Value.ToString();
            
            if (estado == "Completada" || estado == "Cancelada")
            {
                MessageBox.Show("Esta cita ya no puede ser atendida.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _citaRepo.ActualizarEstado(id, "En Curso");
            
            var form = new MomosClinic.Views.Dialogs.ConsultaForm(pacienteId, id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                var consultaRepo = new ConsultaRepository();
                int consultaId = consultaRepo.Insertar(form.ConsultaActual);
                
                if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales))
                {
                    form.RecetaActual.ConsultaId = consultaId;
                    form.RecetaActual.PacienteId = pacienteId;
                    var recetaRepo = new RecetaRepository();
                    recetaRepo.Insertar(form.RecetaActual);

                    if (MessageBox.Show("¿Desea imprimir la receta médica?", "Imprimir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        var pacienteRepo = new PacienteRepository();
                        var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                        
                        // Enviar a caja si hay medicamentos de farmacia
                        Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual);

                        var printer = new MomosClinic.Services.RecetaPrinter(paciente, form.ConsultaActual, form.RecetaActual);
                        printer.Imprimir();
                    }
                    else 
                    {
                        // Si no imprime, igual enviarlo a caja si hay medicamentos de farmacia
                        var pacienteRepo = new PacienteRepository();
                        var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                        Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual);
                    }
                }

                _citaRepo.ActualizarEstado(id, "Completada");
            }
            else 
            {
                _citaRepo.ActualizarEstado(id, "Confirmada"); // Rollback
            }

            CargarDatos();
        }
        
        private void BtnCompletar_Click(object sender, EventArgs e)
        {
            if (dgvCitas.SelectedRows.Count == 0) return;
            var id = (int)dgvCitas.SelectedRows[0].Cells["Id"].Value;
            _citaRepo.ActualizarEstado(id, "Completada");
            CargarDatos();
        }
    }
}
