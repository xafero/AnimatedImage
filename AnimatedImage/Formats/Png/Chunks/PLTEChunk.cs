using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedImage.Formats.Png.Chunks
{
    /// <summary>
    /// Color Palette Chunk
    /// </summary>
    internal class PLTEChunk
    {
        public const string ChunkType = "PLTE";

        public PngColor[] Colors { get; }

        internal PLTEChunk(ChunkStream cs)
        {
            int length = (int)cs.Length / 3;

            Colors = new PngColor[length];

            for (var i = 0; i < length; ++i)
            {
                Colors[i] = new PngColor(
                    (byte)cs.ReadByte(),
                    (byte)cs.ReadByte(),
                    (byte)cs.ReadByte());
            }

            cs.ReadCrc();
        }

    }

    internal struct PngColor
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public PngColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
