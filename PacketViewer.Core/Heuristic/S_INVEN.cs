using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_INVEN : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 5000) { return; }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) { return; }
            if(OpcodeFinder.Instance.LastOccurrence(OpcodeEnum.S_LOGIN).Value.Key < OpcodeFinder.Instance.PacketCount - 5) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
