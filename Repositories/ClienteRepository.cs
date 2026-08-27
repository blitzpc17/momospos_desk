using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using momospos.Models;

namespace momospos.Repositories
{
    public class ClienteRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public List<Cliente> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Cliente>("SELECT * FROM Clientes ORDER BY Nombre").ToList();
            }
        }

        public void Guardar(Cliente c)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (c.Id == 0)
                {
                    db.Execute(@"INSERT INTO Clientes (Nombre, Telefono, Correo, LimiteCredito) 
                                 VALUES (@Nombre, @Telefono, @Correo, @LimiteCredito)", c);
                }
                else
                {
                    db.Execute(@"UPDATE Clientes SET Nombre=@Nombre, Telefono=@Telefono, 
                                 Correo=@Correo, LimiteCredito=@LimiteCredito WHERE Id=@Id", c);
                }
            }
        }

        public void Actualizar(Cliente c)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute(@"UPDATE Clientes SET Nombre=@Nombre, Telefono=@Telefono, 
                             Correo=@Correo, LimiteCredito=@LimiteCredito, Saldo=@Saldo, Estado=@Estado WHERE Id=@Id", c);
            }
        }

        public void CambiarEstado(int id, string estado)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("UPDATE Clientes SET Estado = @Estado WHERE Id = @Id", new { Estado = estado, Id = id });
            }
        }
    }
}
