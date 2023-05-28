// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;
using System.IO.Compression;
using System.Linq;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal class fdATChunk : Chunk
    {
        public fdATChunk(byte[] bytes)
            : base(bytes)
        {
        }

        public fdATChunk(MemoryStream ms)
            : base(ms)
        {
        }

        public fdATChunk(Chunk chunk)
            : base(chunk)
        {
        }

        public uint SequenceNumber { get; private set; }

        public byte[] FrameData { get; private set; }

        protected override void ParseData(MemoryStream ms)
        {
            SequenceNumber = Helper.ConvertEndian(ms.ReadUInt32());
            FrameData = ms.ReadBytes((int)Length - 4);
        }

        public IDATChunk ToIDATChunk()
        {
            uint newCrc;
            using (var msCrc = new MemoryStream())
            {
                msCrc.WriteBytes(new[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' });
                msCrc.WriteBytes(FrameData);

                newCrc = CrcHelper.Calculate(msCrc.ToArray());
            }

            using (var ms = new MemoryStream())
            {
                ms.WriteUInt32(Helper.ConvertEndian(Length - 4));
                ms.WriteBytes(new[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' });
                ms.WriteBytes(FrameData);
                ms.WriteUInt32(Helper.ConvertEndian(newCrc));
                ms.Position = 0;

                return new IDATChunk(ms);
            }
        }

        public Stream Decompress()
        {
            var mem = new MemoryStream(FrameData, 8, FrameData.Length - 12);
            return new GZipStream(mem, CompressionMode.Decompress);
        }


    }
}