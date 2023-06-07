using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WpfAnimatedGif.Formats.Gif;
using WpfAnimatedGif.Formats.Png;

namespace WpfAnimatedGif.Formats
{
    internal abstract class FrameRenderer
    {
#if !NETFRAMEWORK
        private static readonly System.Net.Http.HttpClient s_client = new();
#endif

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

        public static bool TryCreate(
            BitmapSource image,
            IUriContext context,
#if !NETFRAMEWORK
            [MaybeNullWhen(false)]
#endif
            out FrameRenderer renderer)
        {
            if (image is BitmapFrame frame)
            {
                if (Uri.TryCreate(frame.BaseUri, frame.ToString(), out var uri)
                    && TryOpen(uri, out var stream))
                {
                    using (stream)
                    {
                        if (frame.Decoder is GifBitmapDecoder)
                        {
                            stream.Position = 0;
                            renderer = new GifRenderer(new GifFile(stream));
                            return true;
                        }

                        return TryCreate(stream, out renderer);
                    }
                }

                renderer = null!;
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

                    if (TryOpen(uri, out var stream))
                        using (stream)
                            return TryCreate(stream, out renderer);
                }
            }

            renderer = null!;
            return false;
        }

        private static bool TryCreate(
            Stream stream,
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
                renderer = new GifRenderer(gif);
                return true;
            }

            if (Signature.IsPngSignature(magic))
            {
                var png = new ApngFile(stream);

                renderer = new PngRenderer(png);
                return true;
            }

            renderer = null!;
            return false;
        }

        private static bool TryOpen(
            Uri resourceUri,
#if !NETFRAMEWORK
            [MaybeNullWhen(false)]
#endif
            out Stream strm
            )
        {
            var stream = OpenFirst(resourceUri);
            if (stream is null)
            {
                strm = null!;
                return false;
            }

            if (stream.CanSeek)
            {
                strm = stream;
                return true;
            }

            var memstream = new MemoryStream();
            stream.CopyTo(memstream);
            strm = memstream;
            return true;


            static Stream? OpenFirst(Uri uri)
            {
                switch (uri.Scheme)
                {
                    case "pack":
                        StreamResourceInfo sri;
                        if (uri.Authority == "siteoforigin:,,,")
                            sri = Application.GetRemoteStream(uri);
                        else
                            sri = Application.GetResourceStream(uri);

                        if (sri != null)
                            return sri.Stream;
                        break;

#if NETFRAMEWORK
                    case "http":
                    case "https":
                    case "file":
                    case "ftp":
                        var wc = new WebClient();
                        return wc.OpenRead(uri);
#else
                    case "http":
                    case "https":
                        return s_client.GetStreamAsync(uri).Result;

                    case "file":
                        return File.OpenRead(uri.LocalPath);
#endif
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
