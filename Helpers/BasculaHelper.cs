using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace momospos.Helpers
{
    public static class BasculaHelper
    {
        public static decimal LeerPeso(string puerto, int baudRate = 9600)
        {
            if (string.IsNullOrWhiteSpace(puerto))
                throw new Exception("Puerto COM no configurado.");

            using (SerialPort sp = new SerialPort(puerto, baudRate, Parity.None, 8, StopBits.One))
            {
                sp.ReadTimeout = 1500;
                sp.WriteTimeout = 500;

                sp.Open();
                
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();

                // Enviar comando genérico de impresión ('P' suele funcionar en Torrey y Rhino)
                sp.Write("P");
                
                Thread.Sleep(200);

                string readData = "";
                try
                {
                    for(int i = 0; i < 5; i++)
                    {
                        if(sp.BytesToRead > 0)
                        {
                            readData += sp.ReadExisting();
                            // Si detecta un salto de línea o "kg", asume que ya terminó el mensaje.
                            if(readData.Contains("\r") || readData.Contains("\n") || readData.ToLower().Contains("kg"))
                                break;
                        }
                        Thread.Sleep(100);
                    }

                    if (string.IsNullOrWhiteSpace(readData))
                    {
                        // Intentar ReadLine si envía flujo continuo
                        readData = sp.ReadLine();
                    }
                }
                catch (TimeoutException)
                {
                    if (string.IsNullOrWhiteSpace(readData))
                        throw new Exception("Tiempo de espera agotado. Asegúrese de que la báscula esté encendida, conectada y que el peso sea estable.");
                }

                return ExtraerPeso(readData);
            }
        }

        private static decimal ExtraerPeso(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData))
                throw new Exception("La báscula no regresó ningún valor.");

            // Expresión regular para encontrar el número (permite decimales y negativos)
            Match m = Regex.Match(rawData, @"-?\d+(\.\d+)?");
            if (m.Success)
            {
                if (decimal.TryParse(m.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal peso))
                {
                    return peso;
                }
            }

            throw new Exception($"El formato enviado por la báscula no se pudo interpretar: '{rawData.Trim()}'");
        }
    }
}
