using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using momospos.Models;
using Npgsql;

namespace momospos.Repositories
{
    public class SeguridadRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public bool UsuarioTienePermiso(int usuarioId, string moduloClave)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                // Un admin siempre tiene permisos implícitos.
                bool esAdmin = db.QueryFirstOrDefault<bool>("SELECT EsAdmin FROM Usuarios WHERE Id = @Id", new { Id = usuarioId });
                if (esAdmin) return true;

                // Consulta consolidada: Revisar si el módulo está asignado al rol del usuario,
                // O si está asignado explícitamente al usuario.
                // Y revisar que NO esté explícitamente denegado al usuario.
                string query = @"
                    SELECT m.Id
                    FROM Modulos m
                    WHERE m.Clave = @Clave
                    AND (
                        (
                            EXISTS (
                                SELECT 1 FROM RolModulos rm 
                                INNER JOIN UsuarioRoles ur ON ur.RolId = rm.RolId
                                WHERE rm.ModuloId = m.Id AND ur.UsuarioId = @UsuarioId
                            )
                            OR EXISTS (
                                SELECT 1 FROM UsuarioModulos um 
                                WHERE um.ModuloId = m.Id AND um.UsuarioId = @UsuarioId AND um.Concedido = true
                            )
                        )
                        AND NOT EXISTS (
                            SELECT 1 FROM UsuarioModulos um 
                            WHERE um.ModuloId = m.Id AND um.UsuarioId = @UsuarioId AND um.Concedido = false
                        )
                    )";

                var id = db.QueryFirstOrDefault<int?>(query, new { UsuarioId = usuarioId, Clave = moduloClave });
                return id.HasValue;
            }
        }

        public List<Modulo> ObtenerArbolModulos(int usuarioId, bool esAdmin)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                List<Modulo> todosLosModulosPermitidos;

                if (esAdmin)
                {
                    todosLosModulosPermitidos = db.Query<Modulo>("SELECT * FROM Modulos ORDER BY Orden").ToList();
                }
                else
                {
                    string query = @"
                        SELECT m.*
                        FROM Modulos m
                        WHERE (
                            EXISTS (
                                SELECT 1 FROM RolModulos rm 
                                INNER JOIN UsuarioRoles ur ON ur.RolId = rm.RolId
                                WHERE rm.ModuloId = m.Id AND ur.UsuarioId = @UsuarioId
                            )
                            OR EXISTS (
                                SELECT 1 FROM UsuarioModulos um 
                                WHERE um.ModuloId = m.Id AND um.UsuarioId = @UsuarioId AND um.Concedido = true
                            )
                        )
                        AND NOT EXISTS (
                            SELECT 1 FROM UsuarioModulos um 
                            WHERE um.ModuloId = m.Id AND um.UsuarioId = @UsuarioId AND um.Concedido = false
                        )
                        ORDER BY m.Orden";
                    todosLosModulosPermitidos = db.Query<Modulo>(query, new { UsuarioId = usuarioId }).ToList();
                }

                return ConstruirArbol(todosLosModulosPermitidos);
            }
        }

        private List<Modulo> ConstruirArbol(List<Modulo> modulosPlana)
        {
            var dict = modulosPlana.ToDictionary(m => m.Id);
            var raices = new List<Modulo>();

            foreach (var m in modulosPlana)
            {
                if (m.PadreId.HasValue && dict.ContainsKey(m.PadreId.Value))
                {
                    dict[m.PadreId.Value].Submodulos.Add(m);
                }
                else
                {
                    raices.Add(m);
                }
            }

            // Ordenar submodulos
            foreach (var m in raices)
            {
                OrdenarSubmodulos(m);
            }

            return raices.OrderBy(r => r.Orden).ToList();
        }

        private void OrdenarSubmodulos(Modulo padre)
        {
            if (padre.Submodulos.Any())
            {
                padre.Submodulos = padre.Submodulos.OrderBy(m => m.Orden).ToList();
                foreach (var hijo in padre.Submodulos)
                {
                    OrdenarSubmodulos(hijo);
                }
            }
        }
        
        public IEnumerable<Rol> ObtenerTodosLosRoles()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Rol>("SELECT * FROM Roles ORDER BY Nombre").ToList();
            }
        }

        public List<Modulo> ObtenerTodosLosModulosPlana()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<Modulo>("SELECT * FROM Modulos ORDER BY Orden").ToList();
            }
        }

        public List<int> ObtenerModulosPorRol(int rolId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<int>("SELECT ModuloId FROM RolModulos WHERE RolId = @Id", new { Id = rolId }).ToList();
            }
        }

        public void GuardarModulosPorRol(int rolId, List<int> modulosIds)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("DELETE FROM RolModulos WHERE RolId = @Id", new { Id = rolId });
                foreach (var id in modulosIds)
                {
                    db.Execute("INSERT INTO RolModulos (RolId, ModuloId) VALUES (@RolId, @ModuloId)", 
                        new { RolId = rolId, ModuloId = id });
                }
            }
        }

        public Rol ObtenerRolDeUsuario(int usuarioId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                string sql = "SELECT r.* FROM Roles r INNER JOIN UsuarioRoles ur ON r.Id = ur.RolId WHERE ur.UsuarioId = @Id";
                return db.QueryFirstOrDefault<Rol>(sql, new { Id = usuarioId });
            }
        }

        public List<UsuarioModulo> ObtenerModulosPorUsuario(int usuarioId)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<UsuarioModulo>("SELECT * FROM UsuarioModulos WHERE UsuarioId = @Id", new { Id = usuarioId }).ToList();
            }
        }

        public void GuardarPermisosUsuario(int usuarioId, int? rolId, List<UsuarioModulo> excepciones)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("DELETE FROM UsuarioRoles WHERE UsuarioId = @Id", new { Id = usuarioId });
                if (rolId.HasValue)
                {
                    db.Execute("INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (@UsuarioId, @RolId)", 
                        new { UsuarioId = usuarioId, RolId = rolId.Value });
                }

                db.Execute("DELETE FROM UsuarioModulos WHERE UsuarioId = @Id", new { Id = usuarioId });
                foreach (var exc in excepciones)
                {
                    db.Execute("INSERT INTO UsuarioModulos (UsuarioId, ModuloId, Concedido) VALUES (@UsuarioId, @ModuloId, @Concedido)", 
                        new { UsuarioId = usuarioId, ModuloId = exc.ModuloId, Concedido = exc.Concedido });
                }
            }
        }
    }
}
