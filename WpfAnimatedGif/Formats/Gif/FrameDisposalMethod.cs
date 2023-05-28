using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnimatedGif.Formats.Gif
{
    internal enum FrameDisposalMethod
    {
        None = 0,
        DoNotDispose = 1,
        RestoreBackground = 2,
        RestorePrevious = 3
    }
}
