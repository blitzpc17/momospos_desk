using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Collections.Generic;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class BuscadorPacienteForm : Form
    {
        private TextBox txtBuscar;
        private DataGridView dgvResultados;
        private PacienteRepository _pacienteRepo;
        private bool _mostrarInactivos;
        private Label lblTitulo;
        private Label lblTotal;
        
        public Paciente PacienteSeleccionado { get; private set; }

        public BuscadorPacienteForm(bool mostrarInactivos = false)
        {
            _pacienteRepo = new PacienteRepository();
            _mostrarInactivos = mostrarInactivos;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Buscar Paciente";
            this.Size = new Size(850, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            lblTitulo = new Label { Text = "🔍 Buscar:", Font = Theme.FontSubtitle, Location = new Point(20, 25), AutoSize = true, ForeColor = Theme.TextDark };
            txtBuscar = new TextBox { Location = new Point(140, 22), Width = 400, Font = Theme.FontTitle };
            txtBuscar.TextChanged += (s, e) => Buscar();
            txtBuscar.KeyDown += TxtBuscar_KeyDown;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(txtBuscar);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(20, 0, 20, 10) };
            lblTotal = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, Dock = DockStyle.Left, AutoSize = true, ForeColor = Theme.TextLight };
            bottomPanel.Controls.Add(lblTotal);

            dgvResultados = new DataGridView();
            dgvResultados.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvResultados);
            dgvResultados.CellDoubleClick += DgvResultados_CellDoubleClick;
            dgvResultados.KeyDown += DgvResultados_KeyDown;

            // Formato de celdas para Inactivos
            dgvResultados.CellFormatting += (s, e) => {
                if (dgvResultados.Columns[e.ColumnIndex].Name == "Activo" && e.Value != null)
                {
                    bool activo = (bool)e.Value;
                    if (!activo) e.CellStyle.ForeColor = Color.Red;
                }
            };

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 0) };
            marginPanel.Controls.Add(dgvResultados);

            this.Controls.Add(marginPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void Buscar()
        {
            string query = txtBuscar.Text.Trim();
            var resultados = _pacienteRepo.Buscar(query, _mostrarInactivos);
            dgvResultados.DataSource = resultados;
            
            lblTotal.Text = $"Total de registros: {dgvResultados.Rows.Count}";
            
            if (dgvResultados.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in dgvResultados.Columns)
                {
                    col.Visible = false;
                }

                if (dgvResultados.Columns.Contains("Clave"))
                {
                    dgvResultados.Columns["Clave"].Visible = true;
                    dgvResultados.Columns["Clave"].Width = 100;
                    dgvResultados.Columns["Clave"].DisplayIndex = 0;
                }
                
                if (dgvResultados.Columns.Contains("NombreCompleto"))
                {
                    dgvResultados.Columns["NombreCompleto"].Visible = true;
                    dgvResultados.Columns["NombreCompleto"].Width = 350;
                    dgvResultados.Columns["NombreCompleto"].HeaderText = "Nombre Completo";
                    dgvResultados.Columns["NombreCompleto"].DisplayIndex = 1;
                }

                if (dgvResultados.Columns.Contains("FechaNacimiento"))
                {
                    dgvResultados.Columns["FechaNacimiento"].Visible = true;
                    dgvResultados.Columns["FechaNacimiento"].Width = 150;
                    dgvResultados.Columns["FechaNacimiento"].HeaderText = "F. Nacimiento";
                    dgvResultados.Columns["FechaNacimiento"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    dgvResultados.Columns["FechaNacimiento"].DisplayIndex = 2;
                }

                if (_mostrarInactivos && dgvResultados.Columns.Contains("Activo"))
                {
                    dgvResultados.Columns["Activo"].Visible = true;
                    dgvResultados.Columns["Activo"].Width = 80;
                    dgvResultados.Columns["Activo"].DisplayIndex = 3;
                }
            }
        }

        private void SeleccionarPaciente()
        {
            if (dgvResultados.CurrentRow != null && dgvResultados.CurrentRow.DataBoundItem is Paciente p)
            {
                PacienteSeleccionado = p;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void DgvResultados_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) SeleccionarPaciente();
        }

        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                dgvResultados.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && dgvResultados.Rows != null && dgvResultados.Rows.Count > 0)
            {
                dgvResultados.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void DgvResultados_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SeleccionarPaciente();
                e.Handled = true;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtBuscar.Focus();
            Buscar();
        }
    }
}
