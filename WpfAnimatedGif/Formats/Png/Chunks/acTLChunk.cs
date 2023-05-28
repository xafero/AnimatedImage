// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    /// <summary>
    /// Animation Control Chunk
    /// </summary>
    public class acTLChunk : Chunk
    {
        public acTLChunk(byte[] bytes)
            : base(bytes)
        {
        }

        public acTLChunk(MemoryStream ms)
            : base(ms)
        {
        }

        public acTLChunk(Chunk chunk)
            : base(chunk)
        {
        }

        public uint NumFrames { get; private set; }

        public uint NumPlays { get; private set; }

        protected override void ParseData(MemoryStream ms)
        {
            NumFrames = Helper.ConvertEndian(ms.ReadUInt32());
            NumPlays = Helper.ConvertEndian(ms.ReadUInt32());
        }
    }
}