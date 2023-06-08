using System;

namespace AnimatedImage.Formats.Png.Types
{
    internal enum ColorType : byte
    {
        Glayscale = 0,
        Color = 2,
        IndexedColor = 3,
        GlayscaleAlpha = 4,
        ColorAlpha = 6,
    }
}
