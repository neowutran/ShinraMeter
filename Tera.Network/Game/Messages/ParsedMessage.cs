using System;

namespace Tera.Game.Messages
{
    // Base class for parsed messages
    public abstract class ParsedMessage : Message
    {
        internal ParsedMessage(TeraMessageReader reader)
            : base(reader.Message.Time, reader.Message.Direction, reader.Message.Data)
        {
            Raw = reader.Message.Payload.Array;
            OpCodeName = reader.OpCodeName;
        }

        public void PrintRaw()
        {
            Console.WriteLine(OpCodeName + ": ");
            Console.WriteLine(BitConverter.ToString(Raw));
        }

        public byte[] Raw { get; protected set; }

        public string OpCodeName { get; private set; }
    }
}