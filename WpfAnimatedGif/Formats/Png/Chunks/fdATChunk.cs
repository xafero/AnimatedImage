// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    /// <summary>
    /// Fragment Data Chunk
    /// </summary>
    internal class fdATChunk
    {
        public const string ChunkType = "fdAT";

        internal fdATChunk(ChunkStream cs)
        {
            SequenceNumber = cs.ReadUInt32();
            FrameData = cs.ReadBytes((int)cs.Length - 4);
            cs.ReadCrc();
        }

        public uint SequenceNumber { get; }
        public byte[] FrameData { get; }

        public IDATChunk ToIDATChunk()
        {
            return new IDATChunk(FrameData);
        }
    }
}