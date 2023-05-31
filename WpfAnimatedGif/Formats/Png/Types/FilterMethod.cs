using System;

namespace WpfAnimatedGif.Formats.Png.Types
{
    internal enum FilterMethod : byte
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4,
    }
}
