using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MEMBER_ABNORMAL_DEL : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }

                return;
            }
            if(message.Payload.Count != 4+4+4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_ADD)) return;
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var abnormId = Reader.ReadUInt32();
            if(!DbUtils.IsPartyMember(playerId, serverId)) return;
            if (!DbUtils.PartyMemberHasAbnorm(playerId, serverId, abnormId)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            DbUtils.RemovePartyMemberAbnormal(playerId, serverId, abnormId);
        }

        private void Parse()
        {
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var abnormId = Reader.ReadUInt32();
            DbUtils.RemovePartyMemberAbnormal(playerId, serverId, abnormId);

        }
    }
}
