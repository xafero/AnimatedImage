using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AnimatedImage.Formats;
using AnimatedImage.Formats.Gif;
using AnimatedImage.Formats.Png;

namespace AnimatedImage
{
    public abstract class FrameRenderer
    {
        public abstract int CurrentIndex { get; }

        public abstract int FrameCount { get; }

        public abstract int RepeatCount { get; }

        public abstract TimeSpan Duration { get; }

        public abstract IBitmapFace Current { get; }

        public abstract FrameRenderFrame this[int frameIndex] { get; }

        public void ProcessFrame(TimeSpan timespan)
        {
            while (timespan > Duration)
            {
                timespan -= Duration;
            }

            for (var frameIdx = 0; frameIdx < FrameCount; ++frameIdx)
            {
                var frame = this[frameIdx];
                if (frame.Begin <= timespan && timespan < frame.End)
                {
                    ProcessFrame(frameIdx);
                    return;
                }
            }
        }

        public abstract void ProcessFrame(int frameIndex);

        public abstract TimeSpan GetStartTime(int idx);

        public abstract FrameRenderer Clone();

        public static bool TryCreate(
            Stream stream,
            IBitmapFaceFactory factory,
#if !NETFRAMEWORK
            [MaybeNullWhen(false)]
#endif
            out FrameRenderer renderer)
        {
            stream.Position = 0;
            var magic = new byte[Signature.MaxLength];
            stream.Read(magic, 0, magic.Length);

            stream.Position = 0;
            if (Signature.IsGifSignature(magic))
            {
                var gif = new GifFile(stream);
                renderer = new GifRenderer(gif, factory);
                return true;
            }

            if (Signature.IsPngSignature(magic))
            {
                var png = new ApngFile(stream);

                renderer = new PngRenderer(png, factory);
                return true;
            }

            renderer = null!;
            return false;
        }

        private static class Signature
        {
            public static readonly byte[] GifHead = new byte[] { 0x47, 0x49, 0x46, 0x38 }; // GIF8
            public static readonly byte[] Png = ApngFrame.Signature;
            public static readonly int MaxLength = Math.Max(GifHead.Length + 2, Png.Length);

            public static bool IsGifSignature(byte[] signature)
            {
                if (signature.Length < 6)
                    return false;

                // GIF8
                for (var i = 0; i < GifHead.Length; ++i)
                    if (signature[i] != GifHead[i])
                        return false;

                // 8 or 9
                if (signature[4] != '8' && signature[4] != '9')
                    return false;

                // a
                if (signature[5] != 'a')
                    return false;

                return true;
            }

            public static bool IsPngSignature(byte[] signature)
            {
                if (signature.Length < Png.Length)
                    return false;

                for (var i = 0; i < Png.Length; ++i)
                    if (signature[i] != Png[i])
                        return false;

                return true;
            }
        }
    }

    public class FrameRenderFrame
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public TimeSpan Begin { get; }
        public TimeSpan End { get; }

        public FrameRenderFrame(int x, int y, int width, int height, TimeSpan begin, TimeSpan end)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Begin = begin;
            End = end;
        }

        public bool IsInvolve(FrameRenderFrame frame)
        {
            return X <= frame.X
                && Y <= frame.Y
                && frame.X + frame.Width <= X + Width
                && frame.Y + frame.Height <= Y + Height;
        }
    }
}
