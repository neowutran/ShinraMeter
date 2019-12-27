using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CREST_MESSAGE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;
            if (message.Payload.Count != 4+4+4) return;
            var unk = Reader.ReadUInt32();
            if(unk != 0) return;
            var type = Reader.ReadUInt32(); // 6 = reset, could be hardcoded since it's easy to test for many classes
            var skill = Reader.ReadUInt32();
            if(skill/100 != C_START_SKILL.LatestSkill/100) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
