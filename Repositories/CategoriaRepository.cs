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
                    db.Execute("UPDATE Categorias SET Nombre = @Nombre WHERE Id = @Id", cat);
                }
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
