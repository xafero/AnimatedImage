using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    public class PLTEChunk : Chunk
    {
        private PngColor[] _colors;
        public PngColor[] Colors => _colors;

        public PLTEChunk(byte[] bytes)
            : base(bytes)
        {
        }

        public PLTEChunk(MemoryStream ms)
            : base(ms)
        {
        }

        public PLTEChunk(Chunk chunk)
            : base(chunk)
        {
        }

        protected override void ParseData(MemoryStream ms)
        {
            int length = ms.ReadInt32() / 3;
            ms.Position += 4;

            _colors = new PngColor[length];

            for (var i = 0; i < length; ++i)
            {
                _colors[i] = new PngColor(
                    (byte)ms.ReadByte(),
                    (byte)ms.ReadByte(),
                    (byte)ms.ReadByte());
            }
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
