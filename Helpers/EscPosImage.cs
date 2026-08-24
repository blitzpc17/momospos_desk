using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace momospos.Helpers
{
    public static class EscPosImage
    {
        public static byte[] ImageToEscPos(string imagePath, int maxWidth = 500)
        {
            try
            {
                // Leer a través de MemoryStream para evitar bloqueos
                byte[] imgBytes = System.IO.File.ReadAllBytes(imagePath);
                using (var ms = new System.IO.MemoryStream(imgBytes))
                using (Image original = Image.FromStream(ms))
                {
                    int width = original.Width;
                    int height = original.Height;

                    if (width > maxWidth)
                    {
                        double ratio = (double)maxWidth / width;
                        width = maxWidth;
                        height = (int)(height * ratio);
                    }

                    using (Bitmap bitmap = new Bitmap(width, height))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.Clear(Color.White);
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(original, 0, 0, width, height);
                        }

                        int widthBytes = (width + 7) / 8;
                        List<byte> result = new List<byte>();

                        // Comando GS v 0 para imprimir imagen raster
                        result.Add(0x1D);
                        result.Add(0x76);
                        result.Add(0x30);
                        result.Add(0x00);

                        // Ancho en bytes (Little Endian)
                        result.Add((byte)(widthBytes & 0xFF));
                        result.Add((byte)((widthBytes >> 8) & 0xFF));

                        // Alto en pixeles (Little Endian)
                        result.Add((byte)(height & 0xFF));
                        result.Add((byte)((height >> 8) & 0xFF));

                        for (int y = 0; y < height; y++)
                        {
                            for (int xByte = 0; xByte < widthBytes; xByte++)
                            {
                                byte value = 0;

                                for (int bit = 0; bit < 8; bit++)
                                {
                                    int x = (xByte * 8) + bit;

                                    if (x >= width)
                                        continue;

                                    Color pixel = bitmap.GetPixel(x, y);

                                    // Lógica estricta blanco y negro. Canal alpha incluido
                                    int gray = (pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000;
                                    
                                    // Si es oscuro y medianamente opaco, es negro. (0x80 >> bit)
                                    if (gray < 200 && pixel.A >= 128)
                                    {
                                        value |= (byte)(0x80 >> bit);
                                    }
                                }

                                result.Add(value);
                            }
                        }

                        return result.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                // Retornar arreglo vacío en caso de error para no tumbar la impresión
                Console.WriteLine("Error convirtiendo logo: " + ex.Message);
                return new byte[0];
            }
        }
    }
}
