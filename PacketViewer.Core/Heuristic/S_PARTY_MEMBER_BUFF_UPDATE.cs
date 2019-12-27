using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MEMBER_BUFF_UPDATE : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public static uint LastServerId;
        public static uint LastPlayerId;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if(!DbUtils.IsPartyFormed()) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_STAT_UPDATE)) return;
            if (message.Payload.Count < 2+2+2+2+4+4+4+4) return;
            var abnCount = Reader.ReadUInt16();
            var abnOffset = Reader.ReadUInt16();
            var condCount = Reader.ReadUInt16();
            var condOffset = Reader.ReadUInt16();
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            //arrays here

            if(BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;
            if(!DbUtils.IsPartyMember(playerId)) return;
            PossibleOpcode = message.OpCode;

            /*  
             *  it's almost always between 2 sPartyMemberStatUpdate (case 1: stat -> this -> stat),
             *  but sometimes it appears before or after the two (case 2: stat -> stat -> this; case 3: this -> stat -> stat)
             *  the following already confirms case 1 and 2; sPartyMemberStatUpdate confirms case 3
             */     

            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode ==
                OpcodeFinder.Instance.GetOpcode(OpcodeEnum.S_PARTY_MEMBER_STAT_UPDATE))
            {
                Confirm();
            }
            LastServerId = serverId;
            LastPlayerId = playerId;
        }

        public static void Confirm()
        {
            if(OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_BUFF_UPDATE)) return;
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.S_PARTY_MEMBER_BUFF_UPDATE);
        }
    }
}
