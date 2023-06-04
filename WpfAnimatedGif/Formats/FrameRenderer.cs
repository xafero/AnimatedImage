using System;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WpfAnimatedGif.Formats.Gif;
using WpfAnimatedGif.Formats.Png;

namespace WpfAnimatedGif.Formats
{
    internal abstract class FrameRenderer
    {
        public abstract int CurrentIndex { get; }

        public abstract int FrameCount { get; }

        public abstract int RepeatCount { get; }

        public abstract TimeSpan Duration { get; }

        public abstract WriteableBitmap Current { get; }

        protected abstract FrameRenderFrame this[int frameIndex] { get; }

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

        public static bool TryCreate(BitmapSource image, IUriContext context, out FrameRenderer renderer)
        {
            if (image is BitmapFrame frame)
            {
                if (Uri.TryCreate(frame.BaseUri, frame.ToString(), out var uri))
                {
                    using var stream = Open(uri);

                    if (frame.Decoder is GifBitmapDecoder)
                    {
                        stream.Position = 0;
                        renderer = new GifRenderer(new GifFile(stream));
                        return true;
                    }

                    return TryCreate(stream, out renderer);
                }

                renderer = null;
                return false;
            }

            if (image is BitmapImage bmp)
            {
                if (bmp.StreamSource != null)
                {
                    return TryCreate(bmp.StreamSource, out renderer);
                }

                if (bmp.UriSource != null)
                {
                    var uri = bmp.UriSource;
                    if (!uri.IsAbsoluteUri)
                    {
                        var baseUri = bmp.BaseUri ?? context?.BaseUri;
                        if (baseUri != null)
                            uri = new Uri(baseUri, uri);
                    }

                    using var stream = Open(uri);
                    return TryCreate(stream, out renderer);
                }
            }

            renderer = null;
            return false;
        }

        private static bool TryCreate(Stream stream, out FrameRenderer renderer)
        {
            stream.Position = 0;
            var magic = new byte[Signature.MaxLength];
            stream.Read(magic, 0, magic.Length);

            stream.Position = 0;
            if (Signature.IsGifSignature(magic))
            {
                var gif = new GifFile(stream);
                renderer = new GifRenderer(gif);
                return true;
            }

            if (Signature.IsPngSignature(magic))
            {
                var png = new ApngFile(stream);

                renderer = new PngRenderer(png);
                return true;
            }

            renderer = null;
            return false;
        }

        private static Stream Open(Uri resourceUri)
        {
            var stream = OpenFirst(resourceUri);

            if (!stream.CanSeek)
            {
                var memstream = new MemoryStream();
                stream.CopyTo(memstream);
                return memstream;
            }
            return stream;

            static Stream OpenFirst(Uri uri)
            {
                if (uri.Scheme == PackUriHelper.UriSchemePack)
                {
                    StreamResourceInfo sri;
                    if (uri.Authority == "siteoforigin:,,,")
                        sri = Application.GetRemoteStream(uri);
                    else
                        sri = Application.GetResourceStream(uri);

                    if (sri != null)
                        return sri.Stream;
                }
                else
                {
                    WebClient wc = new WebClient();
                    return wc.OpenRead(uri);
                }

                return null;
            }
        }

        private static class Signature
        {
            public static readonly byte[] GifHead = new byte[] { 0x47, 0x49, 0x46, 0x38 }; // GIF8
            public static readonly byte[] Png = global::WpfAnimatedGif.Formats.Png.ApngFrame.Signature;
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

    internal class FrameRenderFrame
    {
        public int X => Bounds.X;
        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Int32Rect Bounds { get; }

        public TimeSpan Begin { get; }
        public TimeSpan End { get; }

        public FrameRenderFrame(int x, int y, int width, int height, TimeSpan begin, TimeSpan end)
        {
            Bounds = new Int32Rect(x, y, width, height);
            Begin = begin;
            End = end;
        }

        public bool IsInvolve(FrameRenderFrame frame)
        {
            return X <= frame.X
                && Y <= frame.Y
                && (frame.X + frame.Width) <= (X + Width)
                && (frame.Y + frame.Height) <= (Y + Height);
        }
    }
}
