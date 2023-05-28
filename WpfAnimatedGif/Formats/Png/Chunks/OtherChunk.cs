// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    public class OtherChunk : Chunk
    {
        public OtherChunk(byte[] bytes)
            : base(bytes)
        {
        }

        public OtherChunk(MemoryStream ms)
            : base(ms)
        {
        }

        public OtherChunk(Chunk chunk)
            : base(chunk)
        {
        }

        protected override void ParseData(MemoryStream ms)
        {
        }
    }
}