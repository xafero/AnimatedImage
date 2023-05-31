// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;
using System.Linq;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal class IDATChunk
    {
        internal IDATChunk(byte[] framedata)
        {
            FrameData = framedata;
        }

        internal IDATChunk(ChunkStream cs)
        {
            FrameData = cs.ReadBytes((int)cs.Length);
            cs.ReadCrc();
        }

        public byte[] FrameData { get; private set; }
    }
}