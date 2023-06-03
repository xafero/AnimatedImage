using System.IO;
using System.Text;

namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifCommentExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xFE;

        public string? Text { get; }

        private GifCommentExtension(Stream stream)
        {
            // Note: at this point, the label (0xFE) has already been read

            var bytes = GifHelpers.ReadDataBlocks(stream);

            Text = bytes is not null ?
                    Encoding.ASCII.GetString(bytes) :
                    null;
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.SpecialPurpose; }
        }

        internal static GifCommentExtension ReadComment(Stream stream)
        {
            return new GifCommentExtension(stream);
        }
    }
}
