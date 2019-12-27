using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CREATURE_ROTATE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8+2+2) return;
            var npc = Reader.ReadUInt64();
            var angle = Reader.ReadUInt16();
            var time = Reader.ReadUInt16();
            if(time == 0) return;
            if(!DbUtils.IsNpcSpawned(npc)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
