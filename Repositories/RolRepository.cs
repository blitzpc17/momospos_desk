using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using momospos.Models;
using Npgsql;

namespace momospos.Repositories
{
    public class RolRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public IEnumerable<Rol> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Rol>("SELECT * FROM Roles ORDER BY Nombre");
            }
        }

        public Rol ObtenerPorId(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.QueryFirstOrDefault<Rol>("SELECT * FROM Roles WHERE Id = @Id", new { Id = id });
            }
        }

        public void Insertar(Rol rol)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "INSERT INTO Roles (Nombre, Descripcion, Activo) VALUES (@Nombre, @Descripcion, @Activo)";
                db.Execute(sql, rol);
            }
        }

        public void Actualizar(Rol rol)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "UPDATE Roles SET Nombre = @Nombre, Descripcion = @Descripcion, Activo = @Activo WHERE Id = @Id";
                db.Execute(sql, rol);
            }
        }

        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Soft delete
                db.Execute("UPDATE Roles SET Activo = FALSE WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
