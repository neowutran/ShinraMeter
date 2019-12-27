using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ACCOMPLISH_ACHIEVEMENT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 29) return;
            //would need more checks (always preceded and followed by S_UPDATE_ACHIEVEMENT_PROGRESS?)
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
