using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_RESULT_BIDDING_DICE_THROW : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8+4) return;
            if(!DbUtils.IsPartyFormed())return;

            var cid = Reader.ReadUInt64();
            var roll = Reader.ReadUInt32();

            if(roll > 100 && roll != UInt32.MaxValue && roll != 0) return;
            if(!DbUtils.IsPartyMember(cid) && DbUtils.GetPlayercId() != cid) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
