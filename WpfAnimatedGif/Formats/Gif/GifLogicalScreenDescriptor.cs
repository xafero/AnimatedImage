using System;
using System.IO;

namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifLogicalScreenDescriptor
    {
        public int Width { get; }
        public int Height { get; }
        public bool HasGlobalColorTable { get; }
        public int ColorResolution { get; }
        public bool IsGlobalColorTableSorted { get; }
        public int GlobalColorTableSize { get; }
        public int BackgroundColorIndex { get; }
        public double PixelAspectRatio { get; }

        internal GifLogicalScreenDescriptor(Stream stream)
        {
            byte[] bytes = new byte[7];
            stream.ReadAll(bytes, 0, bytes.Length);

            Width = BitConverter.ToUInt16(bytes, 0);
            Height = BitConverter.ToUInt16(bytes, 2);
            byte packedFields = bytes[4];
            HasGlobalColorTable = (packedFields & 0x80) != 0;
            ColorResolution = ((packedFields & 0x70) >> 4) + 1;
            IsGlobalColorTableSorted = (packedFields & 0x08) != 0;
            GlobalColorTableSize = 1 << (packedFields & 0x07) + 1;
            BackgroundColorIndex = bytes[5];
            PixelAspectRatio =
                bytes[5] == 0
                    ? 0.0
                    : (15 + bytes[5]) / 64.0;
        }
    }
}
