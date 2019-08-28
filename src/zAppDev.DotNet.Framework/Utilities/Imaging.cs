using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class Imaging
    {

        public static List<byte> GetThumbnail(byte[] blob, int? width, int? height)
        {
            using (var image = ByteArrayToImage(blob))
            {
                return Resize(image, width, height);
            }
        }

        public static List<byte> GetThumbnail(string path, int? width, int? height)
        {
            using (var image = PathToImage(path))
            {
                return Resize(image, width, height);
            }
        }

        public static Bitmap ByteArrayToImage(byte[] source)
        {
            using (var ms = new MemoryStream(source))
            {
                return new Bitmap(ms);
            }
        }

        public static Bitmap PathToImage(string source)
        {
            using (var pngStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            {
                return new Bitmap(pngStream);
            }
        }

        public static List<byte> ImageToByteArray(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray().ToList();
            }
        }

        public static List<byte> Resize(Image image, int? width, int? height)
        {
            var resized = new Bitmap((width).GetValueOrDefault(0), (height).GetValueOrDefault(0));
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(image, 0, 0, width.GetValueOrDefault(0), height.GetValueOrDefault(0));
                return ImageToByteArray(resized);
            }
        }
    }
}
