// Copyright (c) 2013 Amemiya
// Licensed under the MIT License.
// Ported from: https://github.com/xupefei/APNG.NET

using System;
using System.IO;
using System.Text;

namespace WpfAnimatedGif.Formats.Png.Chunks
{
    internal class Chunk
    {
        internal Chunk(ChunkStream cs)
        {
            Length = cs.Length;
            ChunkType = cs.ChunkType ;
            ChunkData = cs.ReadBytes((int)Length);
            Crc = cs.ReadCrc();
        }

        public uint Length { get; set; }

        public string ChunkType { get; set; }

        public byte[] ChunkData { get; set; }

        public uint Crc { get; set; }

        /// <summary>
        ///     Get raw data of the chunk
        /// </summary>
        public byte[] RawData
        {
            get
            {
                var ms = new MemoryStream();
                ms.WriteUInt32(Helper.ConvertEndian(Length));
                ms.WriteBytes(Encoding.ASCII.GetBytes(ChunkType));
                ms.WriteBytes(ChunkData);
                ms.WriteUInt32(Helper.ConvertEndian(Crc));

                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Modify the ChunkData part.
        /// </summary>
        public void ModifyChunkData(int postion, byte[] newData)
        {
            Array.Copy(newData, 0, ChunkData, postion, newData.Length);

            using (var msCrc = new MemoryStream())
            {
                msCrc.WriteBytes(Encoding.ASCII.GetBytes(ChunkType));
                msCrc.WriteBytes(ChunkData);

                Crc = CrcHelper.Calculate(msCrc.ToArray());
            }
        }

        /// <summary>
        ///     Modify the ChunkData part.
        /// </summary>
        public void ModifyChunkData(int postion, uint newData)
        {
            ModifyChunkData(postion, BitConverter.GetBytes(newData));
        }
    }
}