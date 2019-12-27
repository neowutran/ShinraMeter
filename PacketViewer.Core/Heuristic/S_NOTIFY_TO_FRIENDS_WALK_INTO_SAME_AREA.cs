using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_NOTIFY_TO_FRIENDS_WALK_INTO_SAME_AREA : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4*4) return;
            var playerId = Reader.ReadUInt32();
            var worldId = Reader.ReadUInt32();
            var guardId = Reader.ReadUInt32();
            var sectionId = Reader.ReadUInt32();
            if(worldId == 0) return;
            if(guardId == 0) return;
            if(sectionId == 0) return;
            if (!DbUtils.IsFriend(playerId)) return;
            //TODO: sometimes mistaken for other packets, add checks on world/guard/section IDs
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
