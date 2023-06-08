using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AnimatedImage.Formats.Gif
{
    // label 0x01
    internal class GifPlainTextExtension : GifExtension
    {
        internal const int ExtensionLabel = 0x01;

        public int BlockSize { get; }
        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }
        public int CellWidth { get; }
        public int CellHeight { get; }
        public int ForegroundColorIndex { get; }
        public int BackgroundColorIndex { get; }
        public string Text { get; }

        public IList<GifExtension> Extensions { get; }

        internal GifPlainTextExtension(Stream stream, IEnumerable<GifExtension> controlExtensions)
        {
            // Note: at this point, the label (0x01) has already been read

            byte[] bytes = new byte[13];
            stream.ReadAll(bytes, 0, bytes.Length);

            BlockSize = bytes[0];
            if (BlockSize != 12)
                throw GifHelpers.InvalidBlockSizeException("Plain Text Extension", 12, BlockSize);

            Left = BitConverter.ToUInt16(bytes, 1);
            Top = BitConverter.ToUInt16(bytes, 3);
            Width = BitConverter.ToUInt16(bytes, 5);
            Height = BitConverter.ToUInt16(bytes, 7);
            CellWidth = bytes[9];
            CellHeight = bytes[10];
            ForegroundColorIndex = bytes[11];
            BackgroundColorIndex = bytes[12];

            var dataBytes = GifHelpers.ReadDataBlocks(stream);
            Text = Encoding.ASCII.GetString(dataBytes);
            Extensions = controlExtensions.ToList().AsReadOnly();
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.GraphicRendering; }
        }
    }
}
