using System;
using System.Windows.Forms;

namespace MomosClinic
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Ejecutar scripts de creación para asegurar que clinic y OrdenesCobro existen
            Helpers.DatabaseHelper.EjecutarActualizacionEsquemas();

            var loginForm = new Views.LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainForm(loginForm.UsuarioLogueado));
            }
        }
    }
}