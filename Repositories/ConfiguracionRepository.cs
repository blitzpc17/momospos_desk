using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;

namespace momospos.Repositories
{
    public class ConfiguracionRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ConfiguracionRepository()
        {
            // Auto-migración básica
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS Configuracion (
                        Clave VARCHAR(50) PRIMARY KEY, 
                        Valor TEXT
                    );
                    INSERT INTO Configuracion (Clave, Valor) VALUES 
                    ('NombreNegocio', 'Mi Tienda POS'), 
                    ('RFC', 'XAXX010101000'), 
                    ('Direccion', 'Calle Falsa 123'), 
                    ('MensajeTicket', '¡Gracias por su preferencia!') 
                    ON CONFLICT DO NOTHING;
                    
                    CREATE INDEX IF NOT EXISTS IDX_Productos_Nombre ON Productos(Nombre);
                    CREATE INDEX IF NOT EXISTS IDX_Productos_CodigoBarras ON Productos(CodigoBarras);
                    CREATE INDEX IF NOT EXISTS IDX_Clientes_Nombre ON Clientes(Nombre);
                    CREATE INDEX IF NOT EXISTS IDX_Ventas_Fecha ON Ventas(Fecha);
                ");
            }
        }

        private class ConfigRow
        {
            public string Clave { get; set; }
            public string Valor { get; set; }
        }

        public Dictionary<string, string> ObtenerTodas()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                var list = db.Query<ConfigRow>("SELECT Clave, Valor FROM Configuracion");
                return list.ToDictionary(row => row.Clave, row => row.Valor);
            }
        }

        public string ObtenerValor(string clave)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<string>("SELECT Valor FROM Configuracion WHERE Clave = @Clave", new { Clave = clave });
            }
        }

        public void GuardarValor(string clave, string valor)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute(@"
                    INSERT INTO Configuracion (Clave, Valor) VALUES (@Clave, @Valor)
                    ON CONFLICT (Clave) DO UPDATE SET Valor = EXCLUDED.Valor;
                ", new { Clave = clave, Valor = valor });
            }
        }
    }
}
