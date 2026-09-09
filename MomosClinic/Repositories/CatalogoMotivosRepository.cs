using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using MomosClinic.Models;
using momospos.Helpers;

namespace MomosClinic.Repositories
{
    public class CatalogoMotivosRepository
    {
        private string GetConnectionString()
        {
            return ConfiguracionHelper.ObtenerCadenaConexion();
        }

        public List<MotivoCancelacionCita> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<MotivoCancelacionCita>("SELECT * FROM MotivosCancelacionCita WHERE Activo = TRUE ORDER BY Motivo").ToList();
            }
        }
    }
}
