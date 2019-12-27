using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_LOGOUT_PARTY_MEMBER : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public static uint LastPlayerId;
        public static uint LastServerId;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 4+4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_STAT_UPDATE)) return;

            //quite a raw way of doing this, could be made more elegant

            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            if (!DbUtils.IsPartyMember(playerId, serverId)) return;
            PossibleOpcode = message.OpCode;
            LastPlayerId = playerId;
            LastServerId = serverId;

        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.S_LOGOUT_PARTY_MEMBER);

        }
    }
}
