using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal class PLTEChunk
    {
        private PngColor[] _colors;
        public PngColor[] Colors => _colors;

        internal PLTEChunk(ChunkStream cs)
        {
            int length = (int)cs.Length / 3;

            _colors = new PngColor[length];

            for (var i = 0; i < length; ++i)
            {
                _colors[i] = new PngColor(
                    (byte)cs.ReadByte(),
                    (byte)cs.ReadByte(),
                    (byte)cs.ReadByte());
            }

            cs.ReadCrc();
        }

    }

    public struct PngColor
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
