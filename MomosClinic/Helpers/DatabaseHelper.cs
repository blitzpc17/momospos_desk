using System;
using System.IO;
using Npgsql;
using momospos.Repositories;
using System.Configuration;

namespace MomosClinic.Helpers
{
    public static class DatabaseHelper
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString ?? "";
        }

        public static void EjecutarActualizacionEsquemas()
        {
            string connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString)) return;

            string[] scripts = new string[] { "Schema.sql", "UpdateSchema.sql", "ClinicSchema.sql", "UpdateSchemaClinic.sql" };
            
            foreach (var scriptName in scripts)
            {
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Database", scriptName);
                if (!File.Exists(scriptPath))
                {
                    scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", scriptName);
                }

                if (File.Exists(scriptPath))
                {
                    try
                    {
                        string sql = File.ReadAllText(scriptPath);
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var cmd = new NpgsqlCommand(sql, connection))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show($"Error al ejecutar {scriptName}: {ex.Message}", "Error de BD", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}
