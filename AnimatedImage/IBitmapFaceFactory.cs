using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedImage
{
    /// <summary>
    /// The wrapper for image creation.
    /// </summary>
    public interface IBitmapFaceFactory
    {
        /// <summary>
        /// Creates the blank image with the specified area.
        /// </summary>
        /// <param name="width">The width of the specified area.</param>
        /// <param name="height">The width of the specified area.</param>
        /// <returns>The wrapper for image rendering.</returns>
        public IBitmapFace Create(int width, int height);
    }
}
