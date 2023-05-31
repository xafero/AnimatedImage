// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.IO;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal class IENDChunk
    {
        internal IENDChunk(ChunkStream cs)
        {
            cs.ReadCrc();
        }

    }
}