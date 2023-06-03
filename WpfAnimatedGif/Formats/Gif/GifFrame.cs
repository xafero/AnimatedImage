using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifFrame : GifBlock
    {
        internal const int ImageSeparator = 0x2C;

        public GifImageDescriptor Descriptor { get; }
        public GifColor[]? LocalColorTable { get; }
        public IList<GifExtension> Extensions { get; }
        public GifImageData ImageData { get; }

        internal GifFrame(Stream stream, IEnumerable<GifExtension> controlExtensions)
        {
            // Note: at this point, the Image Separator (0x2C) has already been read

            Descriptor = new GifImageDescriptor(stream);
            LocalColorTable = Descriptor.HasLocalColorTable ?
                                GifHelpers.ReadColorTable(stream, Descriptor.LocalColorTableSize) :
                                null;

            ImageData = new GifImageData(this, stream);
            Extensions = controlExtensions.ToList().AsReadOnly();
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.GraphicRendering; }
        }
    }
}
