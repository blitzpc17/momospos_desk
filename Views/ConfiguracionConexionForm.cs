using System;
using System.Windows.Forms;
using momospos.Helpers;

namespace momospos.Views
{
    public partial class ConfiguracionConexionForm : Form
    {
        public ConfiguracionConexionForm()
        {
            InitializeComponent();
            CargarDatosActuales();
        }

        private void CargarDatosActuales()
        {
            string cadena = ConfiguracionHelper.ObtenerCadenaConexion();
            if (ConfiguracionHelper.AnalizarCadena(cadena, out string host, out string port, out string db, out string user, out string pass))
            {
                txtServidor.Text = host;
                txtPuerto.Text = port;
                txtBaseDatos.Text = db;
                txtUsuario.Text = user;
                txtContrasena.Text = pass;
            }
            txtCajaId.Text = ConfiguracionHelper.ObtenerCajaLocalId().ToString();
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                bool exito = ConfiguracionHelper.ProbarConexion(
                    txtServidor.Text.Trim(),
                    txtPuerto.Text.Trim(),
                    txtBaseDatos.Text.Trim(),
                    txtUsuario.Text.Trim(),
                    txtContrasena.Text.Trim()
                );

                if (exito)
                {
                    momospos.Views.CustomMessageBox.Show("Conexión exitosa al servidor.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    momospos.Views.CustomMessageBox.Show("No se pudo conectar al servidor de base de datos. Verifique los datos e intente de nuevo.", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                bool exito = ConfiguracionHelper.ProbarConexion(
                    txtServidor.Text.Trim(),
                    txtPuerto.Text.Trim(),
                    txtBaseDatos.Text.Trim(),
                    txtUsuario.Text.Trim(),
                    txtContrasena.Text.Trim()
                );

                if (!exito)
                {
                    var result = momospos.Views.CustomMessageBox.Show("La conexión falló. ¿Desea guardar de todos modos?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                ConfiguracionHelper.GuardarCadenaConexion(
                    txtServidor.Text.Trim(),
                    txtPuerto.Text.Trim(),
                    txtBaseDatos.Text.Trim(),
                    txtUsuario.Text.Trim(),
                    txtContrasena.Text.Trim()
                );
                
                if (int.TryParse(txtCajaId.Text.Trim(), out int cajaId))
                {
                    ConfiguracionHelper.GuardarCajaLocalId(cajaId);
                }

                momospos.Views.CustomMessageBox.Show("Configuración guardada correctamente. La aplicación debe reiniciarse para aplicar los cambios si ya estaba iniciada, o continuará normalmente si acaba de iniciar.", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                momospos.Views.CustomMessageBox.Show("Error al guardar la configuración: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
