using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnimatedImage.Wpf
{
    internal class WriteableBitmapFace : IBitmapFace
    {
        public WriteableBitmap Bitmap { get; }

        public WriteableBitmapFace(int width, int height)
        {
            Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
        }

        public void ReadBGRA(byte[] buffer, int x, int y, int width, int height)
        {
            var bounds = new Int32Rect(x, y, Math.Min(width, Bitmap.PixelWidth - x), Math.Min(height, Bitmap.PixelHeight - y));
            Bitmap.CopyPixels(bounds, buffer, 4 * width, 0);
        }

        public void WriteBGRA(byte[] buffer, int x, int y, int width, int height)
        {
            var bounds = new Int32Rect(x, y, Math.Min(width, Bitmap.PixelWidth - x), Math.Min(height, Bitmap.PixelHeight - y));
            Bitmap.WritePixels(bounds, buffer, 4 * width, 0);
        }
    }

    internal class WriteableBitmapFaceFactory : IBitmapFaceFactory
    {
        public IBitmapFace Create(int width, int height)
            => new WriteableBitmapFace(width, height);
    }
}
