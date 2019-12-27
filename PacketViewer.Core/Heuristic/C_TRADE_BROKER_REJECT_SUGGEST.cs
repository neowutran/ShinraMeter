using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_TRADE_BROKER_REJECT_SUGGEST : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8) return;
            if(S_TRADE_BROKER_DEAL_SUGGESTED.LatestListing == 0) return;
            if(S_TRADE_BROKER_DEAL_SUGGESTED.LatestBuyerId == 0) return;
            var playerId = Reader.ReadUInt32();
            var listing = Reader.ReadUInt32();
            if (playerId == S_TRADE_BROKER_DEAL_SUGGESTED.LatestBuyerId && listing == S_TRADE_BROKER_DEAL_SUGGESTED.LatestListing)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }
        }
    }
}
