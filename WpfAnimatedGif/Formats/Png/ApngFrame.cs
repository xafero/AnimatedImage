// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System.Collections.Generic;
using System.IO;
using WpfAnimatedGif.Formats.Png.Chunks;

namespace WpfAnimatedGif.Formats.Png
{
    /// <summary>
    ///     Describe a single frame.
    /// </summary>
    internal class ApngFrame
    {
        public static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        private List<IDATChunk> idatChunks = new List<IDATChunk>();

        public ApngFrame(IHDRChunk header)
        {
            IHDRChunk = header;
            fcTLChunk = null;
        }

        public ApngFrame(IHDRChunk header, fcTLChunk framecontrol)
        {
            IHDRChunk = header;
            fcTLChunk = framecontrol;
        }

        /// <summary>
        ///     Gets or Sets the acTL chunk
        /// </summary>
        public IHDRChunk IHDRChunk { get; }

        /// <summary>
        ///     Gets or Sets the fcTL chunk
        /// </summary>
        public fcTLChunk? fcTLChunk { get; }

        /// <summary>
        ///     Gets or Sets the IDAT chunks
        /// </summary>
        public List<IDATChunk> IDATChunks
        {
            get { return idatChunks; }
            set { idatChunks = value; }
        }

        /// <summary>
        ///     Add an IDAT Chunk to end end of existing list.
        /// </summary>
        public void AddIDATChunk(IDATChunk chunk)
        {
            idatChunks.Add(chunk);
        }

        // <summary>
        //     Gets the frame as PNG FileStream.
        // </summary>
        //        public MemoryStream GetStream()
        //        {
        //            var ihdrChunk = new IHDRChunk(IHDRChunk);
        //            if (fcTLChunk != null)
        //            {
        //                // Fix frame size with fcTL data.
        //                ihdrChunk.ModifyChunkData(0, Helper.ConvertEndian(fcTLChunk.Width));
        //                ihdrChunk.ModifyChunkData(4, Helper.ConvertEndian(fcTLChunk.Height));
        //            }
        //
        //            // Write image data
        //            using (var ms = new MemoryStream())
        //            {
        //                ms.WriteBytes(Signature);
        //                ms.WriteBytes(ihdrChunk.RawData);
        //                otherChunks.ForEach(o => ms.WriteBytes(o.RawData));
        //                idatChunks.ForEach(i => ms.WriteBytes(i.RawData));
        //                ms.WriteBytes(IENDChunk.RawData);
        //
        //                ms.Position = 0;
        //                return ms;
        //            }
        //        }
    }
}