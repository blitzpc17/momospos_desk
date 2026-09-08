using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;
using momospos.Repositories;
using System.Collections.Generic;

namespace MomosClinic.Views.Dialogs
{
    public class CitaForm : Form
    {
        public Cita CitaConfigurada { get; private set; }
        private DateTime _fechaSugerida;
        private TimeSpan? _horaSeleccionada;
        
        // UI Controls - Columna Izquierda
        private TextBox txtNombrePaciente;
        private int? _pacienteIdSeleccionado;
        private Button btnBuscarPaciente;
        private Button btnNuevoPaciente;
        private DateTimePicker dtpFecha;
        private ComboBox cbMedico;
        private TextBox txtMotivo;
        private TextBox txtNotas;
        private Button btnGuardar;

        // UI Controls - Columna Derecha (Calendario)
        private FlowLayoutPanel flpHorarios;
        private Label lblFechaCalendario;
        private Label lblTotalDisponibles;

        private PacienteRepository _pacienteRepo;
        private CitaRepository _citaRepo;
        private MedicoRepository _medicoRepo;
        private ConfiguracionRepository _configRepo;

        // Configuración de la Clínica
        private TimeSpan _horaApertura;
        private TimeSpan _horaCierre;
        private int _duracionCitaMins;

        public CitaForm(DateTime fechaSugerida)
        {
            _fechaSugerida = fechaSugerida;
            _pacienteRepo = new PacienteRepository();
            _citaRepo = new CitaRepository();
            _medicoRepo = new MedicoRepository();
            _configRepo = new ConfiguracionRepository();
            CitaConfigurada = new Cita { Estado = "Programada" };
            
            CargarConfiguraciones();
            BuildUI();
        }

        private void CargarConfiguraciones()
        {
            var config = _configRepo.ObtenerTodas();
            
            _horaApertura = TimeSpan.Parse(config.ContainsKey("HoraAperturaClinica") ? config["HoraAperturaClinica"] : "09:00:00");
            _horaCierre = TimeSpan.Parse(config.ContainsKey("HoraCierreClinica") ? config["HoraCierreClinica"] : "18:00:00");
            _duracionCitaMins = int.Parse(config.ContainsKey("DuracionPromedioCitaMinutos") ? config["DuracionPromedioCitaMinutos"] : "30");
        }

        private void BuildUI()
        {
            this.Text = "Agendar Nueva Cita";
            this.Size = new Size(850, 600); // Más ancho para 2 columnas
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Agendar Cita", Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            // Layout a dos columnas usando dos Paneles
            Panel pnlIzquierda = new Panel { Dock = DockStyle.Left, Width = 480, Padding = new Padding(20) };
            Panel pnlDerecha = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 20, 20, 20), BackColor = Color.White };
            
            this.Controls.Add(pnlDerecha);
            this.Controls.Add(pnlIzquierda);

            ConstruirColumnaIzquierda(pnlIzquierda);
            ConstruirColumnaDerecha(pnlDerecha);

