using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views
{
    public class MedicosView : UserControl
    {
        private DataGridView dgvMedicos;
        private MedicoRepository _repo;
        private TextBox txtBusqueda;

        public MedicosView()
        {
            _repo = new MedicoRepository();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "👨‍⚕️ Médicos", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            topPanel.Controls.Add(lblTitulo);

            Button btnNuevo = new Button { Text = "➕ Nuevo Médico", Height = 40, Location = new Point(this.Width - 220, 15), Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true };
            Theme.StyleButton(btnNuevo, Theme.PrimaryColor, Color.White, Theme.FontSubtitle);
            btnNuevo.Click += BtnNuevo_Click;
            topPanel.Controls.Add(btnNuevo);

            this.Controls.Add(topPanel);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(20, 0, 20, 0) };
            Label lblTotal = new Label { Text = "Total de registros: 0", Font = Theme.FontNormal, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 10), Name = "lblTotal" };
            bottomPanel.Controls.Add(lblTotal);
            this.Controls.Add(bottomPanel);

            Panel gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            dgvMedicos = new DataGridView();
            dgvMedicos.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvMedicos);
            dgvMedicos.ColumnHeadersVisible = true;
            dgvMedicos.CellDoubleClick += DgvMedicos_CellDoubleClick;

            ContextMenuStrip cms = new ContextMenuStrip();
            var menuModificar = new ToolStripMenuItem("Modificar médico", null, BtnModificar_Click);
            cms.Items.Add(menuModificar);
            dgvMedicos.ContextMenuStrip = cms;

            gridPanel.Controls.Add(dgvMedicos);

            this.Controls.Add(gridPanel);
            gridPanel.BringToFront();

            CargarDatos();
        }

        private void CargarDatos()
        {
            var medicos = _repo.ObtenerTodos().ToList();
            dgvMedicos.DataSource = medicos;

            if (dgvMedicos.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in dgvMedicos.Columns) col.Visible = false;

                dgvMedicos.Columns["NombreCompleto"].Visible = true;
                dgvMedicos.Columns["NombreCompleto"].MinimumWidth = 250;
                dgvMedicos.Columns["NombreCompleto"].HeaderText = "Nombre del Médico";

                dgvMedicos.Columns["Especialidad"].Visible = true;
                dgvMedicos.Columns["Especialidad"].MinimumWidth = 200;
                dgvMedicos.Columns["Especialidad"].HeaderText = "Especialidad";

                dgvMedicos.Columns["Telefono"].Visible = true;
                dgvMedicos.Columns["Telefono"].MinimumWidth = 120;
                dgvMedicos.Columns["Telefono"].HeaderText = "Teléfono";

                dgvMedicos.Columns["Correo"].Visible = true;
                dgvMedicos.Columns["Correo"].MinimumWidth = 150;
                dgvMedicos.Columns["Correo"].HeaderText = "Correo";

                dgvMedicos.Columns["Activo"].Visible = true;
                dgvMedicos.Columns["Activo"].MinimumWidth = 60;
                dgvMedicos.Columns["Activo"].HeaderText = "Activo";
            }
            
            var lblTotal = this.Controls.Find("lblTotal", true).FirstOrDefault() as Label;
            if (lblTotal != null)
            {
                lblTotal.Text = $"Total de registros: {medicos.Count}";
            }
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            MostrarDialogoEdicion(new Medico { Activo = true });
        }

        private void DgvMedicos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvMedicos.Rows[e.RowIndex].DataBoundItem is Medico m)
            {
                MostrarDialogoEdicion(m);
            }
        }

        private void MostrarDialogoEdicion(Medico m)
        {
            using (var form = new Dialogs.MedicoForm(m))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (dgvMedicos.SelectedRows.Count == 0) return;
            if (dgvMedicos.SelectedRows[0].DataBoundItem is Medico m)
            {
                MostrarDialogoEdicion(m);
            }
        }
    }
}
