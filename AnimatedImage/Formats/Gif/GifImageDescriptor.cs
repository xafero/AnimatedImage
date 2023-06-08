using System;
using System.IO;

namespace AnimatedImage.Formats.Gif
{
    internal class GifImageDescriptor
    {
        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }
        public bool HasLocalColorTable { get; }
        public bool Interlace { get; }
        public bool IsLocalColorTableSorted { get; }
        public int LocalColorTableSize { get; }

        internal GifImageDescriptor(Stream stream)
        {
            byte[] bytes = new byte[9];
            stream.ReadAll(bytes, 0, bytes.Length);
            Left = BitConverter.ToUInt16(bytes, 0);
            Top = BitConverter.ToUInt16(bytes, 2);
            Width = BitConverter.ToUInt16(bytes, 4);
            Height = BitConverter.ToUInt16(bytes, 6);
            byte packedFields = bytes[8];
            HasLocalColorTable = (packedFields & 0x80) != 0;
            Interlace = (packedFields & 0x40) != 0;
            IsLocalColorTableSorted = (packedFields & 0x20) != 0;
            LocalColorTableSize = 1 << (packedFields & 0x07) + 1;
        }
    }
}
