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

            bool databaseExists = false;
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'usuarios')", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            databaseExists = Convert.ToBoolean(result);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error checking existence, assume it doesn't exist and fail gracefully later.
            }

            System.Collections.Generic.List<string> scripts = new System.Collections.Generic.List<string>();
            
            if (!databaseExists)
            {
                scripts.Add("Schema.sql");
                scripts.Add("ClinicSchema.sql");
            }
            
            scripts.Add("UpdateSchema.sql");
            scripts.Add("UpdateSchemaClinic.sql");
            
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
                        momospos.Views.CustomMessageBox.Show($"Error al ejecutar {scriptName}: {ex.Message}", "Error de BD", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}
