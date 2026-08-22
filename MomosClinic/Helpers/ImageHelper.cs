using System.Drawing;
using System.IO;

namespace MomosClinic.Helpers
{
    public static class ImageHelper
    {
        public static Image LoadImageWithoutLock(string path)
        {
            if (!File.Exists(path)) return null;
            byte[] bytes = File.ReadAllBytes(path);
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
