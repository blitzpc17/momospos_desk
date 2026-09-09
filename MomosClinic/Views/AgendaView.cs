using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;
using momospos.Repositories;

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
        private ConfiguracionRepository _configRepo;
        
        public AgendaView()
        {
            _citaRepo = new CitaRepository();
            _configRepo = new ConfiguracionRepository();
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
            var menuModificar = new ToolStripMenuItem("Modificar / Reagendar cita", null, BtnModificarCita_Click);
            var menuCancelar = new ToolStripMenuItem("Cancelar cita", null, BtnCancelarCita_Click);
            cms.Items.Add(menuModificar);
            cms.Items.Add(menuCancelar);
            dgvCitas.ContextMenuStrip = cms;

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);

            AplicarConfiguracionSecretario();
        }

        private void AplicarConfiguracionSecretario()
        {
            var conf = _configRepo.ObtenerTodas();
            bool usoSecretario = true; // Por defecto
            if (conf.ContainsKey("Clinic_UsoSecretario"))
            {
                bool.TryParse(conf["Clinic_UsoSecretario"], out usoSecretario);
            }

            btnAtender.Visible = usoSecretario;
            btnCompletar.Visible = usoSecretario;
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
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        row.DefaultCellStyle.SelectionBackColor = Color.LightCoral;
                    }
                    else if (estado == "Finalizada" || estado == "Completada")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                        row.DefaultCellStyle.SelectionBackColor = Color.ForestGreen;
                    }
                    else if (estado == "En Consulta" || estado == "En Curso")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        row.DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                        row.DefaultCellStyle.SelectionBackColor = Color.Gold;
                    }
                    else if (estado == "Pendiente" || estado == "Confirmada" || estado == "Programada")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        row.DefaultCellStyle.ForeColor = Color.DarkBlue;
                        row.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
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

            var id = (int)dgvCitas.SelectedRows[0].Cells["Id"].Value;
            var citaExistente = _citaRepo.ObtenerPorId(id);

            var form = new MomosClinic.Views.Dialogs.CitaForm(citaExistente);
            var result = form.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Ignore)
            {
                CargarDatos();
            }
        }

        private void BtnCancelarCita_Click(object sender, EventArgs e)
        {
            if (dgvCitas.SelectedRows.Count == 0) return;
            var id = (int)dgvCitas.SelectedRows[0].Cells["Id"].Value;
            var estado = dgvCitas.SelectedRows[0].Cells["Estado"].Value.ToString();

            if (estado == "Completada" || estado == "Cancelada")
            {
                CustomMessageBox.Show("Esta cita ya está " + estado.ToLower() + " y no se puede cancelar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new MomosClinic.Views.Dialogs.CancelarCitaForm("Cancelación de Cita"))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _citaRepo.ActualizarEstado(id, "Cancelada", form.MotivoSeleccionado, form.NotasExtra);
                    CargarDatos();
                }
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

                    var pacienteRepo = new PacienteRepository();
                    var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                    
                    var printer = new MomosClinic.Services.RecetaPrinter(paciente, form.ConsultaActual, form.RecetaActual);
                    var configR = new momospos.Repositories.ConfiguracionRepository();
                    using (var dlg = new MomosClinic.Views.Dialogs.RecetaPrintOptionsDialog(configR.ObtenerValor("TamanoReceta"), printer))
                    {
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            // Enviar a caja si hay medicamentos de farmacia
                            Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual);

                            if (!string.IsNullOrEmpty(dlg.TempPdfPath))
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.TempPdfPath) { UseShellExecute = true });
                            }
                        }
                        else 
                        {
                            // Si no imprime, igual enviarlo a caja si hay medicamentos de farmacia
                            Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual);
                        }
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
