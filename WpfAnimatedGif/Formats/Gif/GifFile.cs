using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifFile
    {
        public GifHeader Header { get; }
        public GifColor[]? GlobalColorTable { get; }
        public IList<GifFrame> Frames { get; }
        public IList<GifExtension> Extensions { get; }
        public ushort RepeatCount { get; }

        internal GifFile(Stream stream)
        {
            Header = new GifHeader(stream);

            GlobalColorTable = Header.LogicalScreenDescriptor.HasGlobalColorTable ?
                                GifHelpers.ReadColorTable(stream, Header.LogicalScreenDescriptor.GlobalColorTableSize) :
                                null;

            var frames = new List<GifFrame>();
            var controlExtensions = new List<GifExtension>();
            var specialExtensions = new List<GifExtension>();
            while (true)
            {
                var block = GifBlock.ReadBlock(stream, controlExtensions);

                if (block.Kind == GifBlockKind.GraphicRendering)
                    controlExtensions = new List<GifExtension>();

                if (block is GifFrame)
                {
                    frames.Add((GifFrame)block);
                }
                else if (block is GifExtension)
                {
                    var extension = (GifExtension)block;
                    switch (extension.Kind)
                    {
                        case GifBlockKind.Control:
                            controlExtensions.Add(extension);
                            break;
                        case GifBlockKind.SpecialPurpose:
                            specialExtensions.Add(extension);
                            break;
                    }
                }
                else if (block is GifTrailer)
                {
                    break;
                }
            }

            Frames = frames.AsReadOnly();
            Extensions = specialExtensions.AsReadOnly();

            var netscapeExtension =
                            Extensions
                                .OfType<GifApplicationExtension>()
                                .FirstOrDefault(GifHelpers.IsNetscapeExtension);

            if (netscapeExtension != null)
                RepeatCount = GifHelpers.GetRepeatCount(netscapeExtension);
            else
                RepeatCount = 1;
        }
    }
}
