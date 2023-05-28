// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    public class IENDChunk : Chunk
    {
        public IENDChunk(byte[] bytes)
            : base(bytes)
        {
        }

        public IENDChunk(MemoryStream ms)
            : base(ms)
        {
        }

        public IENDChunk(Chunk chunk)
            : base(chunk)
        {
        }
    }
}