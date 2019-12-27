using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_REQUEST_NONDB_ITEM_INFO : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public static uint LastItemId;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 3*4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_SPAWN_ME)) return;
            var item = Reader.ReadUInt32();
            var unk1 = Reader.ReadUInt32();
            var unk2 = Reader.ReadUInt32();
            if(item == 0) return;
            if(unk1 != 0) return;
            if(unk2 != 0) return;

            PossibleOpcode = message.OpCode;
            LastItemId = item;

            //OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
