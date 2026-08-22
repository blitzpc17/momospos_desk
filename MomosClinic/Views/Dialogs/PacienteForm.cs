using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class PacienteForm : Form
    {
        public Paciente PacienteActual { get; private set; }
        private bool _esEdicion;

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

        public PacienteForm(Paciente paciente = null)
        {
            _esEdicion = paciente != null;
            PacienteActual = paciente ?? new Paciente { FechaNacimiento = DateTime.Today.AddYears(-30) };
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Text = _esEdicion ? "Editar Paciente" : "Nuevo Paciente";
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = this.Text, Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            // Datos Personales (Left Col)
            int yLeft = 80;
            this.Controls.Add(new Label { Text = "Nombre Completo:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtNombre = new TextBox { Location = new Point(30, yLeft + 25), Width = 300, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtNombre);
            yLeft += 70;

            this.Controls.Add(new Label { Text = "Fecha Nacimiento:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            dtpFechaNac = new DateTimePicker { Location = new Point(30, yLeft + 25), Width = 150, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpFechaNac);
            
            this.Controls.Add(new Label { Text = "Género:", Location = new Point(200, yLeft), AutoSize = true, Font = Theme.FontNormal });
            cbGenero = new ComboBox { Location = new Point(200, yLeft + 25), Width = 130, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGenero.Items.AddRange(new[] { "Masculino", "Femenino", "Otro" });
            this.Controls.Add(cbGenero);
            yLeft += 70;

            this.Controls.Add(new Label { Text = "Teléfono:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtTelefono = new TextBox { Location = new Point(30, yLeft + 25), Width = 150, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtTelefono);
            
            this.Controls.Add(new Label { Text = "Email:", Location = new Point(200, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtEmail = new TextBox { Location = new Point(200, yLeft + 25), Width = 130, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtEmail);
            yLeft += 70;

            this.Controls.Add(new Label { Text = "Dirección:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            txtDireccion = new TextBox { Location = new Point(30, yLeft + 25), Width = 300, Height = 60, Multiline = true, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtDireccion);
            yLeft += 100;

            this.Controls.Add(new Label { Text = "Tipo de Sangre:", Location = new Point(30, yLeft), AutoSize = true, Font = Theme.FontNormal });
            cbTipoSangre = new ComboBox { Location = new Point(30, yLeft + 25), Width = 150, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipoSangre.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Desconocido" });
            this.Controls.Add(cbTipoSangre);

            // Datos Médicos (Right Col)
            int yRight = 80;
            this.Controls.Add(new Label { Text = "Alergias:", Location = new Point(400, yRight), AutoSize = true, Font = Theme.FontNormal });
            txtAlergias = new TextBox { Location = new Point(400, yRight + 25), Width = 350, Height = 60, Multiline = true, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtAlergias);
            yRight += 100;

            this.Controls.Add(new Label { Text = "Antecedentes Familiares:", Location = new Point(400, yRight), AutoSize = true, Font = Theme.FontNormal });
            txtAntecedentesFam = new TextBox { Location = new Point(400, yRight + 25), Width = 350, Height = 80, Multiline = true, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtAntecedentesFam);
            yRight += 120;

            this.Controls.Add(new Label { Text = "Antecedentes Patológicos:", Location = new Point(400, yRight), AutoSize = true, Font = Theme.FontNormal });
            txtAntecedentesPat = new TextBox { Location = new Point(400, yRight + 25), Width = 350, Height = 80, Multiline = true, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtAntecedentesPat);

            // Buttons
            Button btnGuardar = new Button { Text = "💾 Guardar", Location = new Point(400, 520), Width = 160, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(590, 520), Width = 160, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void CargarDatos()
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
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es requerido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            this.DialogResult = DialogResult.OK;
        }
    }
}
