using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using momospos.Views;

namespace MomosClinic.Views
{
    public class PacientesView : UserControl
    {
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Button btnNuevo;
        private Button btnGuardar;
        
        // Form Controls
        private TextBox txtNombre;
        private DateTimePicker dtpFechaNac;
        private ComboBox cbGenero;
        private TextBox txtTelefono;
        private TextBox txtEmail;
        private TextBox txtDireccion;
        private ComboBox cbTipoSangre;
        private TextBox txtAlergias;
        private TextBox txtAntecedentesFam;
        private TextBox txtAntecedentesPat;
        private ComboBox cbEstado;
        private TextBox txtMotivoBaja;
        private Label lblMotivoBaja;

        private PacienteRepository _repo;
        private string _usuarioActual;
        
        public Paciente PacienteActual { get; private set; }

        public PacientesView(string usuarioActual = "Sistema")
        {
            _repo = new PacienteRepository();
            _usuarioActual = usuarioActual;
            BuildUI();
            LimpiarFormulario();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            // Panel Superior (Buscador y Acciones)
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };
            
            Label lblTitulo = new Label { Text = "👥 Gestión de Paciente", Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 25), ForeColor = Theme.TextDark };
            
            txtBuscar = new TextBox { Location = new Point(300, 27), Width = 250, Font = Theme.FontNormal };
            btnBuscar = new Button { Text = "🔍 Buscar", Location = new Point(560, 25), Width = 100, Height = 35 };
            Theme.StyleButton(btnBuscar, Theme.SecondaryColor);
            btnBuscar.Click += BtnBuscar_Click;
            txtBuscar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { btnBuscar.PerformClick(); e.SuppressKeyPress = true; } };

            btnNuevo = new Button { Text = "🧹 Limpiar / Nuevo", Location = new Point(670, 25), Width = 160, Height = 35 };
            Theme.StyleButton(btnNuevo, Theme.WarningColor);
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            btnGuardar = new Button { Text = "💾 Guardar", Location = new Point(840, 25), Width = 120, Height = 35 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor);
            btnGuardar.Click += BtnGuardar_Click;

            topPanel.Controls.Add(lblTitulo);
            topPanel.Controls.Add(txtBuscar);
            topPanel.Controls.Add(btnBuscar);
            topPanel.Controls.Add(btnNuevo);
            topPanel.Controls.Add(btnGuardar);

            // Panel Formulario
            Panel formPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            int y = 20;
            // Estado
            formPanel.Controls.Add(new Label { Text = "Estado del Paciente:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontSubtitle });
            cbEstado = new ComboBox { Location = new Point(220, y), Width = 150, Font = Theme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList };
            cbEstado.Items.AddRange(new[] { "Activo", "Inactivo (Baja)" });
            cbEstado.SelectedIndexChanged += CbEstado_SelectedIndexChanged;
            formPanel.Controls.Add(cbEstado);

            lblMotivoBaja = new Label { Text = "Motivo de Baja:", Location = new Point(400, y), AutoSize = true, Font = Theme.FontNormal, Visible = false };
            txtMotivoBaja = new TextBox { Location = new Point(530, y), Width = 400, Font = Theme.FontNormal, Visible = false };
            formPanel.Controls.Add(lblMotivoBaja);
            formPanel.Controls.Add(txtMotivoBaja);
            y += 60;

            // Datos Personales (Left Col)
            int yLeft = y;
            formPanel.Controls.Add(new Label { Text = "Datos Personales", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor });
            yLeft += 40;

            formPanel.Controls.Add(new Label { Text = "Nombre Completo:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtNombre = new TextBox { Location = new Point(30, yLeft + 25), Width = 350, Font = new Font("Segoe UI", 12) };
            formPanel.Controls.Add(txtNombre);
            yLeft += 70;

            formPanel.Controls.Add(new Label { Text = "Fecha Nacimiento:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            dtpFechaNac = new DateTimePicker { Location = new Point(30, yLeft + 25), Width = 160, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Short };
            formPanel.Controls.Add(dtpFechaNac);
            
            formPanel.Controls.Add(new Label { Text = "Género:", Location = new Point(210, yLeft), AutoSize = true, Font = Theme.FontNormal });
            cbGenero = new ComboBox { Location = new Point(210, yLeft + 25), Width = 170, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGenero.Items.AddRange(new[] { "Masculino", "Femenino", "Otro" });
            formPanel.Controls.Add(cbGenero);
            yLeft += 70;

            formPanel.Controls.Add(new Label { Text = "Teléfono:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtTelefono = new TextBox { Location = new Point(30, yLeft + 25), Width = 160, Font = new Font("Segoe UI", 12), MaxLength = 10 };
            txtTelefono.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            formPanel.Controls.Add(txtTelefono);
            
            formPanel.Controls.Add(new Label { Text = "Email:", Location = new Point(210, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtEmail = new TextBox { Location = new Point(210, yLeft + 25), Width = 170, Font = new Font("Segoe UI", 12) };
            formPanel.Controls.Add(txtEmail);
            yLeft += 70;

            formPanel.Controls.Add(new Label { Text = "Dirección:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtDireccion = new TextBox { Location = new Point(30, yLeft + 25), Width = 350, Height = 60, Multiline = true, Font = new Font("Segoe UI", 12) };
            formPanel.Controls.Add(txtDireccion);
            yLeft += 100;

            formPanel.Controls.Add(new Label { Text = "Tipo de Sangre:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            cbTipoSangre = new ComboBox { Location = new Point(30, yLeft + 25), Width = 160, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipoSangre.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Desconocido" });
            formPanel.Controls.Add(cbTipoSangre);

            // Datos Médicos (Right Col)
            int yRight = y;
            formPanel.Controls.Add(new Label { Text = "Datos Médicos", Location = new Point(450, yRight), AutoSize = true, Font = Theme.FontSubtitle, ForeColor = Theme.PrimaryColor });
            yRight += 40;

            formPanel.Controls.Add(new Label { Text = "Alergias:", Location = new Point(450, yRight), AutoSize = true, Font = Theme.FontNormal });
            txtAlergias = new TextBox { Location = new Point(450, yRight + 25), Width = 450, Height = 60, Multiline = true, Font = new Font("Segoe UI", 12) };
            formPanel.Controls.Add(txtAlergias);
            yRight += 100;

            formPanel.Controls.Add(new Label { Text = "Antecedentes Familiares:", Location = new Point(450, yRight), AutoSize = true, Font = Theme.FontNormal });
            txtAntecedentesFam = new TextBox { Location = new Point(450, yRight + 25), Width = 450, Height = 80, Multiline = true, Font = new Font("Segoe UI", 12) };
            formPanel.Controls.Add(txtAntecedentesFam);
            yRight += 120;

            formPanel.Controls.Add(new Label { Text = "Antecedentes Patológicos:", Location = new Point(450, yRight), AutoSize = true, Font = Theme.FontNormal });
            txtAntecedentesPat = new TextBox { Location = new Point(450, yRight + 25), Width = 450, Height = 80, Multiline = true, Font = new Font("Segoe UI", 12) };
            formPanel.Controls.Add(txtAntecedentesPat);

            this.Controls.Add(formPanel);
            this.Controls.Add(topPanel);
            formPanel.BringToFront();
        }

        private void CbEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esInactivo = cbEstado.SelectedIndex == 1;
            lblMotivoBaja.Visible = esInactivo;
            txtMotivoBaja.Visible = esInactivo;
        }

        private void LimpiarFormulario()
        {
            PacienteActual = new Paciente { FechaNacimiento = DateTime.Today.AddYears(-30), Activo = true };
            CargarDatosFormulario();
            txtBuscar.Clear();
            txtBuscar.Focus();
        }

        private void CargarDatosFormulario()
        {
            txtNombre.Text = PacienteActual.NombreCompleto;
            if (PacienteActual.FechaNacimiento.HasValue && PacienteActual.FechaNacimiento > dtpFechaNac.MinDate) 
                dtpFechaNac.Value = PacienteActual.FechaNacimiento.Value;
            cbGenero.SelectedItem = PacienteActual.Genero;
            txtTelefono.Text = PacienteActual.Telefono;
            txtEmail.Text = PacienteActual.Email;
            txtDireccion.Text = PacienteActual.Direccion;
            cbTipoSangre.SelectedItem = PacienteActual.TipoSangre;
            txtAlergias.Text = PacienteActual.Alergias;
            txtAntecedentesFam.Text = PacienteActual.AntecedentesFamiliares;
            txtAntecedentesPat.Text = PacienteActual.AntecedentesPatologicos;
            cbEstado.SelectedIndex = PacienteActual.Activo ? 0 : 1;
            txtMotivoBaja.Text = PacienteActual.MotivoBaja;
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            // true para mostrar inactivos en el modulo de Expediente/Pacientes
            var formBuscador = new MomosClinic.Views.Dialogs.BuscadorPacienteForm(true);
            
            // Si el txtBuscar tiene algo, lo pasamos (esto requeriría cambiar BuscadorPacienteForm para recibir texto inicial, 
            // pero podemos dejar que el usuario lo escriba)
            if (formBuscador.ShowDialog() == DialogResult.OK && formBuscador.PacienteSeleccionado != null)
            {
                PacienteActual = formBuscador.PacienteSeleccionado;
                CargarDatosFormulario();
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomMessageBox.Show("El nombre es requerido.", "Error de Validación");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtTelefono.Text) && txtTelefono.Text.Trim().Length != 10)
            {
                CustomMessageBox.Show("El teléfono debe tener exactamente 10 dígitos.", "Error de Validación");
                return;
            }

            if (cbEstado.SelectedIndex == 1 && string.IsNullOrWhiteSpace(txtMotivoBaja.Text))
            {
                CustomMessageBox.Show("Debe ingresar un motivo de baja.", "Validación");
                return;
            }

            if (_repo.ExistePacienteDuplicado(txtNombre.Text.Trim(), txtTelefono.Text.Trim(), PacienteActual.Id))
            {
                CustomMessageBox.Show("Ya existe un paciente registrado con ese mismo nombre y teléfono.", "Paciente Duplicado");
                return;
            }

            PacienteActual.NombreCompleto = txtNombre.Text.Trim();
            PacienteActual.FechaNacimiento = dtpFechaNac.Value.Date;
            PacienteActual.Genero = cbGenero.SelectedItem?.ToString();
            PacienteActual.Telefono = txtTelefono.Text.Trim();
            PacienteActual.Email = txtEmail.Text.Trim();
            PacienteActual.Direccion = txtDireccion.Text.Trim();
            PacienteActual.TipoSangre = cbTipoSangre.SelectedItem?.ToString();
            PacienteActual.Alergias = txtAlergias.Text.Trim();
            PacienteActual.AntecedentesFamiliares = txtAntecedentesFam.Text.Trim();
            PacienteActual.AntecedentesPatologicos = txtAntecedentesPat.Text.Trim();
            PacienteActual.Activo = cbEstado.SelectedIndex == 0;
            PacienteActual.MotivoBaja = PacienteActual.Activo ? null : txtMotivoBaja.Text.Trim();
            
            if (!PacienteActual.Activo && string.IsNullOrEmpty(PacienteActual.BajaPor))
            {
                PacienteActual.BajaPor = _usuarioActual; // Quién lo dio de baja por primera vez
            }

            bool esNuevo = PacienteActual.Id == 0;

            if (esNuevo)
            {
                PacienteActual.CreadoPor = _usuarioActual;
                _repo.Insertar(PacienteActual);
                CustomMessageBox.Show("Paciente registrado correctamente.", "Éxito");
                LimpiarFormulario(); // Resetear para el siguiente
            }
            else
            {
                PacienteActual.ModificadoPor = _usuarioActual;
                _repo.Actualizar(PacienteActual);
                CustomMessageBox.Show("Paciente actualizado correctamente.", "Éxito");
            }
        }
    }
}
