// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WpfAnimatedGif.Formats.Png.Chunks;

namespace WpfAnimatedGif.Formats.Png
{
    internal class ApngFile
    {
        private readonly ApngFrame defaultImage = new ApngFrame();
        private readonly List<ApngFrame> frames = new List<ApngFrame>();

        public ApngFile(string fileName) : this(File.ReadAllBytes(fileName))
        { }

        public ApngFile(byte[] fileBytes) : this(new MemoryStream(fileBytes))
        { }


        public ApngFile(Stream stream)
        {
            // check file signature.
            if (!Helper.IsBytesEqual(stream.ReadBytes(ApngFrame.Signature.Length), ApngFrame.Signature))
                throw new Exception("File signature incorrect.");


            var chunkStream = new ChunkStream(stream);

            if (chunkStream.ReadChunkType() != "IHDR")
                throw new Exception("IHDR chunk must located before any other chunks.");

            // Read IHDR chunk.
            IHDRChunk = new IHDRChunk(chunkStream);

            // Now let's loop in chunks
            ApngFrame frame = null;
            var otherChunks = new List<Chunk>();
            bool isIDATAlreadyParsed = false;
            bool isIENDParsed = false;

            do
            {
                var chunkType = chunkStream.ReadChunkType();

                Debug.Print(chunkType);
                switch (chunkType)
                {
                    case null:
                        throw new Exception("IEND chunk expected.");

                    case IHDRChunk.ChunkType:
                        throw new Exception("Only single IHDR is allowed.");

                    case acTLChunk.ChunkType:
                        if (IsSimplePNG)
                            throw new Exception("acTL chunk must located before any IDAT and fdAT");

                        acTLChunk = new acTLChunk(chunkStream);
                        break;

                    case IDATChunk.ChunkType:
                        // To be an APNG, acTL must located before any IDAT and fdAT.
                        if (acTLChunk == null)
                            IsSimplePNG = true;

                        // Only default image has IDAT.
                        defaultImage.IHDRChunk = IHDRChunk;
                        defaultImage.AddIDATChunk(new IDATChunk(chunkStream));
                        isIDATAlreadyParsed = true;
                        break;

                    case fcTLChunk.ChunkType:
                        // Simple PNG should ignore this.
                        if (IsSimplePNG)
                            continue;

                        if (frame != null && frame.IDATChunks.Count == 0)
                            throw new Exception("One frame must have only one fcTL chunk.");

                        // IDAT already parsed means this fcTL is used by FRAME IMAGE.
                        if (isIDATAlreadyParsed)
                        {
                            // register current frame object and build a new frame object
                            // for next use
                            if (frame != null)
                                frames.Add(frame);
                            frame = new ApngFrame
                            {
                                IHDRChunk = IHDRChunk,
                                fcTLChunk = new fcTLChunk(chunkStream)
                            };
                        }
                        // Otherwise this fcTL is used by the DEFAULT IMAGE.
                        else
                        {
                            defaultImage.fcTLChunk = new fcTLChunk(chunkStream);
                        }
                        break;

                    case fdATChunk.ChunkType:
                        // Simple PNG should ignore this.
                        if (IsSimplePNG)
                            continue;

                        // fdAT is only used by frame image.
                        if (frame == null || frame.fcTLChunk == null)
                            throw new Exception("fcTL chunk expected.");

                        frame.AddIDATChunk(new fdATChunk(chunkStream).ToIDATChunk());
                        break;

                    case PLTEChunk.ChunkType:
                        PLTEChunk = new PLTEChunk(chunkStream);
                        break;

                    case tRNSChunk.ChunkType:
                        if (IHDRChunk is null)
                        {
                            throw new Exception("IHDR chunk expected");
                        }

                        tRNSChunk = tRNSChunk.Create(IHDRChunk, chunkStream);
                        break;

                    case IENDChunk.ChunkType:
                        // register last frame object
                        if (frame != null)
                            frames.Add(frame);

                        var endChunk = new IENDChunk(chunkStream);

                        if (DefaultImage.IDATChunks.Count != 0)
                            DefaultImage.IENDChunk = endChunk;
                        foreach (ApngFrame f in frames)
                        {
                            f.IENDChunk = endChunk;
                        }
                        isIENDParsed = true;
                        break;

                    default:
                        otherChunks.Add(new Chunk(chunkStream));
                        break;
                }

            } while (!isIENDParsed);

            // We have one more thing to do:
            // If the default image if part of the animation,
            // we should insert it into frames list.
            if (defaultImage.fcTLChunk != null)
            {
                frames.Insert(0, defaultImage);
                DefaultImageIsAnimated = true;
            }

            // Now we should apply every chunk in otherChunks to every frame.
            frames.ForEach(f => otherChunks.ForEach(f.AddOtherChunk));
        }

        /// <summary>
        ///     Indicate whether the file is a simple PNG.
        /// </summary>
        public bool IsSimplePNG { get; private set; }

        /// <summary>
        ///     Indicate whether the default image is part of the animation
        /// </summary>
        public bool DefaultImageIsAnimated { get; private set; }

        /// <summary>
        ///     Gets the base image.
        ///     If IsSimplePNG = True, returns the only image;
        ///     if False, returns the default image
        /// </summary>
        public ApngFrame DefaultImage
        {
            get { return defaultImage; }
        }

        /// <summary>
        ///     Gets the frame array.
        ///     If IsSimplePNG = True, returns empty
        /// </summary>
        public ApngFrame[] Frames
        {
            get { return frames.ToArray(); }
        }

        /// <summary>
        ///     Gets the IHDR Chunk
        /// </summary>
        public IHDRChunk IHDRChunk { get; private set; }

        /// <summary>
        ///     Gets the acTL Chunk
        /// </summary>
        public acTLChunk? acTLChunk { get; private set; }

        public PLTEChunk? PLTEChunk { get; private set; }

        public tRNSChunk? tRNSChunk { get; private set; }
    }
}