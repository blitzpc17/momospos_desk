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

        public void Registrar(Usuario usuario)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO Usuarios (Nombre, Usuario, PasswordHash, EsAdmin, Estado, CreadoEn) 
                               VALUES (@Nombre, @UsuarioLogin, @PasswordHash, @EsAdmin, 'ACTIVO', CURRENT_TIMESTAMP);";
                db.Execute(sql, usuario);
            }
        }

        public void Actualizar(Usuario usuario)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (string.IsNullOrEmpty(usuario.PasswordHash))
                {
                    string sql = @"UPDATE Usuarios SET Nombre = @Nombre, Usuario = @UsuarioLogin, EsAdmin = @EsAdmin WHERE Id = @Id;";
                    db.Execute(sql, usuario);
                }
                else
                {
                    string sql = @"UPDATE Usuarios SET Nombre = @Nombre, Usuario = @UsuarioLogin, PasswordHash = @PasswordHash, EsAdmin = @EsAdmin WHERE Id = @Id;";
                    db.Execute(sql, usuario);
                }
            }
        }

        public void CambiarEstado(int usuarioId, string nuevoEstado)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "UPDATE Usuarios SET Estado = @Estado WHERE Id = @Id;";
                db.Execute(sql, new { Estado = nuevoEstado, Id = usuarioId });
            }
        }
    }
}
