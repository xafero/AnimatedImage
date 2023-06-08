namespace AnimatedImage.Formats.Gif
{
    internal struct GifColor
    {
        public byte B { get; }
        public byte G { get; }
        public byte R { get; }

        internal GifColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override string ToString()
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", R, G, B);
        }
    }
}
