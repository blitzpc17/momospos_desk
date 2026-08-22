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
        private Button btnVerDetalle;
        private Button btnNuevaConsultaLibre;
        
        private ConsultaRepository _repo;

        public ConsultasView()
        {
            _repo = new ConsultaRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "⚕️ Historial de Consultas", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            txtBuscar = new TextBox { Location = new Point(350, 27), Width = 250, Font = Theme.FontNormal };
            btnBuscar = new Button { Text = "🔍 Buscar", Location = new Point(610, 25), Width = 100, Height = 35 };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor);
            btnBuscar.Click += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevaConsultaLibre = new Button { Text = "➕ Consulta Rápida (Sin Cita)", Location = new Point(730, 25), Width = 220, Height = 35 };
            Theme.StyleButton(btnNuevaConsultaLibre, Theme.PrimaryColor);
            btnNuevaConsultaLibre.Click += BtnNuevaConsultaLibre_Click;

            btnVerDetalle = new Button { Text = "👁️ Ver Detalle", Location = new Point(960, 25), Width = 130, Height = 35 };
            Theme.StyleButton(btnVerDetalle, Theme.SecondaryColor);
            btnVerDetalle.Click += BtnVerDetalle_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(txtBuscar);
            topPanel.Controls.Add(btnBuscar);
            topPanel.Controls.Add(btnNuevaConsultaLibre);
            topPanel.Controls.Add(btnVerDetalle);

            dgvConsultas = new DataGridView();
            dgvConsultas.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvConsultas);
            dgvConsultas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConsultas.MultiSelect = false;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvConsultas);

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
            marginPanel.BringToFront();
        }

        private void CargarDatos(string query = "")
        {
            var consultas = _repo.BuscarRecientes(query).ToList();
            dgvConsultas.DataSource = consultas;
            
            if (dgvConsultas.Columns.Count > 0)
            {
                foreach(DataGridViewColumn col in dgvConsultas.Columns) col.Visible = false;
                
                dgvConsultas.Columns["Id"].Visible = true;
                dgvConsultas.Columns["Id"].Width = 50;
                
                dgvConsultas.Columns["NombrePaciente"].HeaderText = "Paciente";
                dgvConsultas.Columns["NombrePaciente"].Visible = true;
                dgvConsultas.Columns["NombrePaciente"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                
                dgvConsultas.Columns["Diagnostico"].Visible = true;
                dgvConsultas.Columns["Diagnostico"].Width = 250;
                
                dgvConsultas.Columns["CreadoEn"].HeaderText = "Fecha";
                dgvConsultas.Columns["CreadoEn"].Visible = true;
                dgvConsultas.Columns["CreadoEn"].Width = 150;
            }
        }

        private void BtnNuevaConsultaLibre_Click(object sender, EventArgs e)
        {
            var form = new MomosClinic.Views.Dialogs.ConsultaForm(null, null); // null pacienteId means user picks
            if (form.ShowDialog() == DialogResult.OK)
            {
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
                        if (MessageBox.Show("¿Desea imprimir la receta médica?", "Imprimir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                
                CargarDatos();
            }
        }

        private void BtnVerDetalle_Click(object sender, EventArgs e)
        {
            if (dgvConsultas.SelectedRows.Count == 0) return;
            MessageBox.Show("Esta pantalla abrirá la consulta en modo solo-lectura para revisión.", "En Construcción");
        }
    }
}
