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
    }
}
