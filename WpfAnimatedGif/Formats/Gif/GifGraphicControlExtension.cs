using System;
using System.IO;

namespace WpfAnimatedGif.Formats.Gif
{
    // label 0xF9
    internal class GifGraphicControlExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xF9;

        public int BlockSize { get; }
        public int DisposalMethod { get; }
        public bool UserInput { get; }
        public bool HasTransparency { get; }
        public int Delay { get; }
        public int TransparencyIndex { get; }

        internal GifGraphicControlExtension(Stream stream)
        {
            // Note: at this point, the label (0xF9) has already been read

            byte[] bytes = new byte[6];
            stream.ReadAll(bytes, 0, bytes.Length);
            BlockSize = bytes[0]; // should always be 4
            if (BlockSize != 4)
                throw GifHelpers.InvalidBlockSizeException("Graphic Control Extension", 4, BlockSize);
            byte packedFields = bytes[1];
            DisposalMethod = (packedFields & 0x1C) >> 2;
            UserInput = (packedFields & 0x02) != 0;
            HasTransparency = (packedFields & 0x01) != 0;
            Delay = BitConverter.ToUInt16(bytes, 2) * 10; // milliseconds
            TransparencyIndex = bytes[4];
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.Control; }
        }
    }
}
