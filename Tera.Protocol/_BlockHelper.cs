using System;
using System.IO;

namespace Tera.Protocol
{
    internal class BlockHelper
    {
        public static void WriteBlock(Stream stream, BlockType blockType, ArraySegment<byte> data)
        {
            stream.WriteByte((byte)blockType);
            WriteRawBlock(stream, data);
        }

        public static void WriteRawBlock(Stream stream, ArraySegment<byte> data)
        {
            var size = data.Count + 2;
            if (size > ushort.MaxValue)
                throw new ArgumentException("data.Count is too big");
            var header = new byte[2];
            header[0] = unchecked((byte)size);
            header[1] = (byte)(size >> 8);
            stream.Write(header, 0, header.Length);
            stream.Write(data.Array, data.Offset, data.Count);
        }

        public static void ReadBlock(Stream stream, out BlockType blockType, out byte[] data)
        {
            blockType = (BlockType)stream.ReadByte();
            var sizeBuffer = stream.ReadBytes(2);
            var size = (ushort)(sizeBuffer[0] | sizeBuffer[1] << 8);
            data = stream.ReadBytes(size - 2);
        }
    }
}
