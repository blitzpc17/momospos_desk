using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using momospos.Models;
using momospos.Repositories;
using momospos.Helpers;

namespace momospos.Views
{
    public class CorteDatos
    {
        public CajaSesion Sesion { get; set; }
        public string NombreCajero { get; set; }
        public DateTime FechaImpresion { get; set; }
        
        public int TotalTickets { get; set; }
        public decimal FondoInicial { get; set; }
        
        // Ventas confirmadas del turno
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasCredito { get; set; }
        
        // Movimientos de caja físicos
        public decimal IngresosCajaEfectivo { get; set; } // VENTA + INGRESO (incluye ventas cobradas, abonos y ventas que luego se cancelaron)
        public decimal SalidasCajaEfectivo { get; set; }  // RETIRO + DEVOLUCION (retiros manuales y dinero devuelto por cancelaciones)
        
        public decimal GananciaBruta { get; set; }
    }

    public class CortePrinter
    {
        private CorteDatos _datos;
        private ConfiguracionRepository _configRepo;
        private Dictionary<string, string> _configs;
        private bool _esPreCorte;

        public CortePrinter(CajaSesion sesion, string nombreCajero, bool esPreCorte = true)
        {
            _configRepo = new ConfiguracionRepository();
            _configs = _configRepo.ObtenerTodas();
            _esPreCorte = esPreCorte;
            _datos = CalcularDatos(sesion, nombreCajero);
        }

        private CorteDatos CalcularDatos(CajaSesion sesion, string nombreCajero)
        {
            var cajaRepo = new CajaRepository();
            var ventaRepo = new VentaRepository();
            
            var movimientos = cajaRepo.ObtenerMovimientosSesion(sesion.Id).ToList();
            
            var ventas = ventaRepo.ObtenerReporteVentas(sesion.FechaApertura.Date, DateTime.Today.AddDays(1).AddTicks(-1))
                                  .Historial
                                  .Where(v => v.CajaSesionId == sesion.Id && v.Estado == "CONFIRMADO")
                                  .ToList();

            var datos = new CorteDatos
            {
                Sesion = sesion,
                NombreCajero = nombreCajero,
                FechaImpresion = DateTime.Now,
                TotalTickets = ventas.Count,
                FondoInicial = sesion.FondoInicial,
                
                IngresosCajaEfectivo = movimientos.Where(x => x.Tipo == "VENTA" || x.Tipo == "INGRESO").Sum(x => x.Importe),
                SalidasCajaEfectivo = movimientos.Where(x => x.Tipo == "RETIRO" || x.Tipo == "DEVOLUCION").Sum(x => Math.Abs(x.Importe))
            };

            datos.VentasEfectivo = 0;
            datos.VentasTarjeta = 0;
            datos.VentasCredito = 0;
            
            foreach(var v in ventas)
            {
                var ventaCompleta = ventaRepo.ObtenerVentaPorId(v.Id);
                if (ventaCompleta != null && ventaCompleta.Pagos != null)
                {
                    datos.VentasEfectivo += ventaCompleta.Pagos.Where(p => p.MetodoPago == "EFECTIVO").Sum(p => p.Importe);
                    datos.VentasTarjeta += ventaCompleta.Pagos.Where(p => p.MetodoPago == "TARJETA").Sum(p => p.Importe);
                    datos.VentasCredito += ventaCompleta.Pagos.Where(p => p.MetodoPago == "CREDITO").Sum(p => p.Importe);
                }
            }

            try
            {
                var detalles = ventaRepo.ObtenerReporteVentaDetallado(sesion.FechaApertura.Date, DateTime.Today.AddDays(1).AddTicks(-1))
                                        .Where(x => ventas.Any(v => v.Folio == x.Folio));
                datos.GananciaBruta = detalles.Sum(x => x.TotalVenta - x.TotalCosto);
            }
            catch
            {
                datos.GananciaBruta = 0;
            }

            return datos;
        }

