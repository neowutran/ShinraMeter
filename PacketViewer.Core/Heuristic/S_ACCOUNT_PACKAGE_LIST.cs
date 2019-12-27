using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class S_ACCOUNT_PACKAGE_LIST : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.GetOpcode(OpcodeEnum.S_LOAD_CLIENT_ACCOUNT_SETTING) == OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount -1).OpCode)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}

