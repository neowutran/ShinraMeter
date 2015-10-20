using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tera.Protocol.Game.Parsing
{
    class TeraMessageReader : BinaryReader
    {
        public Message Message { get; private set; }
        public string OpCodeName { get; private set; }

        private static MemoryStream GetStream(Message message)
        {
            return new MemoryStream(message.Payload.Array, message.Payload.Offset, message.Payload.Count, false, true);
        }

        public TeraMessageReader(Message message, OpCodeNamer opCodeNamer)
            : base(GetStream(message),Encoding.Unicode)
        {
            Message = message;
            OpCodeName = opCodeNamer.GetName(message.OpCode);
        }

        public EntityId ReadEntityId()
        {
            var id = ReadUInt64();
            return new EntityId(id);
        }

        public void Skip(int count)
        {
            ReadBytes(count);
        }

        public string ReadTeraString()
        {
            var builder = new StringBuilder();
            while (true)
            {
                var c = ReadChar();
                if (c == 0)
                    return builder.ToString();
                builder.Append(c);
            }
        }
    }
}