        public void Imprimir()
        {
            if (!_configs.ContainsKey("ImpresoraTicket") || string.IsNullOrEmpty(_configs["ImpresoraTicket"]))
            {
                throw new Exception("No hay una impresora de tickets configurada en el sistema.");
            }
            
            string impresora = _configs["ImpresoraTicket"];
            bool is80mm = _configs.ContainsKey("TamanoTicket") && _configs["TamanoTicket"] == "80mm";
            int maxLen = is80mm ? 48 : 32;

            TicketEscPos ticket = new TicketEscPos();

            string empName = _configs.ContainsKey("NombreNegocio") ? _configs["NombreNegocio"] : "MomosPOS";
            string dir = _configs.ContainsKey("Direccion") ? _configs["Direccion"] : "";
            string rfc = _configs.ContainsKey("RFC") ? _configs["RFC"] : "";

            ticket.Centrar();
            ticket.Linea(empName);
            if (!string.IsNullOrEmpty(dir)) ticket.Linea(dir);
            if (!string.IsNullOrEmpty(rfc)) ticket.Linea(rfc);
            string tituloCorte = _esPreCorte ? "PRE-CORTE DE CAJA" : "CORTE DEL DIA";
            ticket.Linea($"{tituloCorte}\nDEL {_datos.FechaImpresion.ToString("dd/MMM/yyyy").ToUpper()}");
            ticket.Linea();

            ticket.AlinearIzquierda();
            ticket.Linea($"REALIZADO: {_datos.FechaImpresion.ToString("dd/MMM/yyyy hh:mm tt").ToUpper()}");
            ticket.Linea($"CAJERO: {_datos.NombreCajero.ToUpper()}");
            ticket.Linea($"CAJA: {_datos.Sesion.CajaId}");
            ticket.Linea();

            void PrintSection(string title)
            {
                ticket.Centrar();
                ticket.Linea($"== {title} ==");
                ticket.AlinearIzquierda();
            }
            
            PrintSection("VENTAS DEL DIA");
            ticket.Linea($"{_datos.TotalTickets} VENTAS EN EL DIA.");
            ticket.Linea();

            decimal esperado = _datos.FondoInicial + _datos.IngresosCajaEfectivo - _datos.SalidasCajaEfectivo;
            PrintSection("DINERO EN CAJA");
            ticket.Linea(FormatLine("FONDO INICIAL:", $"+{_datos.FondoInicial:C}", maxLen));
            ticket.Linea(FormatLine("ENTRADAS (Ventas/Abonos):", $"+{_datos.IngresosCajaEfectivo:C}", maxLen));
            ticket.Linea(FormatLine("SALIDAS (Retiros/Devol):", $"-{_datos.SalidasCajaEfectivo:C}", maxLen));
            ticket.Linea(FormatLine("EFECTIVO ESPERADO:", esperado, maxLen));
            ticket.Linea();

            decimal ventasTot = _datos.VentasEfectivo + _datos.VentasTarjeta + _datos.VentasCredito;
            PrintSection("VENTAS CONFIRMADAS");
            ticket.Linea(FormatLine("EFECTIVO:", _datos.VentasEfectivo, maxLen));
            ticket.Linea(FormatLine("TARJETA:", _datos.VentasTarjeta, maxLen));
            ticket.Linea(FormatLine("CREDITO:", _datos.VentasCredito, maxLen));
            ticket.Linea(FormatLine("TOTAL VENDIDO:", ventasTot, maxLen));
            ticket.Linea();

            PrintSection("GANANCIA DEL DIA");
            ticket.Linea(FormatLine("GANANCIA BRUTA:", _datos.GananciaBruta, maxLen));
            ticket.Linea();

            ticket.Avanzar(4);
            ticket.CorteCompleto();

            bool resultado = momospos.Helpers.RawPrinterHelper.SendBytesToPrinter(impresora, ticket.ObtenerDatos());
            if (!resultado)
            {
                throw new Exception("Error enviando datos RAW a la impresora.");
            }
        }

        private string FormatLine(string left, decimal value, int maxLen)
        {
            return FormatLine(left, value.ToString("C"), maxLen);
        }

        private string FormatLine(string left, string right, int maxLen)
        {
            int spaces = maxLen - left.Length - right.Length;
            if (spaces < 1) spaces = 1;
            return left + new string(' ', spaces) + right;
        }

