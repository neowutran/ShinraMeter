using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_BAN_PARTY_MEMBER : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+4+4+4) return;

            var nameOffset = Reader.ReadUInt16();
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var unk = Reader.ReadUInt32(); //0xFFFFFFFF
            if(unk != 0xFFFFFFFF) return;
            if (nameOffset != Reader.BaseStream.Position + 4) return;
            try
            {
                var name = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }
            if(!DbUtils.IsPartyMember(playerId, serverId)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
