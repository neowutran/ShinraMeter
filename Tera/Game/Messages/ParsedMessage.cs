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
            /*
            if (OpCodeName.Contains("ABNORMALITY") || OpCodeName == "S_DESPAWN_NPC")
            {
                PrintRaw();
            }
            */
            //    Console.WriteLine(OpCodeName);
            /*
            if (OpCodeName.Contains("CHANGE_MP"))
            {
                PrintRaw();
            }
            */

            /*
            Console.WriteLine(OpCodeName);
            if (OpCodeName == "S_PLAYER_STAT_UPDATE")
            {
                PrintRaw();
            }
            */
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