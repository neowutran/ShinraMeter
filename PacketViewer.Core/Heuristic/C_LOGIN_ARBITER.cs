using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class C_LOGIN_ARBITER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if(IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if(OpcodeFinder.Instance.PacketCount <= 3 && message.Payload.Count > 18)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
