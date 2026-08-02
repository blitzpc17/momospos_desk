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
            
            // 1. Mostrar Login
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                var usuario = loginForm.UsuarioAutenticado;
                
                // 2. Verificar si hay caja abierta
                var cajaRepo = new momospos.Repositories.CajaRepository();
                var sesion = cajaRepo.ObtenerSesionAbierta();
                
                if (sesion == null)
                {
                    // No hay caja abierta, forzar apertura
                    var cajaForm = new CajaForm(usuario, true);
                    if (cajaForm.ShowDialog() != DialogResult.OK)
                    {
                        return; // Se canceló la apertura, cerrar app
                    }
                    sesion = cajaRepo.ObtenerSesionAbierta();
                }
                
                // 3. Iniciar MainForm
                Application.Run(new MainForm(usuario, sesion));
            }
        }
    }
}
