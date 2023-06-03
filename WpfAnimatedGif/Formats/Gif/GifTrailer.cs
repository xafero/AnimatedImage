namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifTrailer : GifBlock
    {
        internal const int TrailerByte = 0x3B;

        internal GifTrailer()
        {
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.Other; }
        }
    }
}
