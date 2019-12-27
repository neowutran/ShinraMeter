using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_WEAK_POINT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 8+4+4+4+4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;

            var target = Reader.ReadUInt64();
            var prevVal = Reader.ReadUInt32();
            var newVal = Reader.ReadUInt32();
            var type = Reader.ReadUInt32();
            var skill = Reader.ReadUInt32();

            if(type != 0 && type != 1 && type != 2 && type != 3) return;
            if(prevVal > 7) return;
            if(newVal > 7) return;

            if(C_START_SKILL.LatestSkill != skill) return;
            if(DbUtils.GetPlayercId() != target && !DbUtils.IsNpcSpawned(target)) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
