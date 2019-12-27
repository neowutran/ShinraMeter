using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_REQUEST_USER_ITEMLEVEL_INFO : AbstractPacketHeuristic //    #4
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 0) return;

            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != C_DUNGEON_CLEAR_COUNT_LIST.PossibleOpcode) return;
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 2).OpCode != C_NPCGUILD_LIST.PossibleOpcode) return;
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 3).OpCode != C_DUNGEON_COOL_TIME_LIST.PossibleOpcode) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            C_DUNGEON_CLEAR_COUNT_LIST.Confirm();
            C_NPCGUILD_LIST.Confirm();
            C_DUNGEON_COOL_TIME_LIST.Confirm();
        }
    }
}
