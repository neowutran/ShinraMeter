using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ACTION_STAGE : AbstractPacketHeuristic
    {
        public static uint LastId;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;
             if (message.Payload.Count < 2+2+8+4+4+4+2+4+4+4+4+4+4+1+4+4+4+4+4) return;
            Reader.Skip(2+2);
            var source = Reader.ReadUInt64();
            var pos = Reader.ReadVector3f();
            Reader.Skip(2);
            var model = Reader.ReadUInt32();
            var skill = Reader.ReadUInt32();
            var stage = Reader.ReadUInt32();
            Reader.Skip(4);
            var id = Reader.ReadUInt32(); // == sActionEnd.id

            if(source != DbUtils.GetPlayercId()) return;
            if(model != DbUtils.GetPlayerModel()) return;
            if(!Equals(pos, DbUtils.GetPlayerLocation())) return;
            if(skill - 0x04000000 != C_START_SKILL.LatestSkill) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            LastId = id;
        }
    }
}
