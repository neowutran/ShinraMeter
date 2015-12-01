using System;

namespace Tera.Game.Messages
{
    // Created when we want a parsed message, but don't know how to handle that OpCode
    public class UnknownMessage : ParsedMessage
    {
        internal UnknownMessage(TeraMessageReader reader)
            : base(reader)
        {
            /*
            var opname = reader.OpCodeName;
            if (opname == "S_ABNORMALITY_END" || opname == "S_ABNORMALITY_BEGIN" || opname == "S_CREATURE_CHANGE_HP" ||
                opname == "S_TARGET_INFO")
            {
                //S_NPC_STATUS
                Console.WriteLine(reader.OpCodeName);
            }

            if (opname == "S_CREATURE_CHANGE_HP")
            {
                Console.WriteLine(BitConverter.ToString(reader.Message.Payload.Array));
            }
            */
        }
    }
}