using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AnimatedImage.Avalonia
{
    internal class WriteableBitmapFace : IBitmapFace
    {
        public IImage Bitmap
        {
            get
            {
                _flg = !_flg;
                Send();
                return Current;
            }
        }

        public IImage Current => _flg ? _tick : _tack;

        private bool _flg = false;
        private readonly PixelSize _size;
        private readonly WriteableBitmap _buffer;
        private readonly WriteableBitmap _bitmap;
        public readonly IImage _tick;
        public readonly IImage _tack;

        public WriteableBitmapFace(int width, int height)
        {
            _size = new PixelSize(width, height);
            var dpi = new Vector(96, 96);
            _buffer = new WriteableBitmap(_size, dpi, PixelFormats.Bgra8888, null);
            _bitmap = new WriteableBitmap(_size, dpi, PixelFormats.Bgra8888, null);

            _tick = new BitmapWrapper(_bitmap);
            _tack = new BitmapWrapper(_bitmap);
        }

        public unsafe void ReadBGRA(byte[] buffer, int x, int y, int width, int height)
        {
            if (width * height * 4 > buffer.Length)
                throw new IndexOutOfRangeException();

            using var bit = _buffer.Lock();

            byte* ptr = (byte*)(void*)(bit.Address);
            ptr += y * bit.RowBytes + x * 4;

            fixed (byte* buffer0 = &buffer[0])
            {
                byte* bufferPtr = buffer0;

                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width * 4; ++j)
                    {
                        bufferPtr[0] = ptr[j];
                        bufferPtr++;
                    }
                    ptr += bit.RowBytes;
                }
            }
        }

        public unsafe void WriteBGRA(byte[] buffer, int x, int y, int width, int height)
        {
            if (width * height * 4 > buffer.Length)
                throw new IndexOutOfRangeException();

            using var bit = _buffer.Lock();

            byte* ptr = (byte*)(void*)(bit.Address);
            ptr += y * bit.RowBytes + x * 4;

            fixed (byte* buffer0 = &buffer[0])
            {
                byte* bufferPtr = buffer0;

                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width * 4; ++j)
                    {
                        ptr[j] = bufferPtr[0];
                        bufferPtr++;
                    }

                    ptr += bit.RowBytes;
                }
            }
        }

        private void Send()
        {
            using var fBit = _bitmap.Lock();
            _buffer.CopyPixels(
                new PixelRect(_size),
                fBit.Address,
                fBit.RowBytes * _size.Height,
                fBit.RowBytes);
        }

        class BitmapWrapper : IImage
        {
            private IImage _bitmap;

            public BitmapWrapper(WriteableBitmap bitmap)
            {
                _bitmap = bitmap;
            }

            public Size Size
                => _bitmap.Size;

            public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
                => _bitmap.Draw(context, sourceRect, destRect);
        }
    }

    internal class WriteableBitmapFaceFactory : IBitmapFaceFactory
    {
        public IBitmapFace Create(int width, int height) => new WriteableBitmapFace(width, height);
    }
}