        public void ImprimirComoPdf(string outputPath)
        {
            using (PrintDocument pd = new PrintDocument())
            {
                pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pd.PrinterSettings.PrintToFile = true;
                pd.PrinterSettings.PrintFileName = outputPath;

                pd.DefaultPageSettings.PaperSize = new PaperSize("Custom", 315, 1200); 
                pd.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

                pd.PrintPage += Pd_PrintPage;
                pd.Print();
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font fTitle = new Font("Courier New", 12, FontStyle.Bold);
            Font fNormal = new Font("Courier New", 9);
            Brush brush = Brushes.Black;

            float yPos = 10;
            float leftMargin = 10;
            float rightMargin = e.PageBounds.Width - 20;

            StringFormat centerFmt = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat rightFmt = new StringFormat { Alignment = StringAlignment.Far };

            RectangleF rectFull = new RectangleF(0, yPos, e.PageBounds.Width, 20);

            if (_configs.ContainsKey("RutaLogo") && !string.IsNullOrEmpty(_configs["RutaLogo"]) && System.IO.File.Exists(_configs["RutaLogo"]))
            {
                try
                {
                    using (Image logo = Image.FromFile(_configs["RutaLogo"]))
                    {
                        int w = 150;
                        int h = (int)(logo.Height * ((float)w / logo.Width));
                        g.DrawImage(logo, (e.PageBounds.Width - w) / 2, yPos, w, h);
                        yPos += h + 10;
                    }
                }
                catch { }
            }

            string empName = _configs.ContainsKey("NombreNegocio") ? _configs["NombreNegocio"] : "MomosPOS";
            string dir = _configs.ContainsKey("Direccion") ? _configs["Direccion"] : "";
            string rfc = _configs.ContainsKey("RFC") ? _configs["RFC"] : "";

            rectFull.Y = yPos; g.DrawString(empName, fTitle, brush, rectFull, centerFmt); yPos += 20;
            if (!string.IsNullOrEmpty(dir)) { rectFull.Y = yPos; g.DrawString(dir, fNormal, brush, rectFull, centerFmt); yPos += 15; }
            if (!string.IsNullOrEmpty(rfc)) { rectFull.Y = yPos; g.DrawString(rfc, fNormal, brush, rectFull, centerFmt); yPos += 15; }

            yPos += 10;
            string tituloCorte = _esPreCorte ? "PRE-CORTE DE CAJA" : "CORTE DEL DIA";
            rectFull.Y = yPos; g.DrawString(tituloCorte, fTitle, brush, rectFull, centerFmt); yPos += 20;
            rectFull.Y = yPos; g.DrawString($"DEL {_datos.FechaImpresion.ToString("dd/MMM/yyyy").ToUpper()}", fTitle, brush, rectFull, centerFmt); yPos += 30;

            g.DrawString($"REALIZADO: {_datos.FechaImpresion.ToString("dd/MMM/yyyy hh:mm tt").ToUpper()}", fNormal, brush, leftMargin, yPos); yPos += 15;
            g.DrawString($"CAJERO: {_datos.NombreCajero.ToUpper()}", fNormal, brush, leftMargin, yPos); yPos += 15;
            g.DrawString($"CAJA: {_datos.Sesion.CajaId}", fNormal, brush, leftMargin, yPos); yPos += 25;

            void DrawSection(string title)
            {
                rectFull.Y = yPos; 
                g.DrawString($"== {title} ==", fNormal, brush, rectFull, centerFmt); 
                yPos += 20;
            }
            void DrawLine(string lbl, string val)
            {
                g.DrawString(lbl, fNormal, brush, leftMargin, yPos);
                g.DrawString(val, fNormal, brush, rightMargin, yPos, rightFmt);
                yPos += 15;
            }

            DrawSection("VENTAS DEL DIA");
            g.DrawString($"{_datos.TotalTickets} VENTAS EN EL DIA.", fNormal, brush, leftMargin, yPos); yPos += 25;

            decimal esperado = _datos.FondoInicial + _datos.IngresosCajaEfectivo - _datos.SalidasCajaEfectivo;
            DrawSection("DINERO EN CAJA");
            DrawLine("FONDO INICIAL:", $"+{_datos.FondoInicial:C}");
            DrawLine("ENTRADAS (Ventas/Abonos):", $"+{_datos.IngresosCajaEfectivo:C}");
            DrawLine("SALIDAS (Retiros/Devol):", $"-{_datos.SalidasCajaEfectivo:C}");
            DrawLine("EFECTIVO ESPERADO:", esperado.ToString("C")); yPos += 10;

            decimal ventasTot = _datos.VentasEfectivo + _datos.VentasTarjeta + _datos.VentasCredito;
            DrawSection("VENTAS CONFIRMADAS");
            DrawLine("EFECTIVO:", _datos.VentasEfectivo.ToString("C"));
            DrawLine("TARJETA:", _datos.VentasTarjeta.ToString("C"));
            DrawLine("CREDITO:", _datos.VentasCredito.ToString("C"));
            DrawLine("TOTAL VENDIDO:", ventasTot.ToString("C")); yPos += 10;

            DrawSection("GANANCIA DEL DIA");
            DrawLine("GANANCIA BRUTA:", _datos.GananciaBruta.ToString("C")); yPos += 10;

            e.HasMorePages = false;
        }
    }
}
