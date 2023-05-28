// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    public class IDATChunk : Chunk
    {
        public IDATChunk(byte[] bytes)
            : base(bytes)
        {
        }

        public IDATChunk(MemoryStream ms)
            : base(ms)
        {
        }

        public IDATChunk(Chunk chunk)
            : base(chunk)
        {
        }
    }
}