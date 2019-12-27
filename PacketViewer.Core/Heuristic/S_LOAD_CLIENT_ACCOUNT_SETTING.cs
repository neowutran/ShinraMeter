using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class S_LOAD_CLIENT_ACCOUNT_SETTING : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_GET_USER_LIST)) return;
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            
            if (OpcodeFinder.Instance.PacketCount < 18 && message.Payload.Count > 1000)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
