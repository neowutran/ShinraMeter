using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{

    class S_SKILL_LIST : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_INVEN)) { return; }
            if (OpcodeFinder.Instance.LastOccurrence(OpcodeEnum.S_INVEN).Value.Key < OpcodeFinder.Instance.PacketCount - 2) { return; }
            var countSkillList = Reader.ReadUInt16();
            var offsetSkillList = Reader.ReadUInt16();

            // countSkillList + offsetSkillList + countSkillList * Array{ pointer + nextOffset + skill + unk }
            if(message.Payload.Count != (2 + 2) + (countSkillList * ( 2 +2 + 4 + 1 )))
            {
                return;
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
