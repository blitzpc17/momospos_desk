using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using System.Linq;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class ConsultaForm : Form
    {
        public Consulta ConsultaActual { get; private set; }
        private PacienteRepository _pacienteRepo;
        
        // Controles Superiores
        private TextBox txtPaciente;
        private Button btnBuscarPaciente;
        private Button btnNuevoPaciente;
        private ComboBox cbServicio;
        public int? ServicioCobrarId { get; private set; }

        // Tabs
        private TabControl tabControl;

        // Signos Vitales
        private NumericUpDown numPeso;
        private NumericUpDown numTalla;
        private NumericUpDown numTemp;
        private TextBox txtPresion;
        private NumericUpDown numFC;
        private NumericUpDown numFR;
        private NumericUpDown numOxigeno;
        private Label lblIMCVal;

        // SOAP
        private TextBox txtMotivo;
        private TextBox txtExploracion;
        private TextBox txtAnalisis;
        private TextBox txtDiagnostico;
        private TextBox txtPlan;

        public ConsultaForm(int? pacienteId = null, int? citaId = null)
        {
            _pacienteRepo = new PacienteRepository();
            ConsultaActual = new Consulta { CitaId = citaId };
            if (pacienteId.HasValue) ConsultaActual.PacienteId = pacienteId.Value;
            
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Consulta Médica";
            this.Size = new Size(950, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Expediente de Consulta", Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            // Paciente Selector
            Panel patientPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(20) };
            patientPanel.Controls.Add(new Label { Text = "Paciente:", Location = new Point(20, 20), AutoSize = true, Font = Theme.FontSubtitle });
            
            txtPaciente = new TextBox { Location = new Point(100, 15), Width = 280, Font = new Font("Segoe UI", 12), ReadOnly = true };
            if (ConsultaActual.PacienteId > 0)
            {
                var p = _pacienteRepo.ObtenerPorId(ConsultaActual.PacienteId);
                if (p != null) txtPaciente.Text = p.NombreCompleto;
            }
            
            btnBuscarPaciente = new Button { Text = "🔍", Location = new Point(390, 14), Width = 40, Height = 32 };
            btnBuscarPaciente.Click += BtnBuscarPaciente_Click;
            btnNuevoPaciente = new Button { Text = "➕", Location = new Point(440, 14), Width = 40, Height = 32 };
            btnNuevoPaciente.Click += BtnNuevoPaciente_Click;

            patientPanel.Controls.Add(txtPaciente);
            patientPanel.Controls.Add(btnBuscarPaciente);
            patientPanel.Controls.Add(btnNuevoPaciente);

            patientPanel.Controls.Add(new Label { Text = "Servicio (Cobro):", Location = new Point(490, 20), AutoSize = true, Font = Theme.FontSubtitle });
            cbServicio = new ComboBox { Location = new Point(640, 15), Width = 280, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            var srvRepo = new ServiciosRepository();
            var servicios = srvRepo.ObtenerTodos();
            servicios.Insert(0, new ServicioMedico { Id = 0, Nombre = "-- Sin cobro de servicio --" });
            cbServicio.DataSource = servicios;
            cbServicio.DisplayMember = "Nombre";
            cbServicio.ValueMember = "Id";
            patientPanel.Controls.Add(cbServicio);

            this.Controls.Add(patientPanel);

            // Tab Control
            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12) };
            
            TabPage tabSignos = new TabPage("Signos Vitales");
            tabSignos.BackColor = Theme.BackgroundColor;
            BuildSignosTab(tabSignos);

            TabPage tabSOAP = new TabPage("SOAP (Clínico)");
            tabSOAP.BackColor = Theme.BackgroundColor;
            BuildSOAPTab(tabSOAP);

            TabPage tabReceta = new TabPage("Receta");
            tabReceta.BackColor = Theme.BackgroundColor;
            BuildRecetaTab(tabReceta);

            tabControl.TabPages.Add(tabSignos);
            tabControl.TabPages.Add(tabSOAP);
            tabControl.TabPages.Add(tabReceta);
            
            Panel fillPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            fillPanel.Controls.Add(tabControl);
            this.Controls.Add(fillPanel);
            
            // Fix docking overlap order
            fillPanel.BringToFront();
            patientPanel.BringToFront();
            topPanel.BringToFront();

            // Bottom Panel
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 70 };
            Button btnGuardar = new Button { Text = "💾 Terminar Consulta", Location = new Point(560, 10), Width = 200, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(780, 10), Width = 130, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            bottomPanel.Controls.Add(btnGuardar);
            bottomPanel.Controls.Add(btnCancelar);
            this.Controls.Add(bottomPanel);
        }

        private void BuildSignosTab(TabPage tab)
        {
            int y = 30;
            
            tab.Controls.Add(new Label { Text = "Peso (kg):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            numPeso = new NumericUpDown { Location = new Point(160, y), Width = 100, DecimalPlaces = 2, Maximum = 300 };
            numPeso.ValueChanged += CalcularIMC;
            tab.Controls.Add(numPeso);

            tab.Controls.Add(new Label { Text = "Talla (m):", Location = new Point(300, y), AutoSize = true, Font = Theme.FontNormal });
            numTalla = new NumericUpDown { Location = new Point(390, y), Width = 100, DecimalPlaces = 2, Maximum = 3 };
            numTalla.ValueChanged += CalcularIMC;
            tab.Controls.Add(numTalla);

            tab.Controls.Add(new Label { Text = "IMC:", Location = new Point(550, y), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor });
            lblIMCVal = new Label { Text = "0.00", Location = new Point(600, y), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor };
            tab.Controls.Add(lblIMCVal);

            y += 60;
            tab.Controls.Add(new Label { Text = "Temp (°C):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            numTemp = new NumericUpDown { Location = new Point(160, y), Width = 100, DecimalPlaces = 1, Maximum = 45 };
            numTemp.ValueChanged += ValidarSignos;
            tab.Controls.Add(numTemp);

            tab.Controls.Add(new Label { Text = "Presión Art.:", Location = new Point(300, y), AutoSize = true, Font = Theme.FontNormal });
            txtPresion = new TextBox { Location = new Point(390, y), Width = 100 };
            tab.Controls.Add(txtPresion);

            y += 60;
            tab.Controls.Add(new Label { Text = "FC (lpm):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            numFC = new NumericUpDown { Location = new Point(160, y), Width = 100, Maximum = 300 };
            numFC.ValueChanged += ValidarSignos;
            tab.Controls.Add(numFC);

            tab.Controls.Add(new Label { Text = "FR (rpm):", Location = new Point(300, y), AutoSize = true, Font = Theme.FontNormal });
            numFR = new NumericUpDown { Location = new Point(390, y), Width = 100, Maximum = 100 };
            numFR.ValueChanged += ValidarSignos;
            tab.Controls.Add(numFR);

            y += 60;
            tab.Controls.Add(new Label { Text = "SpO2 (%):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            numOxigeno = new NumericUpDown { Location = new Point(160, y), Width = 100, Maximum = 100 };
            numOxigeno.ValueChanged += ValidarSignos;
            tab.Controls.Add(numOxigeno);
        }

        private void ValidarSignos(object sender, EventArgs e)
        {
            numTemp.ForeColor = (numTemp.Value > 37.5m || numTemp.Value < 35.0m && numTemp.Value > 0) ? Color.Red : Theme.TextDark;
            numFC.ForeColor = (numFC.Value > 100 || numFC.Value < 60 && numFC.Value > 0) ? Color.Red : Theme.TextDark;
            numFR.ForeColor = (numFR.Value > 20 || numFR.Value < 12 && numFR.Value > 0) ? Color.Red : Theme.TextDark;
            numOxigeno.ForeColor = (numOxigeno.Value < 90 && numOxigeno.Value > 0) ? Color.Red : Theme.TextDark;
        }

        private void CalcularIMC(object sender, EventArgs e)
        {
            if (numTalla.Value > 0 && numPeso.Value > 0)
            {
                var imc = numPeso.Value / (numTalla.Value * numTalla.Value);
                lblIMCVal.Text = Math.Round(imc, 2).ToString();
            }
            else lblIMCVal.Text = "0.00";
        }

        private DataGridView dgvReceta;
        private TextBox txtMedNombre;
        private TextBox txtMedDosis;
        private TextBox txtMedFrecuencia;
        private TextBox txtMedDuracion;
        private NumericUpDown numMedCantidad;
        private TextBox txtIndicacionesGen;
        public MomosClinic.Models.Receta RecetaActual { get; private set; }

        private void BuildSOAPTab(TabPage tab)
        {
            int y = 20;
            
            tab.Controls.Add(new Label { Text = "Motivo de Consulta (S):", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtMotivo = new TextBox { Location = new Point(20, y+25), Width = 400, Height = 100, Multiline = true };
            tab.Controls.Add(txtMotivo);

            tab.Controls.Add(new Label { Text = "Exploración Física (O):", Location = new Point(450, y), AutoSize = true, Font = Theme.FontNormal });
            txtExploracion = new TextBox { Location = new Point(450, y+25), Width = 400, Height = 100, Multiline = true };
            tab.Controls.Add(txtExploracion);

            y += 140;
            tab.Controls.Add(new Label { Text = "Análisis / Evolución (A):", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtAnalisis = new TextBox { Location = new Point(20, y+25), Width = 400, Height = 100, Multiline = true };
            tab.Controls.Add(txtAnalisis);

            tab.Controls.Add(new Label { Text = "Diagnóstico (CIE-10 / Texto):", Location = new Point(450, y), AutoSize = true, Font = Theme.FontNormal });
            txtDiagnostico = new TextBox { Location = new Point(450, y+25), Width = 400, Height = 100, Multiline = true };
            tab.Controls.Add(txtDiagnostico);

            y += 140;
            tab.Controls.Add(new Label { Text = "Plan de Tratamiento (P):", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtPlan = new TextBox { Location = new Point(20, y+25), Width = 830, Height = 100, Multiline = true };
            tab.Controls.Add(txtPlan);
        }

        private void BuildRecetaTab(TabPage tab)
        {
            RecetaActual = new MomosClinic.Models.Receta();

            int y = 20;
            tab.Controls.Add(new Label { Text = "Medicamento / Producto (Puede buscar código de farmacia):", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtMedNombre = new TextBox { Location = new Point(20, y+25), Width = 250, ReadOnly = true };
            tab.Controls.Add(txtMedNombre);

            Button btnBuscarProd = new Button { Text = "🔍", Location = new Point(280, y+24), Width = 40, Height = 32 };
            btnBuscarProd.Click += BtnBuscarProd_Click;
            tab.Controls.Add(btnBuscarProd);

            tab.Controls.Add(new Label { Text = "Dosis:", Location = new Point(340, y), AutoSize = true, Font = Theme.FontNormal });
            txtMedDosis = new TextBox { Location = new Point(340, y+25), Width = 100 };
            tab.Controls.Add(txtMedDosis);

            tab.Controls.Add(new Label { Text = "Frecuencia:", Location = new Point(460, y), AutoSize = true, Font = Theme.FontNormal });
            txtMedFrecuencia = new TextBox { Location = new Point(460, y+25), Width = 120 };
            tab.Controls.Add(txtMedFrecuencia);

            tab.Controls.Add(new Label { Text = "Duración:", Location = new Point(600, y), AutoSize = true, Font = Theme.FontNormal });
            txtMedDuracion = new TextBox { Location = new Point(600, y+25), Width = 100 };
            tab.Controls.Add(txtMedDuracion);

            tab.Controls.Add(new Label { Text = "Cant:", Location = new Point(720, y), AutoSize = true, Font = Theme.FontNormal });
            numMedCantidad = new NumericUpDown { Location = new Point(720, y+25), Width = 60, Minimum = 1, Value = 1 };
            tab.Controls.Add(numMedCantidad);

            Button btnAgregarMed = new Button { Text = "➕", Location = new Point(800, y+25), Width = 50, Height = 30 };
            Theme.StyleButton(btnAgregarMed, Theme.PrimaryColor);
            btnAgregarMed.Click += BtnAgregarMed_Click;
            tab.Controls.Add(btnAgregarMed);

            y += 70;
            dgvReceta = new DataGridView { Location = new Point(20, y), Width = 830, Height = 150 };
            Theme.StyleDataGridView(dgvReceta);
            tab.Controls.Add(dgvReceta);

            y += 170;
            tab.Controls.Add(new Label { Text = "Indicaciones Generales:", Location = new Point(20, y), AutoSize = true, Font = Theme.FontNormal });
            txtIndicacionesGen = new TextBox { Location = new Point(20, y+25), Width = 830, Height = 80, Multiline = true };
            tab.Controls.Add(txtIndicacionesGen);

            ActualizarGridReceta();
        }

        private void BtnBuscarProd_Click(object sender, EventArgs e)
        {
            using (var form = new momospos.Views.BuscadorProductoForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.ProductoSeleccionado != null)
                {
                    txtMedNombre.Text = form.ProductoSeleccionado.Nombre;
                }
            }
        }

        private void BtnBuscarPaciente_Click(object sender, EventArgs e)
        {
            using (var form = new BuscadorPacienteForm(false))
            {
                if (form.ShowDialog() == DialogResult.OK && form.PacienteSeleccionado != null)
                {
                    ConsultaActual.PacienteId = form.PacienteSeleccionado.Id;
                    txtPaciente.Text = form.PacienteSeleccionado.NombreCompleto;
                }
            }
        }

        private void BtnNuevoPaciente_Click(object sender, EventArgs e)
        {
            using (var form = new PacienteForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.PacienteActual != null && form.PacienteActual.Id > 0)
                {
                    ConsultaActual.PacienteId = form.PacienteActual.Id;
                    txtPaciente.Text = form.PacienteActual.NombreCompleto;
                }
            }
        }

        private void BtnAgregarMed_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMedNombre.Text)) return;
            RecetaActual.Detalles.Add(new MomosClinic.Models.RecetaDetalle {
                NombreMedicamento = txtMedNombre.Text,
                Dosis = txtMedDosis.Text,
                Frecuencia = txtMedFrecuencia.Text,
                Duracion = txtMedDuracion.Text,
                Cantidad = (int)numMedCantidad.Value
            });
            
            txtMedNombre.Clear();
            txtMedDosis.Clear();
            txtMedFrecuencia.Clear();
            txtMedDuracion.Clear();
            numMedCantidad.Value = 1;
            
            ActualizarGridReceta();
        }

        private void ActualizarGridReceta()
        {
            dgvReceta.DataSource = null;
            dgvReceta.DataSource = RecetaActual.Detalles;
            if (dgvReceta.Columns.Count > 0)
            {
                dgvReceta.Columns["Id"].Visible = false;
                dgvReceta.Columns["RecetaId"].Visible = false;
                dgvReceta.Columns["ProductoId"].Visible = false;
                dgvReceta.Columns["Instrucciones"].Visible = false;
                dgvReceta.Columns["NombreMedicamento"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (ConsultaActual.PacienteId <= 0)
            {
                MessageBox.Show("Seleccione un paciente.");
                return;
            }
            ConsultaActual.Peso = numPeso.Value > 0 ? numPeso.Value : (decimal?)null;
            ConsultaActual.Talla = numTalla.Value > 0 ? numTalla.Value : (decimal?)null;
            ConsultaActual.Temperatura = numTemp.Value > 0 ? numTemp.Value : (decimal?)null;
            ConsultaActual.PresionArterial = txtPresion.Text.Trim();
            ConsultaActual.FrecuenciaCardiaca = numFC.Value > 0 ? (int)numFC.Value : (int?)null;
            ConsultaActual.FrecuenciaRespiratoria = numFR.Value > 0 ? (int)numFR.Value : (int?)null;
            ConsultaActual.SaturacionOxigeno = numOxigeno.Value > 0 ? (int)numOxigeno.Value : (int?)null;

            ConsultaActual.MotivoConsulta = txtMotivo.Text.Trim();
            ConsultaActual.ExploracionFisica = txtExploracion.Text.Trim();
            ConsultaActual.Analisis = txtAnalisis.Text.Trim();
            ConsultaActual.Diagnostico = txtDiagnostico.Text.Trim();
            ConsultaActual.PlanTratamiento = txtPlan.Text.Trim();

            if (cbServicio.SelectedValue != null && (int)cbServicio.SelectedValue > 0)
            {
                ServicioCobrarId = (int)cbServicio.SelectedValue;
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
