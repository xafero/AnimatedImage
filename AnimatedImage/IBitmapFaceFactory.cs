using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedImage
{
    public interface IBitmapFaceFactory
    {
        public IBitmapFace Create(int width, int height);
    }
}
