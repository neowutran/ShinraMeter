using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{

    class S_AVAILABLE_SOCIAL_LIST : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_SKILL_LIST)) { return; }
            if(message.Payload.Count != 4) { return; }    
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
