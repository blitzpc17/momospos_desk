using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using momospos.Models;

namespace momospos.Repositories
{
    public class CategoriaRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public List<Categoria> ObtenerTodas()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Categoria>("SELECT * FROM Categorias ORDER BY Nombre").ToList();
            }
        }

        public void Guardar(Categoria cat)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (cat.Id == 0)
                {
                    db.Execute("INSERT INTO Categorias (Nombre) VALUES (@Nombre)", cat);
                }
                else
                {
                    var existingName = db.QueryFirstOrDefault<string>("SELECT Nombre FROM Categorias WHERE Id = @Id", new { Id = cat.Id });
                    if (existingName?.ToUpper() == "SERVICIOS")
                        throw new System.Exception("La categoría 'SERVICIOS' es reservada por el sistema y no se puede modificar.");
                        
                    db.Execute("UPDATE Categorias SET Nombre = @Nombre WHERE Id = @Id", cat);
                }
            }
        }

        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                var existingName = db.QueryFirstOrDefault<string>("SELECT Nombre FROM Categorias WHERE Id = @Id", new { Id = id });
                if (existingName?.ToUpper() == "SERVICIOS")
                    throw new System.Exception("La categoría 'SERVICIOS' es reservada por el sistema y no se puede eliminar.");
                    
                int count = db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Productos WHERE CategoriaId = @Id", new { Id = id });
                if (count > 0)
                    throw new System.Exception($"No se puede eliminar la categoría porque tiene {count} producto(s) asociado(s).");
                    
                db.Execute("DELETE FROM Categorias WHERE Id = @Id", new { Id = id });
            }
        }
    }

    public class UnidadMedidaRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public List<UnidadMedida> ObtenerTodas()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<UnidadMedida>("SELECT * FROM UnidadesMedida ORDER BY Nombre").ToList();
            }
        }
    }
}
