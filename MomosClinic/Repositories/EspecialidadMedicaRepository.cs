using Dapper;
using MomosClinic.Models;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using Npgsql;

namespace MomosClinic.Repositories
{
    public class EspecialidadMedicaRepository
    {
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public IEnumerable<EspecialidadMedica> ObtenerTodas()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<EspecialidadMedica>("SELECT * FROM clinic.EspecialidadesMedicas WHERE Activo = true ORDER BY Nombre");
            }
        }
    }
}