            // Trigger inicial de horarios
            GenerarCuadriculaHorarios();
        }

        private void ConstruirColumnaIzquierda(Panel panel)
        {
            int y = 10;
            
            panel.Controls.Add(new Label { Text = "Paciente:", Location = new Point(10, y), AutoSize = true, Font = Theme.FontNormal });
            
            // Caja de texto "Flat" envolviéndolo en un panel WhiteSmoke
            Panel pnlPaciente = new Panel { Location = new Point(10, y + 25), Width = 350, Height = 35, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };
            txtNombrePaciente = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12), ReadOnly = true, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.None };
            pnlPaciente.Controls.Add(txtNombrePaciente);
            panel.Controls.Add(pnlPaciente);
            
            btnBuscarPaciente = new Button { Text = "🔍", Location = new Point(365, y + 25), Width = 45, Height = 35, FlatStyle = FlatStyle.Flat };
            btnBuscarPaciente.FlatAppearance.BorderSize = 0;
            btnBuscarPaciente.BackColor = Theme.SecondaryColor;
            btnBuscarPaciente.ForeColor = Color.White;
            btnBuscarPaciente.Click += BtnBuscarPaciente_Click;
            panel.Controls.Add(btnBuscarPaciente);
            
            btnNuevoPaciente = new Button { Text = "➕", Location = new Point(415, y + 25), Width = 45, Height = 35, FlatStyle = FlatStyle.Flat };
            btnNuevoPaciente.FlatAppearance.BorderSize = 0;
            btnNuevoPaciente.BackColor = Theme.PrimaryColor;
            btnNuevoPaciente.ForeColor = Color.White;
            btnNuevoPaciente.Click += BtnNuevoPaciente_Click;
            panel.Controls.Add(btnNuevoPaciente);
            
            y += 70;

            panel.Controls.Add(new Label { Text = "Médico (Opcional):", Location = new Point(10, y), AutoSize = true, Font = Theme.FontNormal });
            cbMedico = new ComboBox { Location = new Point(10, y + 25), Width = 450, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            
            var medicos = new List<Medico> { new Medico { Id = 0, NombreCompleto = "Libre / Sin Asignar" } };
            medicos.AddRange(_medicoRepo.ObtenerActivos());
            cbMedico.DataSource = medicos;
            cbMedico.DisplayMember = "NombreCompleto";
            cbMedico.ValueMember = "Id";

            // Seleccionar default
            var config = _configRepo.ObtenerTodas();
            if (config.ContainsKey("MedicoPorDefectoId") && int.TryParse(config["MedicoPorDefectoId"], out int defId))
            {
                if (medicos.Any(m => m.Id == defId)) cbMedico.SelectedValue = defId;
            }
            cbMedico.SelectedIndexChanged += (s, e) => GenerarCuadriculaHorarios();
            panel.Controls.Add(cbMedico);
            y += 70;

            panel.Controls.Add(new Label { Text = "Fecha de la Cita:", Location = new Point(10, y), AutoSize = true, Font = Theme.FontNormal });
            dtpFecha = new DateTimePicker { Location = new Point(10, y + 25), Width = 200, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Short };
            dtpFecha.MinDate = DateTime.Today;
            dtpFecha.Value = _fechaSugerida.Date < DateTime.Today ? DateTime.Today : _fechaSugerida.Date;
            dtpFecha.ValueChanged += (s, e) => GenerarCuadriculaHorarios();
            panel.Controls.Add(dtpFecha);
            y += 70;

            panel.Controls.Add(new Label { Text = "Motivo de Consulta:", Location = new Point(10, y), AutoSize = true, Font = Theme.FontNormal });
            Panel pnlMotivo = new Panel { Location = new Point(10, y + 25), Width = 450, Height = 35, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };
            txtMotivo = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12), BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.None };
            pnlMotivo.Controls.Add(txtMotivo);
            panel.Controls.Add(pnlMotivo);
            y += 70;

            panel.Controls.Add(new Label { Text = "Notas (Opcional):", Location = new Point(10, y), AutoSize = true, Font = Theme.FontNormal });
            Panel pnlNotas = new Panel { Location = new Point(10, y + 25), Width = 450, Height = 70, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };
            txtNotas = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12), BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.None, Multiline = true };
            pnlNotas.Controls.Add(txtNotas);
            panel.Controls.Add(pnlNotas);
            y += 90;

            btnGuardar = new Button { Text = "💾 Confirmar Cita", Location = new Point(90, y), Width = 300, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;
            panel.Controls.Add(btnGuardar);
        }

        private void ConstruirColumnaDerecha(Panel panel)
        {
            lblFechaCalendario = new Label { Text = "Disponibilidad para " + dtpFecha.Value.ToString("dd MMM yyyy"), Font = Theme.FontSubtitle, AutoSize = true, Location = new Point(0, 0), ForeColor = Theme.TextDark };
            panel.Controls.Add(lblFechaCalendario);

            lblTotalDisponibles = new Label { Text = "Cargando horarios...", Font = new Font("Segoe UI", 10), AutoSize = true, Location = new Point(0, 30), ForeColor = Color.Gray };
            panel.Controls.Add(lblTotalDisponibles);

            flpHorarios = new FlowLayoutPanel();
            flpHorarios.Location = new Point(0, 60);
            flpHorarios.Size = new Size(panel.Width, panel.Height - 60);
            flpHorarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpHorarios.AutoScroll = true;
            panel.Controls.Add(flpHorarios);
        }

        private void GenerarCuadriculaHorarios()
        {
            if (flpHorarios == null) return;
            flpHorarios.Controls.Clear();
            _horaSeleccionada = null;

            DateTime fechaSel = dtpFecha.Value.Date;
            lblFechaCalendario.Text = "Disponibilidad para " + fechaSel.ToString("dd MMM yyyy");

            int? medicoId = null;
            if (cbMedico != null && cbMedico.SelectedValue != null && int.TryParse(cbMedico.SelectedValue.ToString(), out int val))
            {
                medicoId = val;
            }
            if (medicoId == 0) medicoId = null;

            // Obtener todas las citas del día seleccionado para este médico (o sin médico)
            var citasDelDia = _citaRepo.ObtenerCitasDelDia(fechaSel);
            
            var horariosOcupados = new HashSet<TimeSpan>();
            foreach(var cita in citasDelDia)
            {
                if (cita.Estado != "Cancelada")
                {
                    if (medicoId.HasValue && medicoId.Value > 0) 
                    {
                        // Si consultamos un médico específico, se bloquea si él ya tiene cita, o si hay una cita Libre (que podría asignarse a él)
                        if (cita.MedicoId == medicoId.Value || cita.MedicoId == null) 
                        {
                            horariosOcupados.Add(cita.FechaHora.TimeOfDay);
                        }
                    }
                    else
                    {
                        // Si consultamos Libre, se bloquea si YA HAY ALGUNA cita en ese horario (evita sobrecupo)
                        horariosOcupados.Add(cita.FechaHora.TimeOfDay);
                    }
                }
            }

            TimeSpan iterador = _horaApertura;
            int disponiblesCount = 0;

            while (iterador < _horaCierre)
            {
                Button btnSlot = new Button();
                btnSlot.Text = iterador.ToString(@"hh\:mm");
                btnSlot.Width = 75;
                btnSlot.Height = 40;
                btnSlot.FlatStyle = FlatStyle.Flat;
                btnSlot.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                btnSlot.Tag = iterador;

                // Si es un día pasado o una hora pasada de hoy
                bool esPasado = fechaSel < DateTime.Today || (fechaSel == DateTime.Today && iterador <= DateTime.Now.TimeOfDay);
                bool estaOcupado = horariosOcupados.Contains(iterador);

                if (esPasado || estaOcupado)
                {
                    btnSlot.Enabled = false;
                    btnSlot.BackColor = Color.LightGray;
                    btnSlot.ForeColor = Color.DarkGray;
                    btnSlot.FlatAppearance.BorderColor = Color.LightGray;
                }
                else
                {
                    disponiblesCount++;
                    btnSlot.BackColor = Color.White;
                    btnSlot.ForeColor = Theme.PrimaryColor;
                    btnSlot.FlatAppearance.BorderColor = Theme.PrimaryColor;
                    btnSlot.Cursor = Cursors.Hand;
                    btnSlot.Click += BtnSlot_Click;
                }

                flpHorarios.Controls.Add(btnSlot);
                iterador = iterador.Add(TimeSpan.FromMinutes(_duracionCitaMins));
            }

            lblTotalDisponibles.Text = disponiblesCount > 0 ? $"{disponiblesCount} horarios disponibles" : "Sin horarios disponibles";
        }

        private void BtnSlot_Click(object sender, EventArgs e)
        {
            Button seleccionado = (Button)sender;
            _horaSeleccionada = (TimeSpan)seleccionado.Tag;

            // Limpiar estilos de los demás
            foreach (Control c in flpHorarios.Controls)
            {
                if (c is Button b && b.Enabled)
                {
                    b.BackColor = Color.White;
                    b.ForeColor = Theme.PrimaryColor;
                }
            }

            // Resaltar seleccionado
            seleccionado.BackColor = Theme.PrimaryColor;
            seleccionado.ForeColor = Color.White;
        }

        private void BtnBuscarPaciente_Click(object sender, EventArgs e)
        {
            using (var buscador = new BuscadorPacienteForm(false))
            {
                if (buscador.ShowDialog() == DialogResult.OK && buscador.PacienteSeleccionado != null)
                {
                    _pacienteIdSeleccionado = buscador.PacienteSeleccionado.Id;
                    txtNombrePaciente.Text = buscador.PacienteSeleccionado.NombreCompleto;
                }
            }
        }

        private void BtnNuevoPaciente_Click(object sender, EventArgs e)
        {
            using (var expressForm = new PacienteExpressForm())
            {
                if (expressForm.ShowDialog() == DialogResult.OK && expressForm.PacienteActual != null)
                {
                    var todos = _pacienteRepo.ObtenerTodos().ToList();
                    var creado = todos.OrderByDescending(p => p.Id).FirstOrDefault(); 

                    if (creado != null)
                    {
                        _pacienteIdSeleccionado = creado.Id;
                        txtNombrePaciente.Text = creado.NombreCompleto;
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!_pacienteIdSeleccionado.HasValue)
            {
                CustomMessageBox.Show("Seleccione un paciente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_horaSeleccionada.HasValue)
            {
                CustomMessageBox.Show("Seleccione un horario disponible en el calendario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime fechaFinal = dtpFecha.Value.Date.Add(_horaSeleccionada.Value);

            int? medicoId = (int)cbMedico.SelectedValue;
            if (medicoId == 0) medicoId = null;

            // Doble check de seguridad por si alguien más la ocupó en lo que teniamos abierta la ventana
            if (_citaRepo.ExisteCitaEnFechaHora(fechaFinal, medicoId))
            {
                CustomMessageBox.Show("Al parecer alguien más acaba de agendar este horario. Por favor seleccione otro.", "Horario no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GenerarCuadriculaHorarios();
                return;
            }

            CitaConfigurada.PacienteId = _pacienteIdSeleccionado.Value;
            CitaConfigurada.MedicoId = medicoId;
            CitaConfigurada.FechaHora = fechaFinal;
            CitaConfigurada.Motivo = txtMotivo.Text.Trim();
            CitaConfigurada.Notas = txtNotas.Text.Trim();

            var citaActiva = _citaRepo.ObtenerCitaActivaPaciente(CitaConfigurada.PacienteId);
            if (citaActiva != null)
            {
                var dialogResult = CustomMessageBox.Show(
                    "¿Desea cambiar la fecha de la cita ya agendada?", 
                    "Paciente con cita activa", 
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Cancel)
                {
                    return; // No hace nada, se queda en la ventana
                }
                else if (dialogResult == DialogResult.Yes)
                {
                    _citaRepo.ActualizarFechaHora(citaActiva.Id, CitaConfigurada.FechaHora, CitaConfigurada.MedicoId, CitaConfigurada.Motivo, CitaConfigurada.Notas);
                    this.DialogResult = DialogResult.Ignore; // Indica actualización en vez de insert
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
