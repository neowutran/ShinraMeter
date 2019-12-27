using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_NPC_TARGET_USER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8+1) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_SPAWN_NPC))return;
            var target = Reader.ReadUInt64();
            var status = Reader.ReadByte();
            if(status != 0 && status != 1) return;
            if(!DbUtils.IsNpcSpawned(target)) return; //assume it's current player
            //TODO: add more checks, maybe related to 1st aggro sequence
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
