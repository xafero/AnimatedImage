// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System;
using System.IO;
using System.Linq;
using AnimatedImage.Formats.Png.Types;

namespace AnimatedImage.Formats.Png.Chunks
{
    internal class IHDRChunk
    {
        public const string ChunkType = "IHDR";

        /// <summary>
        /// Image Header Chunk
        /// </summary>
        /// <param name="cs"></param>
        internal IHDRChunk(ChunkStream cs)
        {
            Width = cs.ReadInt32();
            Height = cs.ReadInt32();
            BitDepth = cs.ReadByte();
            ColorType = (ColorType)Convert.ToByte(cs.ReadByte());
            CompressionMethod = (CompressionMethod)Convert.ToByte(cs.ReadByte());
            FilterMethod = (FilterMethod)Convert.ToByte(cs.ReadByte());
            InterlaceMethod = (InterlaceMethod)Convert.ToByte(cs.ReadByte());
            cs.ReadCrc();
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
    }
}