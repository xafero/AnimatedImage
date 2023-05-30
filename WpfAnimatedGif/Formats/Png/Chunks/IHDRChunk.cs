// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System;
using System.IO;
using System.Linq;
using WpfAnimatedGif.Formats.Png.Types;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    public class IHDRChunk : Chunk
    {
        public IHDRChunk(byte[] chunkBytes)
            : base(chunkBytes)
        {
        }

        public IHDRChunk(Stream stream)
            : base(stream)
        {
        }

        public IHDRChunk(Chunk chunk)
            : base(chunk)
        {
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public byte BitDepth { get; private set; }

        public ColorType ColorType { get; private set; }

        public CompressionMethod CompressionMethod { get; private set; }

        public FilterMethod FilterMethod { get; private set; }

        public InterlaceMethod InterlaceMethod { get; private set; }

        public bool IsValidHeader
        {
            get
            {
                if (!IsValid(CompressionMethod))
                    return false;

                if (!IsValid(ColorType))
                    return false;

                if (!IsValid(CompressionMethod))
                    return false;

                if (!IsValid(FilterMethod))
                    return false;

                if (!IsValid(InterlaceMethod))
                    return false;

                // ColorType & BitDepth combination
                if (!IsValidBitDepth)
                    return false;

                return true;

                static bool IsValid<T>(T val) where T : Enum
                {
                    return Enum.GetValues(typeof(T))
                               .Cast<T>()
                               .Any(v => Equals(v, val));
                }
            }
        }

        public bool IsValidBitDepth
        {
            get
            {
                byte[] allowDepths;

                switch (ColorType)
                {
                    case ColorType.Glayscale:
                        allowDepths = new byte[] { 1, 2, 4, 8, 16 };
                        break;
                    case ColorType.IndexedColor:
                        allowDepths = new byte[] { 1, 2, 4, 8 };
                        break;
                    case ColorType.Color:
                    case ColorType.GlayscaleAlpha:
                    case ColorType.ColorAlpha:
                        allowDepths = new byte[] { 8, 16 };
                        break;

                    default:
                        return false;
                };

                return allowDepths.Any(b => b == BitDepth);
            }
        }

        protected override void ParseData(MemoryStream ms)
        {
            Width = Helper.ConvertEndian(ms.ReadInt32());
            Height = Helper.ConvertEndian(ms.ReadInt32());
            BitDepth = Convert.ToByte(ms.ReadByte());
            ColorType = (ColorType)Convert.ToByte(ms.ReadByte());
            CompressionMethod = (CompressionMethod)Convert.ToByte(ms.ReadByte());
            FilterMethod = (FilterMethod)Convert.ToByte(ms.ReadByte());
            InterlaceMethod = (InterlaceMethod)Convert.ToByte(ms.ReadByte());
        }
    }
}