using System;
using System.IO;

namespace Tera.PacketLog
{
    internal class BlockSplitter
    {
        private readonly MemoryStream buffer = new MemoryStream();
        public event Action<byte[]> BlockFinished;

        protected virtual void OnBlockFinished(byte[] block)
        {
            var handler = BlockFinished;
            if (handler != null) handler(block);
        }

        private static void RemoveFront(MemoryStream stream, int count)
        {
            Array.Copy(stream.GetBuffer(), count, stream.GetBuffer(), 0, stream.Length - count);
            stream.SetLength(stream.Length - count);
        }

        private static byte[] PopBlock(MemoryStream stream)
        {
            if (stream.Length < 2)
                return null;
            var buffer = stream.GetBuffer();
            var blockSize = buffer[0] | buffer[1] << 8;
            if (stream.Length < blockSize)
                return null;
            var block = new byte[blockSize];
            Array.Copy(buffer, 2, block, 0, blockSize - 2);
            RemoveFront(stream, blockSize);
            return block;
        }

        public byte[] PopBlock()
        {
            var block = PopBlock(buffer);
            if (block != null)
            {
                OnBlockFinished(block);
            }
            return block;
        }

        public void PopAllBlocks()
        {
            while (PopBlock() != null)
            {
            }
        }

        public void Data(byte[] data)
        {
            buffer.Write(data, 0, data.Length);
        }
    }
}