using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MEMBER_ABNORMAL_CLEAR : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;

        public static uint LastServerId;
        public static uint LastPlayerId;
        
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4 + 4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_ADD)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_DEL)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_REFRESH)) return;

            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            if (!DbUtils.IsPartyMember(playerId, serverId)) return;
            PossibleOpcode = message.OpCode;
            LastServerId = serverId;
            LastPlayerId = playerId;
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_CLEAR);
        }

    }
}
