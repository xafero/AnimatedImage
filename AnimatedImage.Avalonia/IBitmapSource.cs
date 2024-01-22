using System;
using System.IO;

namespace AnimatedImage.Avalonia
{
    public interface IBitmapSource
    {
    }

    public class BitmapStream : IBitmapSource
    {
        public Stream StreamSource { get; }

        public BitmapStream(Stream stream) => StreamSource = stream;

        public static implicit operator BitmapStream(Stream stream) => new BitmapStream(stream);
    }

    public class BitmapUri : IBitmapSource
    {
        public Uri UriSource { get; }

        public BitmapUri(Uri source) => UriSource = source;

        public static implicit operator BitmapUri(Uri uri) => new BitmapUri(uri);
    }
}