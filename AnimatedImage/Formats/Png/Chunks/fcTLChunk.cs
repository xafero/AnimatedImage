// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System;
using AnimatedImage.Formats.Png.Types;

namespace AnimatedImage.Formats.Png.Chunks
{

    /// <summary>
    /// Frame Control Chunk
    /// </summary>
    internal class fcTLChunk
    {
        public const string ChunkType = "fcTL";

        internal fcTLChunk(ChunkStream cs)
        {
            SequenceNumber = cs.ReadUInt32();
            Width = cs.ReadUInt32();
            Height = cs.ReadUInt32();
            XOffset = cs.ReadUInt32();
            YOffset = cs.ReadUInt32();
            DelayNum = cs.ReadUInt16();
            DelayDen = cs.ReadUInt16();
            DisposeOp = (DisposeOps)cs.ReadByte();
            BlendOp = (BlendOps)cs.ReadByte();
            cs.ReadCrc();
        }

        /// <summary>
        ///     Sequence number of the animation chunk, starting from 0
        /// </summary>
        public uint SequenceNumber { get; }

        /// <summary>
        ///     Width of the following frame
        /// </summary>
        public uint Width { get; }

        /// <summary>
        ///     Height of the following frame
        /// </summary>
        public uint Height { get; }

        /// <summary>
        ///     X position at which to render the following frame
        /// </summary>
        public uint XOffset { get; }

        /// <summary>
        ///     Y position at which to render the following frame
        /// </summary>
        public uint YOffset { get; }

        /// <summary>
        ///     Frame delay fraction numerator
        /// </summary>
        public ushort DelayNum { get; }

        /// <summary>
        ///     Frame delay fraction denominator
        /// </summary>
        public ushort DelayDen { get; }

        /// <summary>
        ///     Type of frame area disposal to be done after rendering this frame
        /// </summary>
        public DisposeOps DisposeOp { get; }

        /// <summary>
        ///     Type of frame area rendering for this frame
        /// </summary>
        public BlendOps BlendOp { get; }

        /// <summary>
        ///     Compute delay time from DelayNum and DlayDen
        /// </summary>
        public TimeSpan ComputeDelay()
        {
            int deno = DelayDen == 0 ? 100 : DelayDen;
            return TimeSpan.FromSeconds(DelayNum / (double)deno);
        }
    }
}