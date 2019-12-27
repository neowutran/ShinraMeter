using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_CHANGE_PARTY_MEMBER_AUTHORITY : AbstractPacketHeuristic
    {
      
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if(message.Payload.Count != 4+4+1)return;
            if(!DbUtils.IsPartyFormed()) return;
            var serverId = Reader.ReadUInt32();
            if(BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;
            var playerId = Reader.ReadUInt32();
            var canInvite = Reader.ReadByte();
            if(canInvite != 0 && canInvite != 1) return;
            if(!DbUtils.IsPartyMember(playerId, serverId)) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
