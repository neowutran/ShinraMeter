using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class S_LOGIN_ACCOUNT_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN_ARBITER)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 6 && OpcodeFinder.Instance.PacketCount < 13 && message.Payload.Count > 18)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
