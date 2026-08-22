using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using Npgsql;

namespace MomosClinic.Repositories
{
    public class ServicioMedico
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Activo { get; set; }
    }

    public class ServiciosRepository
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString ?? "";
        }

        public List<ServicioMedico> ObtenerTodos()
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                return db.Query<ServicioMedico>(@"
                    SELECT Id, Nombre, Descripcion, PrecioVenta, Activo 
                    FROM public.Productos 
                    WHERE EsServicio = TRUE AND Activo = TRUE
                    ORDER BY Nombre ASC").AsList();
            }
        }

        public void Guardar(ServicioMedico servicio)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                if (servicio.Id == 0)
                {
                    // Al ser un servicio, se aplican flags específicos (no ocupa stock, precio fijo, esServicio, no caduca)
                    string sql = @"
                        INSERT INTO public.Productos 
                        (Nombre, CodigoBarras, Descripcion, PrecioCompra, PrecioVenta, StockActual, StockMinimo, CategoriaId, UnidadMedidaId, Activo, EsServicio, PrecioFijo, AplicaCaducidad) 
                        VALUES 
                        (@Nombre, @CodigoBarras, @Descripcion, 0, @PrecioVenta, 0, 0, (SELECT COALESCE((SELECT Id FROM public.Categorias WHERE Nombre = 'SERVICIOS' LIMIT 1), 1)), 1, @Activo, TRUE, TRUE, FALSE)";
                    
                    db.Execute(sql, new { 
                        servicio.Nombre, 
                        CodigoBarras = "SERV-" + Guid.NewGuid().ToString().Substring(0,8).ToUpper(), 
                        servicio.Descripcion, 
                        servicio.PrecioVenta, 
                        servicio.Activo 
                    });
                }
                else
                {
                    string sql = @"
                        UPDATE public.Productos 
                        SET Nombre = @Nombre, Descripcion = @Descripcion, PrecioVenta = @PrecioVenta, Activo = @Activo
                        WHERE Id = @Id AND EsServicio = TRUE";
                    db.Execute(sql, servicio);
                }
            }
        }

        public void Eliminar(int id)
        {
            using (IDbConnection db = new NpgsqlConnection(GetConnectionString()))
            {
                db.Execute("DELETE FROM public.Productos WHERE Id = @id AND EsServicio = TRUE", new { id });
            }
        }
    }
}
