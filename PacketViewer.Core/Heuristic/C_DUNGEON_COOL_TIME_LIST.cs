using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_DUNGEON_COOL_TIME_LIST : AbstractPacketHeuristic // #1
    {
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 0) return;
            if (C_NPCGUILD_LIST.PossibleOpcode != 0 && C_DUNGEON_CLEAR_COUNT_LIST.PossibleOpcode != 0) return;
            PossibleOpcode = message.OpCode;
        }
        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_DUNGEON_COOL_TIME_LIST);
        }
    }
}
