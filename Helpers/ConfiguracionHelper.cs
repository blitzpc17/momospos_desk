using System;
using System.Configuration;
using System.Data;
using Npgsql;

namespace momospos.Helpers
{
    public static class ConfiguracionHelper
    {
        private const string ConnectionName = "DefaultConnection";
        private const string CajaIdKey = "CajaLocalId";

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

        public static bool ProbarConexion(string server, string port, string database, string username, string password)
        {
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
    }
}
