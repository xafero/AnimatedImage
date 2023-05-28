using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    public abstract class tRNSChunk : Chunk
    {
        public tRNSChunk(byte[] bytes)
            : base(bytes)
        { }

        public tRNSChunk(MemoryStream ms)
            : base(ms)
        { }

        public tRNSChunk(Chunk chunk)
            : base(chunk)
        { }


        public static tRNSChunk Create(IHDRChunk ihdr, Chunk chunk)
        {
            switch (ihdr.ColorType)
            {
                case Types.ColorType.Glayscale:
                    return new tRNSGrayscaleChunk(chunk);

                case Types.ColorType.Color:
                    return new tRNSColorChunk(chunk);

                case Types.ColorType.IndexedColor:
                    return new tRNSIndexChunk(chunk);


                default:
                    return null;
            }
        }
    }

    public class tRNSGrayscaleChunk : tRNSChunk
    {
        public ushort[] AlphaForEachGrayLevel { private set; get; }

        public tRNSGrayscaleChunk(Chunk chunk) : base(chunk)
        { }

        protected override void ParseData(MemoryStream ms)
        {
            AlphaForEachGrayLevel = new ushort[Length / 2];

            for (var i = 0; i < AlphaForEachGrayLevel.Length; i++)
            {
                AlphaForEachGrayLevel[i] = Helper.ConvertEndian(ms.ReadUInt16());
            }
        }
    }

    public class tRNSColorChunk : tRNSChunk
    {
        public PngColor[] TransparencyColors { private set; get; }

        public tRNSColorChunk(Chunk chunk) : base(chunk)
        { }

        protected override void ParseData(MemoryStream ms)
        {
            TransparencyColors = new PngColor[Length / 3 / 2];

            for (var i = 0; i < TransparencyColors.Length; i++)
            {
                // TODO support 16bit color
                // this code may mistake a non-transparency color as a trasparency color.
                // for example If 0xFF00 is declared as transparency color,
                // 0xFF01 - 0xFFFF are treated as transparency color.

                TransparencyColors[i] =
                    new PngColor(
                        (byte)(Helper.ConvertEndian(ms.ReadUInt16()) >> 8),
                        (byte)(Helper.ConvertEndian(ms.ReadUInt16()) >> 8),
                        (byte)(Helper.ConvertEndian(ms.ReadUInt16()) >> 8));
            }
        }
    }

    public class tRNSIndexChunk : tRNSChunk
    {
        public byte[] AlphaForEachIndex { private set; get; }

        public tRNSIndexChunk(Chunk chunk) : base(chunk)
        { }

        protected override void ParseData(MemoryStream ms)
        {
            AlphaForEachIndex = new byte[Length];

            for (var i = 0; i < Length; i++)
            {
                AlphaForEachIndex[i] = (byte)ms.ReadByte();
            }
        }
    }
}
