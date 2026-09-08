using System;
using System.Drawing;
using System.Windows.Forms;
using MomosClinic.Models;
using MomosClinic.Repositories;
using momospos.Views;
using System.Linq;
using System.IO;

namespace MomosClinic.Views.Dialogs
{
    public class MedicoForm : Form
    {
        private Medico _medico;
        private MedicoRepository _medicoRepo;
        private EspecialidadMedicaRepository _espRepo;

        private TextBox txtNombre;
        private ComboBox cbxEspecialidad;
        private TextBox txtTelefono;
        private TextBox txtCorreo;
        private CheckBox chkActivo;
        private PictureBox pbFoto;
        private Button btnCargarFoto;
        private string _rutaImagenActual;

        public MedicoForm(Medico medico)
        {
            _medico = medico;
            _medicoRepo = new MedicoRepository();
            _espRepo = new EspecialidadMedicaRepository();
            _rutaImagenActual = medico.RutaImagen;

            BuildUI();
            CargarCatalogos();
            AsignarDatos();
        }

        private void BuildUI()
        {
            this.Text = _medico.Id == 0 ? "Nuevo Médico" : "Editar Médico";
            this.Size = new Size(650, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 200, Padding = new Padding(20) };
            this.Controls.Add(leftPanel);

            pbFoto = new PictureBox
            {
                Size = new Size(150, 150),
                Location = new Point(25, 20),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            leftPanel.Controls.Add(pbFoto);

            btnCargarFoto = new Button
            {
                Text = "📷 Subir Foto",
                Width = 150,
                Height = 35,
                Location = new Point(25, 180)
            };
            Theme.StyleButton(btnCargarFoto, Theme.PrimaryColor, Color.White, Theme.FontNormal);
            btnCargarFoto.Click += BtnCargarFoto_Click;
            leftPanel.Controls.Add(btnCargarFoto);

            Panel rightPanel = new Panel { Location = new Point(200, 0), Size = new Size(450, 450), Padding = new Padding(20) };
            this.Controls.Add(rightPanel);

            int y = 20;
            int startX = 20;

            rightPanel.Controls.Add(new Label { Text = "Nombre Completo *", Location = new Point(startX, y), AutoSize = true, Font = Theme.FontNormal });
            txtNombre = new TextBox { Location = new Point(startX, y + 25), Width = 370, Font = new Font("Segoe UI", 12) };
            rightPanel.Controls.Add(txtNombre);
            y += 65;

            rightPanel.Controls.Add(new Label { Text = "Especialidad *", Location = new Point(startX, y), AutoSize = true, Font = Theme.FontNormal });
            cbxEspecialidad = new ComboBox { Location = new Point(startX, y + 25), Width = 370, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            rightPanel.Controls.Add(cbxEspecialidad);
            y += 65;

            rightPanel.Controls.Add(new Label { Text = "Teléfono", Location = new Point(startX, y), AutoSize = true, Font = Theme.FontNormal });
            txtTelefono = new TextBox { Location = new Point(startX, y + 25), Width = 175, Font = new Font("Segoe UI", 12) };
            rightPanel.Controls.Add(txtTelefono);

            rightPanel.Controls.Add(new Label { Text = "Correo Electrónico", Location = new Point(startX + 195, y), AutoSize = true, Font = Theme.FontNormal });
            txtCorreo = new TextBox { Location = new Point(startX + 195, y + 25), Width = 175, Font = new Font("Segoe UI", 12) };
            rightPanel.Controls.Add(txtCorreo);
            y += 65;

            chkActivo = new CheckBox { Text = "Médico Activo en el Sistema", Location = new Point(startX, y), AutoSize = true, Font = Theme.FontNormal };
            rightPanel.Controls.Add(chkActivo);
            y += 60;

            Button btnGuardar = new Button { Text = "💾 Guardar", Width = 150, Height = 40, Location = new Point(startX, y) };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Color.White, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;
            rightPanel.Controls.Add(btnGuardar);

            Button btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 40, Location = new Point(startX + 160, y) };
            Theme.StyleButton(btnCancelar, Color.Gray, Color.White, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            rightPanel.Controls.Add(btnCancelar);
        }

        private void CargarCatalogos()
        {
            var especialidades = _espRepo.ObtenerTodas().ToList();
            cbxEspecialidad.DataSource = especialidades;
            cbxEspecialidad.DisplayMember = "Nombre";
            cbxEspecialidad.ValueMember = "Id";
        }

        private void AsignarDatos()
        {
            txtNombre.Text = _medico.NombreCompleto;
            txtTelefono.Text = _medico.Telefono;
            txtCorreo.Text = _medico.Correo;
            chkActivo.Checked = _medico.Activo;
            
            if (_medico.EspecialidadId.HasValue)
                cbxEspecialidad.SelectedValue = _medico.EspecialidadId.Value;

            if (!string.IsNullOrEmpty(_rutaImagenActual) && File.Exists(_rutaImagenActual))
            {
                try {
                    pbFoto.Image = MomosClinic.Helpers.ImageHelper.LoadImageWithoutLock(_rutaImagenActual);
                } catch { }
            }
        }

        private void BtnCargarFoto_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Copiar imagen a carpeta del sistema
                        string uploadsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", "Medicos");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        
                        string ext = Path.GetExtension(ofd.FileName);
                        string newFileName = $"medico_{Guid.NewGuid()}{ext}";
                        string targetPath = Path.Combine(uploadsFolder, newFileName);
                        
                        File.Copy(ofd.FileName, targetPath, true);
                        _rutaImagenActual = targetPath;
                        
                        pbFoto.Image = MomosClinic.Helpers.ImageHelper.LoadImageWithoutLock(targetPath);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Error al cargar la imagen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomMessageBox.Show("El nombre del médico es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbxEspecialidad.SelectedValue == null)
            {
                CustomMessageBox.Show("Debe seleccionar una especialidad.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _medico.NombreCompleto = txtNombre.Text.Trim();
            _medico.EspecialidadId = (int)cbxEspecialidad.SelectedValue;
            _medico.Telefono = txtTelefono.Text.Trim();
            _medico.Correo = txtCorreo.Text.Trim();
            _medico.Activo = chkActivo.Checked;
            _medico.RutaImagen = _rutaImagenActual;

            if (_medico.Id == 0)
                _medicoRepo.Insertar(_medico);
            else
                _medicoRepo.Actualizar(_medico);

            this.DialogResult = DialogResult.OK;
        }
    }
}
