using System.Collections.Generic;
using System.IO;

namespace WpfAnimatedGif.Formats.Gif
{
    internal abstract class GifExtension : GifBlock
    {
        internal const int ExtensionIntroducer = 0x21;

        internal static GifExtension ReadExtension(Stream stream, IEnumerable<GifExtension> controlExtensions)
        {
            // Note: at this point, the Extension Introducer (0x21) has already been read

            int label = stream.ReadByte();
            if (label < 0)
                throw GifHelpers.UnexpectedEndOfStreamException();
            switch (label)
            {
                case GifGraphicControlExtension.ExtensionLabel:
                    return new GifGraphicControlExtension(stream);
                case GifCommentExtension.ExtensionLabel:
                    return GifCommentExtension.ReadComment(stream);
                case GifPlainTextExtension.ExtensionLabel:
                    return new GifPlainTextExtension(stream, controlExtensions);
                case GifApplicationExtension.ExtensionLabel:
                    return new GifApplicationExtension(stream);
                default:
                    throw GifHelpers.UnknownExtensionTypeException(label);
            }
        }
    }
}
