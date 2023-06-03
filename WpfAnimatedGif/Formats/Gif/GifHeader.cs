using System.IO;

namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifHeader : GifBlock
    {
        public string Signature { get; }
        public string Version { get; }
        public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; }

        internal GifHeader(Stream stream)
        {
            Signature = GifHelpers.ReadString(stream, 3);
            if (Signature != "GIF")
                throw GifHelpers.InvalidSignatureException(Signature);
            Version = GifHelpers.ReadString(stream, 3);
            if (Version != "87a" && Version != "89a")
                throw GifHelpers.UnsupportedVersionException(Version);
            LogicalScreenDescriptor = new GifLogicalScreenDescriptor(stream);
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.Other; }
        }
    }
}
