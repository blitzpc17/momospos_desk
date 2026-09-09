using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views
{
    public class RecetasView : UserControl
    {
        private DataGridView dgvRecetas;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Label lblTotalRegistros;
        private ContextMenuStrip cmsOpciones;
        
        private RecetaRepository _repo;

        public RecetasView()
        {
            _repo = new RecetaRepository();
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // Panel Superior
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "💊 Historial de Recetas", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            txtBuscar = new TextBox { Location = new Point(350, 27), Width = 300, Font = Theme.FontNormal, ReadOnly = true, Text = "Ningún paciente seleccionado..." };
            btnBuscar = new Button { Text = "🔍 Buscar Paciente", Location = new Point(660, 25), Width = 150, Height = 35 };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor);
            btnBuscar.Click += BtnBuscar_Click;

            lblTotalRegistros = new Label { Text = "Registros: 0", Location = new Point(830, 32), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.SecondaryColor };

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(txtBuscar);
            topPanel.Controls.Add(btnBuscar);
            topPanel.Controls.Add(lblTotalRegistros);

            cmsOpciones = new ContextMenuStrip();
            cmsOpciones.Items.Add("🖨️ Reimprimir", null, BtnReimprimir_Click);
            cmsOpciones.Items.Add("👁️ Ver Detalle", null, BtnVerDetalle_Click);

            // DataGridView
            dgvRecetas = new DataGridView();
            dgvRecetas.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvRecetas);
            dgvRecetas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecetas.MultiSelect = false;
            dgvRecetas.ContextMenuStrip = cmsOpciones;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvRecetas);

            this.Controls.Add(marginPanel);
            this.Controls.Add(topPanel);
            marginPanel.BringToFront();
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            var dialog = new MomosClinic.Views.Dialogs.BuscadorPacienteForm();
            if (dialog.ShowDialog() == DialogResult.OK && dialog.PacienteSeleccionado != null)
            {
                txtBuscar.Text = $"[{dialog.PacienteSeleccionado.Clave}] {dialog.PacienteSeleccionado.NombreCompleto}";
                CargarDatosPaciente(dialog.PacienteSeleccionado.Id);
            }
        }

        private void CargarDatosPaciente(int pacienteId)
        {
            try
            {
                var recetas = _repo.ObtenerPorPaciente(pacienteId).ToList();
                dgvRecetas.DataSource = recetas;
                AplicarFormatoGrid();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error cargando recetas del paciente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarDatos(string query = "")
        {
            try
            {
                var recetas = _repo.BuscarRecientes(query).ToList();
                dgvRecetas.DataSource = recetas;
                AplicarFormatoGrid();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error cargando recetas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFormatoGrid()
        {
            if (dgvRecetas.Columns.Count > 0)
            {
                dgvRecetas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                dgvRecetas.Columns["Id"].Visible = false;
                dgvRecetas.Columns["Id"].Frozen = true;
                
                if (dgvRecetas.Columns.Contains("ConsultaId")) dgvRecetas.Columns["ConsultaId"].Visible = false;
                if (dgvRecetas.Columns.Contains("PacienteId")) dgvRecetas.Columns["PacienteId"].Visible = false;

                if (dgvRecetas.Columns.Contains("FechaEmision")) 
                {
                    dgvRecetas.Columns["FechaEmision"].Width = 150;
                    dgvRecetas.Columns["FechaEmision"].HeaderText = "Fecha Emisión";
                    dgvRecetas.Columns["FechaEmision"].Frozen = true;
                }
                
                if (dgvRecetas.Columns.Contains("PacienteNombre")) 
                {
                    dgvRecetas.Columns["PacienteNombre"].Width = 250;
                    dgvRecetas.Columns["PacienteNombre"].HeaderText = "Nombre Paciente";
                    dgvRecetas.Columns["PacienteNombre"].Frozen = true;
                }

                if (dgvRecetas.Columns.Contains("IndicacionesGenerales")) 
                {
                    dgvRecetas.Columns["IndicacionesGenerales"].Width = 500;
                    dgvRecetas.Columns["IndicacionesGenerales"].HeaderText = "Indicaciones Generales";
                }
                
                lblTotalRegistros.Text = $"Registros: {dgvRecetas.RowCount}";
            }
        }

        private void BtnReimprimir_Click(object sender, EventArgs e)
        {
            if (dgvRecetas.SelectedRows.Count == 0) return;
            int id = (int)dgvRecetas.SelectedRows[0].Cells["Id"].Value;
            int consultaId = (int)dgvRecetas.SelectedRows[0].Cells["ConsultaId"].Value;
            int pacienteId = (int)dgvRecetas.SelectedRows[0].Cells["PacienteId"].Value;

            try
            {
                var receta = _repo.ObtenerCompleta(id);
                var pacienteRepo = new PacienteRepository();
                var consultaRepo = new ConsultaRepository();

                var paciente = pacienteRepo.ObtenerPorId(pacienteId);
                var consulta = consultaRepo.ObtenerPorId(consultaId);

                if (receta != null && paciente != null && consulta != null)
                {
                    var printer = new MomosClinic.Services.RecetaPrinter(paciente, consulta, receta);
                    printer.Imprimir(mostrarVistaPrevia: true);
                }
                else
                {
                    momospos.Views.CustomMessageBox.Show("Faltan datos (Consulta o Paciente eliminado) para generar la receta.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al reimprimir receta: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnVerDetalle_Click(object sender, EventArgs e)
        {
            if (dgvRecetas.SelectedRows.Count == 0) return;
            int id = (int)dgvRecetas.SelectedRows[0].Cells["Id"].Value;
            int consultaId = (int)dgvRecetas.SelectedRows[0].Cells["ConsultaId"].Value;
            int pacienteId = (int)dgvRecetas.SelectedRows[0].Cells["PacienteId"].Value;

            var detalleForm = new MomosClinic.Views.Dialogs.RecetaDetalleForm(id, consultaId, pacienteId);
            detalleForm.ShowDialog();
        }
    }
}
