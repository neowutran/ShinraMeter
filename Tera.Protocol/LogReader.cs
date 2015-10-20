using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tera.Protocol
{
    public class LogReader
    {
        private DateTime _time;
        private readonly Stream _stream;

        internal LogReader(Stream stream)
        {
            _stream = stream;
            ReadHeader();
        }

        private void ReadHeader()
        {
            BlockType blockType;
            byte[] data;
            do
            {
                BlockHelper.ReadBlock(_stream, out blockType, out data);
            } while (blockType != BlockType.Start);
        }

        public Message ReadMessage()
        {
            while (_stream.Position != _stream.Length)
            {
                BlockType blockType;
                byte[] data;
                BlockHelper.ReadBlock(_stream, out blockType, out data);
                MessageDirection direction;
                switch (blockType)
                {
                    case BlockType.Timestamp:
                        _time = LogHelper.BytesToTimeSpan(data);
                        break;
                    case BlockType.Client:
                        direction = MessageDirection.ClientToServer;
                        return new Message(_time, direction, new ArraySegment<byte>(data));
                    case BlockType.Server:
                        direction = MessageDirection.ServerToClient;
                        return new Message(_time, direction, new ArraySegment<byte>(data));
                    default:
                        throw new FormatException(string.Format("Unexpected blocktype {0}", blockType));
                }
            }
            return null;
        }

        public static IEnumerable<Message> ReadMessages(string filename)
        {
            return ReadMessages(() => new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        private static IEnumerable<Message> ReadMessages(Func<Stream> openStream)
        {
            using (var stream = openStream())
            {
                var reader = new LogReader(stream);
                Message message;
                while ((message = reader.ReadMessage()) != null)
                {
                    yield return message;
                }
            }
        }
    }
}
