using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class CitaForm : Form
    {
        public Cita CitaConfigurada { get; private set; }
        private DateTime _fechaSugerida;
        
        private ComboBox cbPaciente;
        private DateTimePicker dtpHora;
        private TextBox txtMotivo;
        private TextBox txtNotas;
        
        private PacienteRepository _pacienteRepo;
        private CitaRepository _citaRepo;

        public CitaForm(DateTime fechaSugerida)
        {
            _fechaSugerida = fechaSugerida;
            _pacienteRepo = new PacienteRepository();
            _citaRepo = new CitaRepository();
            CitaConfigurada = new Cita { Estado = "Programada" };
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Nueva Cita";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Agendar Cita", Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            int y = 80;
            
            this.Controls.Add(new Label { Text = "Paciente:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            cbPaciente = new ComboBox { Location = new Point(30, y + 25), Width = 420, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            var pacientes = _pacienteRepo.ObtenerTodos().ToList();
            cbPaciente.DataSource = pacientes;
            cbPaciente.DisplayMember = "NombreCompleto";
            cbPaciente.ValueMember = "Id";
            this.Controls.Add(cbPaciente);
            y += 70;

            this.Controls.Add(new Label { Text = "Hora de la Cita:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            dtpHora = new DateTimePicker { Location = new Point(30, y + 25), Width = 150, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Time, ShowUpDown = true };
            dtpHora.Value = new DateTime(_fechaSugerida.Year, _fechaSugerida.Month, _fechaSugerida.Day, 9, 0, 0);
            this.Controls.Add(dtpHora);
            y += 70;

            this.Controls.Add(new Label { Text = "Motivo de Consulta:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            txtMotivo = new TextBox { Location = new Point(30, y + 25), Width = 420, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtMotivo);
            y += 70;

            this.Controls.Add(new Label { Text = "Notas (Opcional):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            txtNotas = new TextBox { Location = new Point(30, y + 25), Width = 420, Height = 60, Multiline = true, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtNotas);
            y += 90;

            Button btnGuardar = new Button { Text = "💾 Agendar", Location = new Point(90, y), Width = 150, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(260, y), Width = 150, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cbPaciente.SelectedValue == null)
            {
                CustomMessageBox.Show("Seleccione un paciente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_citaRepo.ExisteCitaEnFechaHora(dtpHora.Value))
            {
                CustomMessageBox.Show("Ya existe una cita programada para esta misma fecha y hora. Por favor seleccione otro horario.", "Horario no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CitaConfigurada.PacienteId = (int)cbPaciente.SelectedValue;
            CitaConfigurada.FechaHora = dtpHora.Value;
            CitaConfigurada.Motivo = txtMotivo.Text.Trim();
            CitaConfigurada.Notas = txtNotas.Text.Trim();

            this.DialogResult = DialogResult.OK;
        }
    }
}
