using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ACTION_END : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;

            if (message.Payload.Count != 8+4+4+4+2+4+4+4+4) return;
            var source = Reader.ReadUInt64();
            var pos = Reader.ReadVector3f();
            Reader.Skip(2);
            var model = Reader.ReadUInt32();
            var skill = Reader.ReadUInt32();
            Reader.Skip(4); //type; can be used if needed
            var id = Reader.ReadUInt32(); // == sActionStage.id

            if (source != DbUtils.GetPlayercId()) return;
            if (model != DbUtils.GetPlayerModel()) return;
            if (skill - 0x04000000 != C_START_SKILL.LatestSkill) return;
            if(id != S_ACTION_STAGE.LastId) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
