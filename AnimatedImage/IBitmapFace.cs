using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedImage
{
    public interface IBitmapFace
    {
        public void WriteBGRA(byte[] buffer, int x, int y, int width, int height);
        public void ReadBGRA(byte[] buffer, int x, int y, int width, int height);
    }
}
