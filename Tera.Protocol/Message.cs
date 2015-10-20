using System;

namespace Tera.Protocol
{
    public class Message
    {
        public DateTime Time { get; private set; }
        public MessageDirection Direction { get; private set; }
        public ArraySegment<byte> Data { get; private set; }

        public ushort OpCode { get { return (ushort)(Data.Array[Data.Offset] | Data.Array[Data.Offset + 1] << 8); } }
        public ArraySegment<byte> Payload { get { return new ArraySegment<byte>(Data.Array, Data.Offset + 2, Data.Count - 2); } }

        public Message(DateTime time, MessageDirection direction, ArraySegment<byte> data)
        {
            Time = time;
            Direction = direction;
            Data = data;
        }
    }
}
