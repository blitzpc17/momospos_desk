using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using momospos.Views;

namespace MomosClinic.Views.Dialogs
{
    public class PacienteExpressForm : Form
    {
        public Paciente PacienteActual { get; private set; }

        private TextBox txtNombre;
        private DateTimePicker dtpFechaNac;
        private ComboBox cbGenero;
        private TextBox txtTelefono;

        public PacienteExpressForm()
        {
            PacienteActual = new Paciente { FechaNacimiento = DateTime.Today.AddYears(-30) };
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Registro Rápido de Paciente";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = "Registro Express", Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            int y = 80;

            this.Controls.Add(new Label { Text = "Nombre Completo (Obligatorio):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            txtNombre = new TextBox { Location = new Point(30, y + 25), Width = 420, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtNombre);
            y += 70;

            this.Controls.Add(new Label { Text = "Fecha de Nacimiento:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            dtpFechaNac = new DateTimePicker { Location = new Point(30, y + 25), Width = 200, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Short };
            dtpFechaNac.Value = PacienteActual.FechaNacimiento.Value;
            this.Controls.Add(dtpFechaNac);
            y += 70;

            this.Controls.Add(new Label { Text = "Género (Opcional):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            cbGenero = new ComboBox { Location = new Point(30, y + 25), Width = 200, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGenero.Items.AddRange(new[] { "Masculino", "Femenino", "Otro" });
            this.Controls.Add(cbGenero);

            this.Controls.Add(new Label { Text = "Teléfono (Opcional):", Location = new Point(250, y), AutoSize = true, Font = Theme.FontNormal });
            txtTelefono = new TextBox { Location = new Point(250, y + 25), Width = 200, Font = new Font("Segoe UI", 12), MaxLength = 10 };
            txtTelefono.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            this.Controls.Add(txtTelefono);
            y += 90;

            Button btnGuardar = new Button { Text = "💾 Guardar Express", Location = new Point(80, y), Width = 170, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(260, y), Width = 150, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomMessageBox.Show("El nombre es requerido.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtTelefono.Text) && txtTelefono.Text.Trim().Length != 10)
            {
                CustomMessageBox.Show("El teléfono debe tener exactamente 10 dígitos.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var repo = new MomosClinic.Repositories.PacienteRepository();
            
            if (repo.ExistePacienteDuplicado(txtNombre.Text.Trim(), txtTelefono.Text.Trim(), 0))
            {
                CustomMessageBox.Show("Ya existe un paciente registrado con ese mismo nombre y teléfono.", "Paciente Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PacienteActual.NombreCompleto = txtNombre.Text.Trim();
            PacienteActual.FechaNacimiento = dtpFechaNac.Value.Date;
            PacienteActual.Genero = cbGenero.SelectedItem?.ToString();
            PacienteActual.Telefono = txtTelefono.Text.Trim();
            
            // Valores por defecto para campos vacíos en quick add
            PacienteActual.Email = "";
            PacienteActual.Direccion = "";
            PacienteActual.TipoSangre = "";
            PacienteActual.Alergias = "";
            PacienteActual.AntecedentesFamiliares = "";
            PacienteActual.AntecedentesPatologicos = "";
            PacienteActual.Activo = true;

            try 
            {
                PacienteActual.CreadoPor = "Sistema";
                repo.Insertar(PacienteActual);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
