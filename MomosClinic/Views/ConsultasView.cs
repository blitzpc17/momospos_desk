using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views
{
    public class ConsultasView : UserControl
    {
        private DataGridView dgvConsultas;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Button btnAtender;
        private Button btnNuevaConsultaLibre;
        
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFin;
        
        private CitaRepository _repo;
        private momospos.Models.Usuario _usuarioLogueado;

        public ConsultasView(momospos.Models.Usuario usuarioLogueado)
        {
            _usuarioLogueado = usuarioLogueado;
            _repo = new CitaRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "⚕️ Historial de Consultas", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            Label lblInicio = new Label { Text = "Desde:", AutoSize = true, Location = new Point(350, 32) };
            dtpInicio = new DateTimePicker { Location = new Point(400, 27), Width = 110, Format = DateTimePickerFormat.Short };
            
            Label lblFin = new Label { Text = "Hasta:", AutoSize = true, Location = new Point(520, 32) };
            dtpFin = new DateTimePicker { Location = new Point(560, 27), Width = 110, Format = DateTimePickerFormat.Short };

            txtBuscar = new TextBox { Location = new Point(680, 27), Width = 150, Font = Theme.FontNormal };
            btnBuscar = new Button { Text = "🔍 Buscar", Location = new Point(840, 25), Width = 90, Height = 35 };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor);
            btnBuscar.Click += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevaConsultaLibre = new Button { Text = "➕ Rápida", Location = new Point(940, 25), Width = 100, Height = 35 };
            Theme.StyleButton(btnNuevaConsultaLibre, Theme.PrimaryColor);
            btnNuevaConsultaLibre.Click += BtnNuevaConsultaLibre_Click;

            btnAtender = new Button { Text = "🩺 Iniciar Consulta", Location = new Point(1050, 25), Width = 150, Height = 35 };
            Theme.StyleButton(btnAtender, Theme.SuccessColor);
            btnAtender.Click += BtnAtender_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblInicio);
            topPanel.Controls.Add(dtpInicio);
            topPanel.Controls.Add(lblFin);
            topPanel.Controls.Add(dtpFin);
            topPanel.Controls.Add(txtBuscar);
            topPanel.Controls.Add(btnBuscar);
            topPanel.Controls.Add(btnNuevaConsultaLibre);
            topPanel.Controls.Add(btnAtender);

            dgvConsultas = new DataGridView();
            dgvConsultas.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvConsultas);
            dgvConsultas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConsultas.MultiSelect = false;
            dgvConsultas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvConsultas.ScrollBars = ScrollBars.Both;

            ContextMenuStrip cms = new ContextMenuStrip();
            var menuIniciar = new ToolStripMenuItem("Iniciar Consulta", null, BtnAtender_Click);
            var menuCancelar = new ToolStripMenuItem("Cancelar Cita", null, BtnCancelarCita_Click);
            cms.Items.Add(menuIniciar);
            cms.Items.Add(menuCancelar);
            dgvConsultas.ContextMenuStrip = cms;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvConsultas);

            pnlTotales = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 30, FlowDirection = FlowDirection.LeftToRight };
            
            lblTotalRegistros = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Margin = new Padding(0, 5, 20, 0) };
            lblPendientes = new Label { Font = Theme.FontNormal, ForeColor = Color.SteelBlue, AutoSize = true, Margin = new Padding(0, 5, 20, 0) };
            lblCompletadas = new Label { Font = Theme.FontNormal, ForeColor = Color.ForestGreen, AutoSize = true, Margin = new Padding(0, 5, 20, 0) };
            lblCanceladas = new Label { Font = Theme.FontNormal, ForeColor = Color.DarkRed, AutoSize = true, Margin = new Padding(0, 5, 20, 0) };
            
            pnlTotales.Controls.Add(lblTotalRegistros);
            pnlTotales.Controls.Add(lblPendientes);
            pnlTotales.Controls.Add(lblCompletadas);
            pnlTotales.Controls.Add(lblCanceladas);

            marginPanel.Controls.Add(pnlTotales);

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
            marginPanel.BringToFront();
        }

        private FlowLayoutPanel pnlTotales;
        private Label lblTotalRegistros;
        private Label lblPendientes;
        private Label lblCompletadas;
        private Label lblCanceladas;

        private void CargarDatos(string query = "")
        {
            var citas = _repo.ObtenerCitasPorRango(dtpInicio.Value, dtpFin.Value, query).ToList();
            
            foreach (var cita in citas)
            {
                if (string.IsNullOrWhiteSpace(cita.NombreMedico)) cita.NombreMedico = "SIN ASIGNAR";
            }
            
            // Si no es admin y quisieras filtrar por el médico logueado, lo harías aquí en memoria 
            // o pasando el id al repositorio, pero asumiendo que el usuario quiere ver "su" historial,
            // de momento mostramos todas las del rango, y ocultamos la columna de médico si no es admin.

            dgvConsultas.DataSource = citas;
            
            if (dgvConsultas.Columns.Count > 0)
            {
                foreach(DataGridViewColumn col in dgvConsultas.Columns) col.Visible = false;
                
                dgvConsultas.Columns["NombrePaciente"].HeaderText = "Paciente";
                dgvConsultas.Columns["NombrePaciente"].Visible = true;
                dgvConsultas.Columns["NombrePaciente"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvConsultas.Columns["NombrePaciente"].Width = 300;
                
                dgvConsultas.Columns["FechaHora"].HeaderText = "Horario";
                dgvConsultas.Columns["FechaHora"].Visible = true;
                dgvConsultas.Columns["FechaHora"].DefaultCellStyle.Format = "dd/MM/yyyy hh:mm tt";
                dgvConsultas.Columns["FechaHora"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvConsultas.Columns["FechaHora"].Width = 160;
                
                dgvConsultas.Columns["Estado"].HeaderText = "Estado";
                dgvConsultas.Columns["Estado"].Visible = true;
                dgvConsultas.Columns["Estado"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvConsultas.Columns["Estado"].Width = 130;

                if (_usuarioLogueado != null && _usuarioLogueado.EsAdmin)
                {
                    dgvConsultas.Columns["NombreMedico"].HeaderText = "Médico Asignado";
                    dgvConsultas.Columns["NombreMedico"].Visible = true;
                    dgvConsultas.Columns["NombreMedico"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvConsultas.Columns["NombreMedico"].Width = 250;
                }
            }

            int pendientes = citas.Count(c => c.Estado == "Programada" || c.Estado == "Confirmada");
            int completadas = citas.Count(c => c.Estado == "Completada" || c.Estado == "Finalizada");
            int canceladas = citas.Count(c => c.Estado == "Cancelada");

            lblTotalRegistros.Text = $"Total de registros: {citas.Count}";
            lblPendientes.Text = $"⏳ Pendientes: {pendientes}";
            lblCompletadas.Text = $"✅ Completadas: {completadas}";
            lblCanceladas.Text = $"❌ Canceladas: {canceladas}";
        }

        private void BtnNuevaConsultaLibre_Click(object sender, EventArgs e)
        {
            IniciarConsultaLibre(() => CargarDatos());
        }

        public static void IniciarConsultaLibre(Action onSaved = null)
        {
            var form = new MomosClinic.Views.Dialogs.ConsultaForm(null, null); // null pacienteId means user picks
            if (form.ShowDialog() == DialogResult.OK)
            {
                var _repo = new ConsultaRepository();
                int consultaId = _repo.Insertar(form.ConsultaActual);
                
                if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales) || form.ServicioCobrarId.HasValue)
                {
                    form.RecetaActual.ConsultaId = consultaId;
                    form.RecetaActual.PacienteId = form.ConsultaActual.PacienteId;
                    
                    // Si hay receta escrita (detalles o indicaciones), la guardamos
                    if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales))
                    {
                        var recetaRepo = new RecetaRepository();
                        recetaRepo.Insertar(form.RecetaActual);
                    }

                    if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales))
                    {
                        if (momospos.Views.CustomMessageBox.Show("¿Desea imprimir la receta médica?", "Imprimir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            var pacienteRepo = new PacienteRepository();
                            var paciente = pacienteRepo.ObtenerPorId(form.ConsultaActual.PacienteId);
                            
                            Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual, form.ServicioCobrarId);

                            var printer = new MomosClinic.Services.RecetaPrinter(paciente, form.ConsultaActual, form.RecetaActual);
                            printer.Imprimir();
                        }
                        else
                        {
                            var pacienteRepo = new PacienteRepository();
                            var paciente = pacienteRepo.ObtenerPorId(form.ConsultaActual.PacienteId);
                            Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual, form.ServicioCobrarId);
                        }
                    }
                    else
                    {
                        // Si no hay receta pero SI hay cobro de servicio (ej. solo vino a inyectarse o consulta de revisión)
                        var pacienteRepo = new PacienteRepository();
                        var paciente = pacienteRepo.ObtenerPorId(form.ConsultaActual.PacienteId);
                        Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual, form.ServicioCobrarId);
                    }
                }
                
                onSaved?.Invoke();
            }
        }

        private void BtnAtender_Click(object sender, EventArgs e)
        {
            if (dgvConsultas.SelectedRows.Count == 0) return;
            
            var id = (int)dgvConsultas.SelectedRows[0].Cells["Id"].Value;
            var pacienteId = (int)dgvConsultas.SelectedRows[0].Cells["PacienteId"].Value;
            var estado = dgvConsultas.SelectedRows[0].Cells["Estado"].Value.ToString();
            
            if (estado == "Completada" || estado == "Cancelada")
            {
                momospos.Views.CustomMessageBox.Show("Esta cita ya no puede ser atendida.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _repo.ActualizarEstado(id, "En Curso");
            
            var form = new MomosClinic.Views.Dialogs.ConsultaForm(pacienteId, id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                var consultaRepo = new ConsultaRepository();
                int consultaId = consultaRepo.Insertar(form.ConsultaActual);
                
                if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales) || form.ServicioCobrarId.HasValue)
                {
                    form.RecetaActual.ConsultaId = consultaId;
                    form.RecetaActual.PacienteId = pacienteId;
                    
                    if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales))
                    {
                        var recetaRepo = new RecetaRepository();
                        recetaRepo.Insertar(form.RecetaActual);
                    }

                    if (form.RecetaActual.Detalles.Count > 0 || !string.IsNullOrWhiteSpace(form.RecetaActual.IndicacionesGenerales))
                    {
                        if (momospos.Views.CustomMessageBox.Show("¿Desea imprimir la receta médica?", "Imprimir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            var pacienteRepo = new PacienteRepository();
                            var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                            
                            Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual, form.ServicioCobrarId);

                            var printer = new MomosClinic.Services.RecetaPrinter(paciente, form.ConsultaActual, form.RecetaActual);
                            printer.Imprimir();
                        }
                        else
                        {
                            var pacienteRepo = new PacienteRepository();
                            var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                            Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual, form.ServicioCobrarId);
                        }
                    }
                    else
                    {
                        var pacienteRepo = new PacienteRepository();
                        var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                        Helpers.OrdenCobroHelper.EnviarRecetaACaja(paciente, form.RecetaActual, form.ServicioCobrarId);
                    }
                }

                _repo.ActualizarEstado(id, "Completada");
            }
            else 
            {
                _repo.ActualizarEstado(id, "Confirmada"); // Rollback
            }

            CargarDatos();
        }
        private void BtnCancelarCita_Click(object sender, EventArgs e)
        {
            if (dgvConsultas.SelectedRows.Count == 0) return;
            var id = (int)dgvConsultas.SelectedRows[0].Cells["Id"].Value;
            var estado = dgvConsultas.SelectedRows[0].Cells["Estado"].Value.ToString();

            if (estado == "Completada" || estado == "Cancelada")
            {
                momospos.Views.CustomMessageBox.Show("Esta cita ya está " + estado.ToLower() + " y no se puede cancelar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new MomosClinic.Views.Dialogs.CancelarCitaForm("Cancelación de Cita"))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _repo.ActualizarEstado(id, "Cancelada", form.MotivoSeleccionado, form.NotasExtra);
                    CargarDatos();
                }
            }
        }
    }
}
