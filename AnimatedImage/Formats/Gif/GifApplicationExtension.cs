using System;
using System.IO;
using System.Text;

namespace AnimatedImage.Formats.Gif
{
    // label 0xFF
    internal class GifApplicationExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xFF;

        public int BlockSize { get; }
        public string ApplicationIdentifier { get; }
        public byte[] AuthenticationCode { get; }
        public byte[] Data { get; }

        internal GifApplicationExtension(Stream stream)
        {
            // Note: at this point, the label (0xFF) has already been read

            byte[] bytes = new byte[12];
            stream.ReadAll(bytes, 0, bytes.Length);
            BlockSize = bytes[0]; // should always be 11
            if (BlockSize != 11)
                throw GifHelpers.InvalidBlockSizeException("Application Extension", 11, BlockSize);

            ApplicationIdentifier = Encoding.ASCII.GetString(bytes, 1, 8);
            byte[] authCode = new byte[3];
            Array.Copy(bytes, 9, authCode, 0, 3);
            AuthenticationCode = authCode;
            Data = GifHelpers.ReadDataBlocks(stream);
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.SpecialPurpose; }
        }
    }
}
