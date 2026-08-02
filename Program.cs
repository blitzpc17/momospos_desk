using System;
using System.Windows.Forms;
using momospos.Views;

namespace momospos
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 0. Probar conexión
            if (!momospos.Helpers.ConfiguracionHelper.ProbarConexionActual())
            {
                var configForm = new momospos.Views.ConfiguracionConexionForm();
                if (configForm.ShowDialog() != DialogResult.OK)
                {
                    return; // Si el usuario cancela la configuración, se cierra
                }
            }
            
            // 1. Mostrar Login
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
                        return; // Se canceló la apertura, cerrar app
                    }
                    sesion = cajaRepo.ObtenerSesionAbierta(cajaLocalId);
                }
                
                // 3. Iniciar MainForm
                Application.Run(new MainForm(usuario, sesion));
            }
        }
    }
}
