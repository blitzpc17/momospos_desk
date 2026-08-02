using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Repositories;
using momospos.Models;
using System.Linq;

namespace momospos.Views
{
    public class AutorizacionesView : UserControl
    {
        private DataGridView dgvAutorizaciones;
        private Button btnAutorizar;
        private Button btnRechazar;
        private Label lblTotalPendientes;

        private VentaRepository _ventaRepo;
        private Usuario _usuarioActual;

        public AutorizacionesView(Usuario usuarioActual)
        {
            _usuarioActual = usuarioActual;
            _ventaRepo = new VentaRepository();
            BuildUI();
            CargarAutorizaciones();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // HEADER
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            Label lblTitulo = new Label { Text = "🛡️ Autorizaciones Pendientes", Font = Theme.FontTitle, ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(20, 20) };
            Label lblSubtitulo = new Label { Text = "Módulo exclusivo para usuarios con permisos. Aquí puedes aprobar o rechazar cancelaciones.", Font = Theme.FontNormal, ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 60) };
            
            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(lblSubtitulo);

            // BUTTONS PANEL
            Panel actionsPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20, 10, 20, 10) };
            
            btnAutorizar = new Button { Text = "✅ Autorizar", Location = new Point(20, 20), Width = 150, Height = 40 };
            Theme.StyleButton(btnAutorizar, Theme.SuccessColor);
            btnAutorizar.Click += BtnAutorizar_Click;

            btnRechazar = new Button { Text = "❌ Rechazar", Location = new Point(190, 20), Width = 150, Height = 40 };
            Theme.StyleButton(btnRechazar, Theme.DangerColor);
            btnRechazar.Click += BtnRechazar_Click;

            lblTotalPendientes = new Label { Text = "Pendientes: 0", Font = Theme.FontSubtitle, Location = new Point(360, 25), AutoSize = true };

            actionsPanel.Controls.Add(btnAutorizar);
            actionsPanel.Controls.Add(btnRechazar);
            actionsPanel.Controls.Add(lblTotalPendientes);

            // GRID
            dgvAutorizaciones = new DataGridView();
            dgvAutorizaciones.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(dgvAutorizaciones);
            dgvAutorizaciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Panel marginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            marginPanel.Controls.Add(dgvAutorizaciones);

            this.Controls.Add(marginPanel);
            this.Controls.Add(actionsPanel);
            this.Controls.Add(topPanel);
        }

        private void CargarAutorizaciones()
        {
            try
            {
                var pendientes = _ventaRepo.ObtenerCancelacionesPendientes().ToList();
                dgvAutorizaciones.DataSource = pendientes;

                // Ocultar IDs irrelevantes
                if (dgvAutorizaciones.Columns["Id"] != null) dgvAutorizaciones.Columns["Id"].Visible = false;
                if (dgvAutorizaciones.Columns["VentaId"] != null) dgvAutorizaciones.Columns["VentaId"].Visible = false;
                if (dgvAutorizaciones.Columns["UsuarioSolicitaId"] != null) dgvAutorizaciones.Columns["UsuarioSolicitaId"].Visible = false;
                if (dgvAutorizaciones.Columns["UsuarioAutorizaId"] != null) dgvAutorizaciones.Columns["UsuarioAutorizaId"].Visible = false;

                // Ordenar columnas visualmente
                if (dgvAutorizaciones.Columns["VentaFolio"] != null) dgvAutorizaciones.Columns["VentaFolio"].DisplayIndex = 0;
                if (dgvAutorizaciones.Columns["VentaTotal"] != null) dgvAutorizaciones.Columns["VentaTotal"].DisplayIndex = 1;
                if (dgvAutorizaciones.Columns["Motivo"] != null) dgvAutorizaciones.Columns["Motivo"].DisplayIndex = 2;
                if (dgvAutorizaciones.Columns["NombreSolicitante"] != null) dgvAutorizaciones.Columns["NombreSolicitante"].DisplayIndex = 3;
                if (dgvAutorizaciones.Columns["FechaSolicitud"] != null) dgvAutorizaciones.Columns["FechaSolicitud"].DisplayIndex = 4;
                if (dgvAutorizaciones.Columns["Estado"] != null) dgvAutorizaciones.Columns["Estado"].DisplayIndex = 5;

                lblTotalPendientes.Text = $"Pendientes: {pendientes.Count}";
                
                btnAutorizar.Enabled = pendientes.Count > 0;
                btnRechazar.Enabled = pendientes.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar autorizaciones:\n{ex.Message}");
            }
        }

        private void BtnAutorizar_Click(object sender, EventArgs e)
        {
            ProcesarSolicitud(true);
        }

        private void BtnRechazar_Click(object sender, EventArgs e)
        {
            ProcesarSolicitud(false);
        }

        private void ProcesarSolicitud(bool aprobar)
        {
            if (dgvAutorizaciones.CurrentRow == null || !(dgvAutorizaciones.CurrentRow.DataBoundItem is VentaCancelacion cancelacion))
            {
                MessageBox.Show("Por favor, seleccione una solicitud de la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string accion = aprobar ? "AUTORIZAR" : "RECHAZAR";
            var result = MessageBox.Show($"¿Está seguro que desea {accion} la cancelación de la venta {cancelacion.VentaFolio}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    _ventaRepo.ProcesarCancelacion(cancelacion.Id, _usuarioActual.Id, aprobar);
                    MessageBox.Show($"La cancelación fue {accion.ToLower()}ada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarAutorizaciones();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar solicitud:\n{ex.Message}");
                }
            }
        }
    }
}
