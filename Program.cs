using System;
using System.Windows.Forms;
using momospos.Views;
using AutoUpdaterDotNET;

namespace momospos
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Configurar manejador global de excepciones
            Application.ThreadException += GlobalExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            
            // Habilitar TLS 1.2 para descargar desde GitHub sin errores
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

            // Buscar actualizaciones automáticamente
            AutoUpdater.Start("https://raw.githubusercontent.com/blitzpc17/momospos_desk/master/update.xml");
            
            // 0. Probar conexión
            if (!momospos.Helpers.ConfiguracionHelper.ProbarConexionActual())
            {
                var configForm = new momospos.Views.ConfiguracionConexionForm();
                if (configForm.ShowDialog() != DialogResult.OK)
                {
                    return; // Si el usuario cancela la configuración, se cierra
                }
            }

            // Ejecutar migraciones / actualizaciones de DB
            momospos.Helpers.ConfiguracionHelper.EjecutarActualizacionDeEsquema();
            
            // 1. Mostrar Login y manejar ciclo de turnos continuos
            while (true)
            {
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var usuario = loginForm.UsuarioAutenticado;
                    
                    // 2. Verificar si hay caja abierta para esta máquina física
                    int cajaLocalId = momospos.Helpers.ConfiguracionHelper.ObtenerCajaLocalId();
                    var cajaRepo = new momospos.Repositories.CajaRepository();
                    var sesion = cajaRepo.ObtenerSesionAbierta(cajaLocalId);
                    
                    if (sesion == null)
                    {
                        // No hay caja abierta, forzar apertura
                        var cajaForm = new CajaForm(usuario, true, cajaLocalId);
                        if (cajaForm.ShowDialog() != DialogResult.OK)
                        {
                            continue; // Se canceló la apertura, regresar a login
                        }
                        sesion = cajaRepo.ObtenerSesionAbierta(cajaLocalId);
                    }
                    
                    // 3. Iniciar MainForm
                    Application.Run(new MainForm(usuario, sesion));
                }
                else
                {
                    break; // Termina la aplicación si el usuario cierra o cancela el login
                }
            }
        }

        private static void GlobalExceptionHandler(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogAndShowException(e.Exception);
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogAndShowException(ex);
            }
        }

        private static void LogAndShowException(Exception ex)
        {
            try
            {
                // Intentar obtener el usuario de la sesión activa si MainForm está abierto
                int? usuarioId = null;
                string modulo = "Global";
                
                if (Application.OpenForms.Count > 0)
                {
                    var mainForm = Application.OpenForms["MainForm"] as MainForm;
                    if (mainForm != null && mainForm.UsuarioActual != null)
                    {
                        usuarioId = mainForm.UsuarioActual.Id;
                        
                        // Intentar sacar el módulo activo actual
                        if (mainForm.ContentPanel != null && mainForm.ContentPanel.Controls.Count > 0)
                        {
                            modulo = mainForm.ContentPanel.Controls[0].GetType().Name;
                        }
                    }
                }

                var repo = new momospos.Repositories.ExcepcionRepository();
                repo.Insertar(ex, modulo, usuarioId);

                momospos.Views.CustomMessageBox.Show(
                    "Ocurrió un error inesperado. El sistema ha registrado los detalles técnicos en la bitácora para su análisis.\n\n" +
                    "Mensaje: " + ex.Message,
                    "Error del Sistema",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // Si todo falla
                MessageBox.Show("Error fatal crítico:\n" + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
