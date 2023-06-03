// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    /// <summary>
    /// Animation Control Chunk
    /// </summary>
    internal class acTLChunk
    {
        internal acTLChunk(ChunkStream cs)
        {
            NumFrames = cs.ReadUInt32();
            NumPlays = cs.ReadUInt32();
            cs.ReadCrc();
        }

        public uint NumFrames { get; private set; }

        public uint NumPlays { get; private set; }
    }
}