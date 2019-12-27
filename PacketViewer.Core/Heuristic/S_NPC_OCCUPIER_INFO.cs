using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_NPC_OCCUPIER_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8+8+8) return;
            var npc = Reader.ReadUInt64();
            var ePlayer = Reader.ReadUInt64();
            var cPlayer = Reader.ReadUInt64();
            if(!DbUtils.IsNpcSpawned(npc)) return;
            if(cPlayer != DbUtils.GetPlayercId()) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
