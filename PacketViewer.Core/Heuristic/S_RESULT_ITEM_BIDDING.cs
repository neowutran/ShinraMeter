using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_RESULT_ITEM_BIDDING : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 8) return;

            if(!DbUtils.IsPartyFormed()) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_RESULT_BIDDING_DICE_THROW))return;
            var playerId = Reader.ReadUInt64(); //this is uint32 in def, but doesen't match
            if(!DbUtils.IsPartyMember(playerId)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
