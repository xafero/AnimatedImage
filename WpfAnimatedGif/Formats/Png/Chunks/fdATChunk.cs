// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;
using System.IO.Compression;
using System.Linq;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal class fdATChunk
    {
        internal fdATChunk(ChunkStream cs)
        {
            SequenceNumber = cs.ReadUInt32();
            FrameData = cs.ReadBytes((int)cs.Length - 4);
            cs.ReadCrc();
        }

        public uint SequenceNumber { get; private set; }
        public byte[] FrameData { get; private set; }

        public IDATChunk ToIDATChunk()
        {
            return new IDATChunk(FrameData);
        }
    }
}