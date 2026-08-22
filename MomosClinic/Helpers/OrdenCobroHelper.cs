using System;
using System.Collections.Generic;
using System.Linq;
using momospos.Models;
using momospos.Repositories;
using MomosClinic.Models;
using System.Web.Script.Serialization;

namespace MomosClinic.Helpers
{
    public static class OrdenCobroHelper
    {
        public static void EnviarRecetaACaja(MomosClinic.Models.Paciente paciente, MomosClinic.Models.Receta receta, int? servicioId = null)
        {
            var ventaDetalles = new List<VentaDetalle>();
            var productoRepo = new ProductoRepository();

            if (servicioId.HasValue && servicioId.Value > 0)
            {
                var srv = productoRepo.ObtenerPorId(servicioId.Value);
                if (srv != null)
                {
                    ventaDetalles.Add(new VentaDetalle
                    {
                        ProductoId = srv.Id,
                        Descripcion = srv.Nombre,
                        PrecioUnitario = srv.PrecioVenta,
                        Cantidad = 1,
                        Subtotal = srv.PrecioVenta
                    });
                }
            }

            if (receta != null && receta.Detalles != null)
            {
                foreach (var det in receta.Detalles)
                {
                    if (det.ProductoId.HasValue && det.ProductoId.Value > 0)
                    {
                        var prod = productoRepo.ObtenerPorId(det.ProductoId.Value);
                        if (prod != null)
                        {
                            var vd = new VentaDetalle
                            {
                                ProductoId = prod.Id,
                                Descripcion = prod.Nombre,
                                PrecioUnitario = prod.PrecioVenta,
                                Cantidad = det.Cantidad > 0 ? det.Cantidad : 1,
                                Subtotal = (det.Cantidad > 0 ? det.Cantidad : 1) * prod.PrecioVenta
                            };
                            ventaDetalles.Add(vd);
                        }
                    }
                }

                if (ventaDetalles.Count > 0)
                {
                    var serializer = new JavaScriptSerializer();
                    string json = serializer.Serialize(ventaDetalles);

                    var repo = new OrdenesCobroRepository();
                    var orden = new OrdenCobro
                    {
                        Referencia = "Receta Paciente: " + paciente.NombreCompleto,
                        ModuloOrigen = "MomosClinic",
                        JsonDetalles = json
                    };
                    repo.Insertar(orden);
                }
            }
        }


    }
}
