using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal abstract class tRNSChunk
    {
        internal static tRNSChunk Create(IHDRChunk ihdr, ChunkStream cs)
        {
            switch (ihdr.ColorType)
            {
                case Types.ColorType.Glayscale:
                    return new tRNSGrayscaleChunk(cs);

                case Types.ColorType.Color:
                    return new tRNSColorChunk(cs);

                case Types.ColorType.IndexedColor:
                    return new tRNSIndexChunk(cs);

                default:
                    throw new ArgumentException("unsupport color type: " + ihdr.ColorType);
            }
        }
    }

    internal class tRNSGrayscaleChunk : tRNSChunk
    {
        public ushort[] AlphaForEachGrayLevel { get; }

        internal tRNSGrayscaleChunk(ChunkStream cs)
        {
            AlphaForEachGrayLevel = new ushort[cs.Length / 2];

            for (var i = 0; i < AlphaForEachGrayLevel.Length; i++)
            {
                AlphaForEachGrayLevel[i] = cs.ReadUInt16();
            }

            cs.ReadCrc();
        }
    }

    internal class tRNSColorChunk : tRNSChunk
    {
        public PngColor[] TransparencyColors { get; }

        internal tRNSColorChunk(ChunkStream cs)
        {
            TransparencyColors = new PngColor[cs.Length / 3 / 2];

            for (var i = 0; i < TransparencyColors.Length; i++)
            {
                // TODO support 16bit color
                // this code may mistake a non-transparency color as a trasparency color.
                // for example If 0xFF00 is declared as transparency color,
                // 0xFF01 - 0xFFFF are treated as transparency color.

                TransparencyColors[i] =
                    new PngColor(
                        (byte)(cs.ReadUInt16() >> 8),
                        (byte)(cs.ReadUInt16() >> 8),
                        (byte)(cs.ReadUInt16() >> 8));
            }

            cs.ReadCrc();
        }
    }

    internal class tRNSIndexChunk : tRNSChunk
    {
        public byte[] AlphaForEachIndex { get; }

        internal tRNSIndexChunk(ChunkStream cs)
        {
            AlphaForEachIndex = new byte[cs.Length];

            for (var i = 0; i < cs.Length; i++)
            {
                AlphaForEachIndex[i] = (byte)cs.ReadByte();
            }

            cs.ReadCrc();
        }
    }
}
