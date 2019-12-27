using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_SECOND_PASSWORD_AUTH : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode) || OpcodeFinder.Instance.PacketCount > 10) return;
            if (message.Payload.Count != 132) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

        }
    }
}
