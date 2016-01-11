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


            // Trop d'operation d'ecriture? En tous cas, dps meter ne fonctionne pas avec ca. Peut etre perte de packet? 
            // Too many write operation? I don't know. But with that line, the dps meter doesn't work. Packet loose? Threading pb? 
            // Will investigate but no many idea for now
            PrintRaw();

        }

        public byte[] Raw { get; protected set; }

        public string OpCodeName { get; }

        public void PrintRaw()
        {
            Console.WriteLine(OpCodeName + ": ");
            Console.WriteLine(BitConverter.ToString(Raw));
        }
    }
}