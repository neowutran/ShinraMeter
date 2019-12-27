using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_BAN_PARTY : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 0) return;

            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_CLEAR_WORLD_QUEST_VILLAGER_INFO)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_CANT_FLY_ANYMORE)) return; //this requires the user to empty flight energy before he can be kicked from party to detect sBanParty, not optimal but avoids confusion 
            //TODO: remove this requirement^
            if(!DbUtils.IsPartyFormed()) return;
            var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1); //make sure it's not sLeaveParty
            if (msg.Payload.Count == 0) return;
            if (msg.Direction == MessageDirection.ClientToServer) return;
            //TODO: more checks?
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var x);

        }
    }
}
