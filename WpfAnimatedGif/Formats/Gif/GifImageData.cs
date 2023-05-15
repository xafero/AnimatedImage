using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WpfAnimatedGif.Formats.Gif
{
    internal class GifImageData
    {
        private static readonly int MaxStackSize = 4096;
        private static readonly int MaxBits = 4097;

        internal static GifImageData ReadImageData(GifFrame frame, Stream stream, bool metadataOnly)
        {
            var imgData = new GifImageData();
            imgData.Read(stream, metadataOnly);
            imgData.TotalPixels = frame.Descriptor.Width * frame.Descriptor.Height;
            return imgData;
        }


        public int TotalPixels { get; set; }
        public byte LzwMinimumCodeSize { get; set; }
        public byte[] CompressedData { get; set; }

        private GifImageData()
        {
        }

        private void Read(Stream stream, bool metadataOnly)
        {
            LzwMinimumCodeSize = (byte)stream.ReadByte();
            CompressedData = GifHelpers.ReadDataBlocks(stream, metadataOnly);
        }

        public byte[] Decompress()
        {
            // Copyright (c) 2019 Jumar Macato
            // Licensed under the MIT License.
            // Ported from: https://github.com/AvaloniaUI/Avalonia.GIF

            // Initialize GIF data stream decoder.
            var dataSize = LzwMinimumCodeSize;
            var clear = 1 << dataSize;
            var endOfInformation = clear + 1;
            var available = clear + 2;
            var oldCode = -1;
            var codeSize = dataSize + 1;
            var codeMask = (1 << codeSize) - 1;

            var prefixBuf = new short[MaxStackSize];
            var suffixBuf = new byte[MaxStackSize];
            var pixelStack = new byte[MaxStackSize];
            var indics = new byte[TotalPixels];

            for (var code = 0; code < clear; code++)
            {
                suffixBuf[code] = (byte)code;
            }

            // Decode GIF pixel stream.
            int bits, first, top, pixelIndex;
            var datum = bits = first = top = pixelIndex = 0;

            var blockSize = CompressedData.Length;
            var tempBuf = CompressedData;

            var blockPos = 0;

            while (blockPos < blockSize)
            {
                datum += tempBuf[blockPos] << bits;
                blockPos++;

                bits += 8;

                while (bits >= codeSize)
                {
                    // Get the next code.
                    var code = datum & codeMask;
                    datum >>= codeSize;
                    bits -= codeSize;

                    // Interpret the code
                    if (code == clear)
                    {
                        // Reset decoder.
                        codeSize = dataSize + 1;
                        codeMask = (1 << codeSize) - 1;
                        available = clear + 2;
                        oldCode = -1;
                        continue;
                    }

                    // Check for explicit end-of-stream
                    if (code == endOfInformation)
                        return indics;

                    if (oldCode == -1)
                    {
                        indics[pixelIndex++] = suffixBuf[code];
                        oldCode = code;
                        first = code;
                        continue;
                    }

                    var inCode = code;
                    if (code >= available)
                    {
                        pixelStack[top++] = (byte)first;
                        code = oldCode;

                        if (top == 4097)
                            ThrowException();
                    }

                    while (code >= clear)
                    {
                        if (code >= MaxBits || code == prefixBuf[code])
                            ThrowException();

                        pixelStack[top++] = suffixBuf[code];
                        code = prefixBuf[code];

                        if (top == MaxBits)
                            ThrowException();
                    }

                    first = suffixBuf[code];
                    pixelStack[top++] = (byte)first;

                    // Add new code to the dictionary
                    if (available < MaxStackSize)
                    {
                        prefixBuf[available] = (short)oldCode;
                        suffixBuf[available] = (byte)first;
                        available++;

                        if (((available & codeMask) == 0) && (available < MaxStackSize))
                        {
                            codeSize++;
                            codeMask += available;
                        }
                    }

                    oldCode = inCode;

                    // Drain the pixel stack.
                    do
                    {
                        indics[pixelIndex++] = pixelStack[--top];
                    } while (top > 0);
                }
            }

            while (pixelIndex < TotalPixels)
                indics[pixelIndex++] = 0; // clear missing pixels

            return indics;

            void ThrowException() => throw new InvalidDataException();
        }
    }
}
