using System;
using System.Configuration;
using System.Data;
using System.IO;
using Npgsql;

namespace momospos.Helpers
{
    public static class ConfiguracionHelper
    {
        private const string ConnectionName = "DefaultConnection";
        private const string CajaIdKey = "CajaLocalId";
        private const string UsarBasculaKey = "UsarBascula";
        private const string PuertoBasculaKey = "PuertoBascula";

        public static bool ObtenerUsarBascula()
        {
            var value = ConfigurationManager.AppSettings[UsarBasculaKey];
            if (bool.TryParse(value, out bool usar))
                return usar;
            return false;
        }

        public static void GuardarUsarBascula(bool usar)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[UsarBasculaKey] != null)
                config.AppSettings.Settings[UsarBasculaKey].Value = usar.ToString();
            else
                config.AppSettings.Settings.Add(UsarBasculaKey, usar.ToString());
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string ObtenerPuertoBascula()
        {
            return ConfigurationManager.AppSettings[PuertoBasculaKey] ?? "";
        }

        public static void GuardarPuertoBascula(string puerto)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[PuertoBasculaKey] != null)
                config.AppSettings.Settings[PuertoBasculaKey].Value = puerto;
            else
                config.AppSettings.Settings.Add(PuertoBasculaKey, puerto);
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static int ObtenerCajaLocalId()
        {
            var value = ConfigurationManager.AppSettings[CajaIdKey];
            if (int.TryParse(value, out int cajaId))
                return cajaId;
            return 1; // Caja Principal por defecto
        }

        public static void GuardarCajaLocalId(int cajaId)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[CajaIdKey] != null)
                config.AppSettings.Settings[CajaIdKey].Value = cajaId.ToString();
            else
                config.AppSettings.Settings.Add(CajaIdKey, cajaId.ToString());
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string ObtenerRutaRecursos()
        {
            try 
            {
                var repo = new momospos.Repositories.ConfiguracionRepository();
                string ruta = repo.ObtenerValor("RutaRecursos");
                if (string.IsNullOrWhiteSpace(ruta))
                    return @"C:\MomosPos_Resources";
                return ruta;
            }
            catch 
            {
                return @"C:\MomosPos_Resources";
            }
        }

        public static string ObtenerCadenaConexion()
        {
            return ConfigurationManager.ConnectionStrings[ConnectionName]?.ConnectionString ?? "";
        }

        public static void GuardarCadenaConexion(string server, string port, string database, string username, string password)
        {
            string connectionString = $"Host={server};Port={port};Database={database};Username={username};Password={password}";

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
            if (config.ConnectionStrings.ConnectionStrings[ConnectionName] != null)
            {
                config.ConnectionStrings.ConnectionStrings[ConnectionName].ConnectionString = connectionString;
                config.ConnectionStrings.ConnectionStrings[ConnectionName].ProviderName = "Npgsql";
            }
            else
            {
                config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(ConnectionName, connectionString, "Npgsql"));
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        public static void CrearBaseDatosSiNoExiste(string host, string port, string database, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(database)) return;
            string postgresConnStr = $"Host={host};Port={port};Database=postgres;Username={username};Password={password};Timeout=3";
            try
            {
                using (var conn = new NpgsqlConnection(postgresConnStr))
                {
                    conn.Open();
                    bool dbExists = false;
                    using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname", conn))
                    {
                        cmd.Parameters.AddWithValue("dbname", database);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            dbExists = true;
                        }
                    }

                    if (!dbExists)
                    {
                        using (var cmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch
            {
                // Ignorar error, fallará en la conexión real
            }
        }

        public static bool ProbarConexion(string server, string port, string database, string username, string password)
        {
            CrearBaseDatosSiNoExiste(server, port, database, username, password);
            string connectionString = $"Host={server};Port={port};Database={database};Username={username};Password={password};Timeout=3";
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }
        
        public static bool ProbarConexionActual()
        {
            string connectionString = ObtenerCadenaConexion();
            if (string.IsNullOrEmpty(connectionString))
                return false;
                
            if (AnalizarCadena(connectionString, out string host, out string port, out string database, out string username, out string password))
            {
                CrearBaseDatosSiNoExiste(host, port, database, username, password);
            }

            if (!connectionString.Contains("Timeout="))
            {
                connectionString += ";Timeout=3";
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool AnalizarCadena(string connectionString, out string host, out string port, out string database, out string username, out string password)
        {
            host = "";
            port = "5432";
            database = "";
            username = "";
            password = "";

            if (string.IsNullOrEmpty(connectionString)) return false;

            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                host = builder.Host;
                port = builder.Port.ToString();
                database = builder.Database;
                username = builder.Username;
                password = builder.Password;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void EjecutarActualizacionDeEsquema()
        {
            string connectionString = ObtenerCadenaConexion();
            if (string.IsNullOrEmpty(connectionString)) return;

            string schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Database", "Schema_Produccion.sql");
            if (!File.Exists(schemaPath))
            {
                schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Schema_Produccion.sql");
            }
            if (File.Exists(schemaPath))
            {
                try
                {
                    string sqlSchema = File.ReadAllText(schemaPath);
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var cmd = new NpgsqlCommand(sqlSchema, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error al inicializar la base de datos: " + ex.Message, "Error de DB", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }

            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Database", "UpdateSchema.sql");
            if (!File.Exists(scriptPath))
            {
                scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "UpdateSchema.sql");
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
                    System.Windows.Forms.MessageBox.Show("Error al actualizar la base de datos: " + ex.Message, "Error de DB", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }
    }
}
