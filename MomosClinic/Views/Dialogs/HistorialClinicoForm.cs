using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class HistorialClinicoForm : Form
    {
        private Paciente _paciente;
        private PacienteRepository _pacienteRepo;

        private TextBox txtAlergias;
        private TextBox txtAntFamiliares;
        private TextBox txtAntPatologicos;
        private TextBox txtHistorialClinico;
        private TextBox txtTipoSangre;

        private Button btnGuardar;

        public HistorialClinicoForm(int pacienteId)
        {
            _pacienteRepo = new PacienteRepository();
            _paciente = _pacienteRepo.ObtenerPorId(pacienteId);

            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Historial Clínico - " + _paciente.NombreCompleto;
            this.Size = new Size(650, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            int y = 20;

            pnl.Controls.Add(new Label { Text = "Tipo de Sangre:", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtTipoSangre = new TextBox { Location = new Point(150, y-2), Width = 150, Font = Theme.FontNormal, Text = _paciente.TipoSangre };
            pnl.Controls.Add(txtTipoSangre);
            y += 40;

            pnl.Controls.Add(new Label { Text = "Alergias:", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtAlergias = new TextBox { Location = new Point(20, y+25), Width = 590, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.FontNormal, Text = _paciente.Alergias };
            pnl.Controls.Add(txtAlergias);
            y += 100;

            pnl.Controls.Add(new Label { Text = "Antecedentes Familiares:", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtAntFamiliares = new TextBox { Location = new Point(20, y+25), Width = 590, Height = 80, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.FontNormal, Text = _paciente.AntecedentesFamiliares };
            pnl.Controls.Add(txtAntFamiliares);
            y += 120;

            pnl.Controls.Add(new Label { Text = "Antecedentes Patológicos:", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtAntPatologicos = new TextBox { Location = new Point(20, y+25), Width = 590, Height = 80, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.FontNormal, Text = _paciente.AntecedentesPatologicos };
            pnl.Controls.Add(txtAntPatologicos);
            y += 120;

            pnl.Controls.Add(new Label { Text = "Historial Clínico (Resumen general/Evolución):", Location = new Point(20, y), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor });
            txtHistorialClinico = new TextBox { Location = new Point(20, y+30), Width = 590, Height = 120, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.FontNormal, Text = _paciente.HistorialClinico };
            pnl.Controls.Add(txtHistorialClinico);
            y += 160;

            btnGuardar = new Button { Text = "Guardar Cambios", Location = new Point(410, y), Width = 200, Height = 40 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor);
            btnGuardar.Click += BtnGuardar_Click;
            pnl.Controls.Add(btnGuardar);

            this.Controls.Add(pnl);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            _paciente.TipoSangre = txtTipoSangre.Text.Trim();
            _paciente.Alergias = txtAlergias.Text.Trim();
            _paciente.AntecedentesFamiliares = txtAntFamiliares.Text.Trim();
            _paciente.AntecedentesPatologicos = txtAntPatologicos.Text.Trim();
            _paciente.HistorialClinico = txtHistorialClinico.Text.Trim();

            _pacienteRepo.ActualizarHistorial(_paciente);
            CustomMessageBox.Show("Historial Clínico actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
