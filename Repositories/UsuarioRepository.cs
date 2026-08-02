using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using momospos.Models;

namespace momospos.Repositories
{
    public class UsuarioRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public Usuario Autenticar(string usuarioLogin, string password)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // En un sistema real se debería usar Hashing real (ej. BCrypt)
                return db.QueryFirstOrDefault<Usuario>(
                    "SELECT * FROM Usuarios WHERE Usuario = @UsuarioLogin AND PasswordHash = @Password AND Estado = 'ACTIVO'", 
                    new { UsuarioLogin = usuarioLogin, Password = password });
            }
        }

        public System.Collections.Generic.IEnumerable<Usuario> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Usuario>("SELECT * FROM Usuarios ORDER BY Nombre");
            }
        }
    }
}
