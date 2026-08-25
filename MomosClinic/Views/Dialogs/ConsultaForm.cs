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
            this.Size = new Size(1050, 780);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Expediente de Consulta", Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            // Paciente Selector
            Panel patientPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(20) };
            patientPanel.Controls.Add(new Label { Text = "Paciente:", Location = new Point(20, 22), AutoSize = true, Font = Theme.FontSubtitle });
            
            txtPaciente = new TextBox { Location = new Point(120, 20), Width = 320, Font = new Font("Segoe UI", 12), ReadOnly = true };
            if (ConsultaActual.PacienteId > 0)
            {
                var p = _pacienteRepo.ObtenerPorId(ConsultaActual.PacienteId);
                if (p != null) txtPaciente.Text = p.NombreCompleto;
            }
            
            btnBuscarPaciente = new Button { Text = "🔍", Location = new Point(450, 18), Width = 40, Height = 32 };
            btnBuscarPaciente.Click += BtnBuscarPaciente_Click;
            btnNuevoPaciente = new Button { Text = "➕", Location = new Point(500, 18), Width = 40, Height = 32 };
            btnNuevoPaciente.Click += BtnNuevoPaciente_Click;

            patientPanel.Controls.Add(txtPaciente);
            patientPanel.Controls.Add(btnBuscarPaciente);
            patientPanel.Controls.Add(btnNuevoPaciente);

            patientPanel.Controls.Add(new Label { Text = "Servicio (Cobro):", Location = new Point(580, 22), AutoSize = true, Font = Theme.FontSubtitle });
            cbServicio = new ComboBox { Location = new Point(740, 20), Width = 260, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
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
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            
            // Un pequeño separador
            Panel sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            bottomPanel.Controls.Add(sep);

            Button btnGuardar = new Button { Text = "💾 Terminar Consulta", Location = new Point(630, 20), Width = 220, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(870, 20), Width = 140, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            bottomPanel.Controls.Add(btnGuardar);
            bottomPanel.Controls.Add(btnCancelar);
            this.Controls.Add(bottomPanel);
        }

        private void BuildSignosTab(TabPage tab)
        {
            int y = 40;
            Label lblHeader = new Label { Text = "Registro de Signos Vitales", Font = Theme.FontTitle, ForeColor = Theme.PrimaryColor, AutoSize = true, Location = new Point(40, 15) };
            tab.Controls.Add(lblHeader);

            y += 60;
            tab.Controls.Add(new Label { Text = "⚖️ Peso (kg):", Location = new Point(50, y), AutoSize = true, Font = Theme.FontNormal });
            numPeso = new NumericUpDown { Location = new Point(190, y-2), Width = 120, DecimalPlaces = 2, Maximum = 300, Font = Theme.FontNormal };
            numPeso.ValueChanged += CalcularIMC;
            tab.Controls.Add(numPeso);

            tab.Controls.Add(new Label { Text = "📏 Talla (m):", Location = new Point(370, y), AutoSize = true, Font = Theme.FontNormal });
            numTalla = new NumericUpDown { Location = new Point(490, y-2), Width = 120, DecimalPlaces = 2, Maximum = 3, Font = Theme.FontNormal };
            numTalla.ValueChanged += CalcularIMC;
            tab.Controls.Add(numTalla);

            tab.Controls.Add(new Label { Text = "📊 IMC:", Location = new Point(710, y), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.TextDark });
            lblIMCVal = new Label { Text = "0.00", Location = new Point(810, y), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor };
            tab.Controls.Add(lblIMCVal);

            y += 80;
            tab.Controls.Add(new Label { Text = "🌡️ Temp (°C):", Location = new Point(50, y), AutoSize = true, Font = Theme.FontNormal });
            numTemp = new NumericUpDown { Location = new Point(190, y-2), Width = 120, DecimalPlaces = 1, Maximum = 45, Font = Theme.FontNormal };
            numTemp.ValueChanged += ValidarSignos;
            tab.Controls.Add(numTemp);

            tab.Controls.Add(new Label { Text = "🩺 Presión Art.:", Location = new Point(370, y), AutoSize = true, Font = Theme.FontNormal });
            txtPresion = new TextBox { Location = new Point(490, y-2), Width = 120, Font = Theme.FontNormal };
            tab.Controls.Add(txtPresion);

            y += 80;
            tab.Controls.Add(new Label { Text = "❤️ FC (lpm):", Location = new Point(50, y), AutoSize = true, Font = Theme.FontNormal });
            numFC = new NumericUpDown { Location = new Point(190, y-2), Width = 120, Maximum = 300, Font = Theme.FontNormal };
            numFC.ValueChanged += ValidarSignos;
            tab.Controls.Add(numFC);

            tab.Controls.Add(new Label { Text = "🫁 FR (rpm):", Location = new Point(370, y), AutoSize = true, Font = Theme.FontNormal });
            numFR = new NumericUpDown { Location = new Point(490, y-2), Width = 120, Maximum = 100, Font = Theme.FontNormal };
            numFR.ValueChanged += ValidarSignos;
            tab.Controls.Add(numFR);

            y += 80;
            tab.Controls.Add(new Label { Text = "💨 SpO2 (%):", Location = new Point(50, y), AutoSize = true, Font = Theme.FontNormal });
            numOxigeno = new NumericUpDown { Location = new Point(190, y-2), Width = 120, Maximum = 100, Font = Theme.FontNormal };
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
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 6, ColumnCount = 2 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            var lblMotivo = new Label { Text = "Motivo de Consulta (Subjetivo):", Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor, Dock = DockStyle.Bottom, AutoSize = true };
            txtMotivo = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = Theme.FontNormal, ScrollBars = ScrollBars.Vertical, Margin = new Padding(0, 5, 15, 15) };
            
            var lblExploracion = new Label { Text = "Exploración Física (Objetivo):", Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor, Dock = DockStyle.Bottom, AutoSize = true };
            txtExploracion = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = Theme.FontNormal, ScrollBars = ScrollBars.Vertical, Margin = new Padding(15, 5, 0, 15) };

            var lblAnalisis = new Label { Text = "Análisis / Evolución (Assessment):", Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor, Dock = DockStyle.Bottom, AutoSize = true };
            txtAnalisis = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = Theme.FontNormal, ScrollBars = ScrollBars.Vertical, Margin = new Padding(0, 5, 15, 15) };

            var lblDiagnostico = new Label { Text = "Diagnóstico (CIE-10 / Texto):", Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor, Dock = DockStyle.Bottom, AutoSize = true };
            txtDiagnostico = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = Theme.FontNormal, ScrollBars = ScrollBars.Vertical, Margin = new Padding(15, 5, 0, 15) };

            var lblPlan = new Label { Text = "Plan de Tratamiento (Plan):", Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor, Dock = DockStyle.Bottom, AutoSize = true };
            txtPlan = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = Theme.FontNormal, ScrollBars = ScrollBars.Vertical, Margin = new Padding(0, 5, 0, 0) };

            tlp.Controls.Add(lblMotivo, 0, 0);
            tlp.Controls.Add(txtMotivo, 0, 1);
            tlp.Controls.Add(lblExploracion, 1, 0);
            tlp.Controls.Add(txtExploracion, 1, 1);
            
            tlp.Controls.Add(lblAnalisis, 0, 2);
            tlp.Controls.Add(txtAnalisis, 0, 3);
            tlp.Controls.Add(lblDiagnostico, 1, 2);
            tlp.Controls.Add(txtDiagnostico, 1, 3);

            tlp.Controls.Add(lblPlan, 0, 4);
            tlp.SetColumnSpan(lblPlan, 2);
            tlp.Controls.Add(txtPlan, 0, 5);
            tlp.SetColumnSpan(txtPlan, 2);

            tab.Controls.Add(tlp);
        }

        private void BuildRecetaTab(TabPage tab)
        {
            RecetaActual = new MomosClinic.Models.Receta();

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            
            int yLbl = 10;
            int yTxt = 35;
            
            topPanel.Controls.Add(new Label { Text = "Medicamento / Producto:", Location = new Point(20, yLbl), AutoSize = true, Font = Theme.FontNormal });
            txtMedNombre = new TextBox { Location = new Point(20, yTxt), Width = 200, Font = Theme.FontNormal, ReadOnly = true };
            topPanel.Controls.Add(txtMedNombre);

            Button btnBuscarProd = new Button { Text = "🔍", Location = new Point(225, yTxt-1), Width = 40, Height = 32 };
            btnBuscarProd.Click += BtnBuscarProd_Click;
            topPanel.Controls.Add(btnBuscarProd);

            topPanel.Controls.Add(new Label { Text = "Dosis:", Location = new Point(290, yLbl), AutoSize = true, Font = Theme.FontNormal });
            txtMedDosis = new TextBox { Location = new Point(290, yTxt), Width = 110, Font = Theme.FontNormal };
            topPanel.Controls.Add(txtMedDosis);

            topPanel.Controls.Add(new Label { Text = "Frecuencia:", Location = new Point(420, yLbl), AutoSize = true, Font = Theme.FontNormal });
            txtMedFrecuencia = new TextBox { Location = new Point(420, yTxt), Width = 110, Font = Theme.FontNormal };
            topPanel.Controls.Add(txtMedFrecuencia);

            topPanel.Controls.Add(new Label { Text = "Duración:", Location = new Point(550, yLbl), AutoSize = true, Font = Theme.FontNormal });
            txtMedDuracion = new TextBox { Location = new Point(550, yTxt), Width = 100, Font = Theme.FontNormal };
            topPanel.Controls.Add(txtMedDuracion);

            topPanel.Controls.Add(new Label { Text = "Cant:", Location = new Point(670, yLbl), AutoSize = true, Font = Theme.FontNormal });
            numMedCantidad = new NumericUpDown { Location = new Point(670, yTxt), Width = 60, Minimum = 1, Value = 1, Font = Theme.FontNormal };
            topPanel.Controls.Add(numMedCantidad);

            Button btnAgregarMed = new Button { Text = "➕ Añadir", Location = new Point(750, yTxt-2), Width = 100, Height = 34 };
            Theme.StyleButton(btnAgregarMed, Theme.PrimaryColor);
            btnAgregarMed.Click += BtnAgregarMed_Click;
            topPanel.Controls.Add(btnAgregarMed);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(20) };
            Label lInd = new Label { Text = "Indicaciones Generales para el Paciente:", AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor, Dock = DockStyle.Top };
            txtIndicacionesGen = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = Theme.FontNormal, ScrollBars = ScrollBars.Vertical, Margin = new Padding(0, 5, 0, 0) };
            bottomPanel.Controls.Add(txtIndicacionesGen);
            bottomPanel.Controls.Add(lInd);

            Panel gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            dgvReceta = new DataGridView { Dock = DockStyle.Fill };
            Theme.StyleDataGridView(dgvReceta);
            gridPanel.Controls.Add(dgvReceta);

            tab.Controls.Add(gridPanel);
            tab.Controls.Add(bottomPanel);
            tab.Controls.Add(topPanel);

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
