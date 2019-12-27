using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{

    public class S_SECOND_PASSWORD_AUTH_RESULT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (OpcodeFinder.Instance.PacketCount > 10) { return; }
            if (message.Payload.Count != 7) { return; }
            var previousPacket = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            var previousOpcode = OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_SECOND_PASSWORD_AUTH);
            if (previousOpcode != null && previousOpcode.HasValue && previousOpcode == previousPacket.OpCode)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }
        }
    }
}
