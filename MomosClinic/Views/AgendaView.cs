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
            
            dtpFechaFiltro = new DateTimePicker { Location = new Point(250, 27), Width = 330, Font = Theme.FontNormal, Format = DateTimePickerFormat.Long };
            dtpFechaFiltro.ValueChanged += (s, e) => CargarDatos();

            btnNuevaCita = new Button { Text = "➕ Nueva Cita", Location = new Point(590, 25), Width = 150, Height = 35 };
            Theme.StyleButton(btnNuevaCita, Theme.PrimaryColor);
            btnNuevaCita.Click += BtnNuevaCita_Click;

            btnAtender = new Button { Text = "🩺 Iniciar Consulta", Location = new Point(750, 25), Width = 180, Height = 35 };
            Theme.StyleButton(btnAtender, Theme.SuccessColor);
            btnAtender.Click += BtnAtender_Click;
            
            btnCompletar = new Button { Text = "✅ Completar", Location = new Point(940, 25), Width = 130, Height = 35 };
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
            dgvCitas.CellFormatting += DgvCitas_CellFormatting;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvCitas);

            ContextMenuStrip cms = new ContextMenuStrip();
            var menuModificar = new ToolStripMenuItem("Modificar cita", null, BtnModificarCita_Click);
            cms.Items.Add(menuModificar);
            dgvCitas.ContextMenuStrip = cms;

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarDatos()
        {
            var citas = _citaRepo.ObtenerCitasDelDia(dtpFechaFiltro.Value.Date).ToList();
            foreach (var cita in citas)
            {
                if (string.IsNullOrWhiteSpace(cita.NombreMedico)) cita.NombreMedico = "Sin Asignar";
            }
            dgvCitas.DataSource = citas;
            
            if (dgvCitas.Columns.Count > 0)
            {
                dgvCitas.Columns["Id"].Visible = false;
                dgvCitas.Columns["PacienteId"].Visible = false;
                dgvCitas.Columns["MedicoId"].Visible = false;
                dgvCitas.Columns["CreadoEn"].Visible = false;

                dgvCitas.Columns["Folio"].DisplayIndex = 0;
                dgvCitas.Columns["Folio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCitas.Columns["Folio"].Width = 130;

                dgvCitas.Columns["FechaHora"].DisplayIndex = 1;
                dgvCitas.Columns["FechaHora"].HeaderText = "Hora";
                dgvCitas.Columns["FechaHora"].DefaultCellStyle.Format = "hh:mm tt";
                dgvCitas.Columns["FechaHora"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCitas.Columns["FechaHora"].Width = 100;

                dgvCitas.Columns["NombrePaciente"].DisplayIndex = 2;
                dgvCitas.Columns["NombrePaciente"].HeaderText = "Paciente";
                dgvCitas.Columns["NombrePaciente"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                
                dgvCitas.Columns["NombreMedico"].DisplayIndex = 3;
                dgvCitas.Columns["NombreMedico"].HeaderText = "Médico";
                dgvCitas.Columns["NombreMedico"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCitas.Columns["NombreMedico"].Width = 200;

                dgvCitas.Columns["Motivo"].DisplayIndex = 4;
                dgvCitas.Columns["Motivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCitas.Columns["Motivo"].Width = 200;

                dgvCitas.Columns["Estado"].DisplayIndex = 5;
                dgvCitas.Columns["Estado"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCitas.Columns["Estado"].Width = 100;

                dgvCitas.Columns["Notas"].DisplayIndex = 6;
                dgvCitas.Columns["Notas"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCitas.Columns["Notas"].Width = 150;
            }
        }

        private void BtnNuevaCita_Click(object sender, EventArgs e)
        {
            var form = new MomosClinic.Views.Dialogs.CitaForm(dtpFechaFiltro.Value.Date);
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                _citaRepo.Insertar(form.CitaConfigurada);
                CargarDatos();
            }
            else if (result == DialogResult.Ignore)
            {
                // Ya se actualizó en la base de datos dentro de CitaForm
                CargarDatos();
            }
        }

        private void DgvCitas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvCitas.Rows.Count)
            {
                var row = dgvCitas.Rows[e.RowIndex];
                var estado = row.Cells["Estado"].Value?.ToString();
                
                if (row.Cells["FechaHora"].Value != null && row.Cells["FechaHora"].Value is DateTime fechaHora)
                {
                    if (estado == "Cancelada")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        row.DefaultCellStyle.ForeColor = Color.DimGray;
                        row.DefaultCellStyle.SelectionBackColor = Color.DarkGray;
                    }
                    else if (estado == "Completada")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                        row.DefaultCellStyle.SelectionBackColor = Color.ForestGreen;
                    }
                    else if (fechaHora < DateTime.Now)
                    {
                        // Ya pasó y no está completada ni cancelada (posible falta)
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        row.DefaultCellStyle.SelectionBackColor = Color.IndianRed;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Theme.TextDark;
                    }
                }
            }
        }

        private void BtnModificarCita_Click(object sender, EventArgs e)
        {
            if (dgvCitas.SelectedRows.Count == 0) return;
            var estado = dgvCitas.SelectedRows[0].Cells["Estado"].Value.ToString();
            
            if (estado == "Completada" || estado == "Cancelada")
            {
                CustomMessageBox.Show("Esta cita ya está " + estado.ToLower() + " y no se puede modificar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fechaHora = (DateTime)dgvCitas.SelectedRows[0].Cells["FechaHora"].Value;
            if (fechaHora < DateTime.Now)
            {
                CustomMessageBox.Show("No se puede modificar una cita cuyo horario ya ha pasado. Marquela como Cancelada o genere una nueva.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CustomMessageBox.Show("La función de edición de cita completa estará disponible en la próxima actualización.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAtender_Click(object sender, EventArgs e)
        {
            if (dgvCitas.SelectedRows.Count == 0) return;
            var id = (int)dgvCitas.SelectedRows[0].Cells["Id"].Value;
            var pacienteId = (int)dgvCitas.SelectedRows[0].Cells["PacienteId"].Value;
            var estado = dgvCitas.SelectedRows[0].Cells["Estado"].Value.ToString();
            
            if (estado == "Completada" || estado == "Cancelada")
            {
                momospos.Views.CustomMessageBox.Show("Esta cita ya no puede ser atendida.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    if (momospos.Views.CustomMessageBox.Show("¿Desea imprimir la receta médica?", "Imprimir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
