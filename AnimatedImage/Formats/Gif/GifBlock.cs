using System.Collections.Generic;
using System.IO;

namespace AnimatedImage.Formats.Gif
{
    internal abstract class GifBlock
    {
        internal static GifBlock ReadBlock(Stream stream, IEnumerable<GifExtension> controlExtensions)
        {
            int blockId = stream.ReadByte();
            if (blockId < 0)
                throw GifHelpers.UnexpectedEndOfStreamException();
            switch (blockId)
            {
                case GifExtension.ExtensionIntroducer:
                    return GifExtension.ReadExtension(stream, controlExtensions);
                case GifFrame.ImageSeparator:
                    return new GifFrame(stream, controlExtensions);
                case GifTrailer.TrailerByte:
                    return new GifTrailer();
                default:
                    throw GifHelpers.UnknownBlockTypeException(blockId);
            }
        }

        internal abstract GifBlockKind Kind { get; }
    }
}
