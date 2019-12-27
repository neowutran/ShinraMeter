using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class C_PONG : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 0) { return; }
            var previousPacket = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (previousPacket.Direction == Tera.MessageDirection.ServerToClient && previousPacket.Payload.Count == 0)
            {
                OpcodeFinder.Instance.SetOpcode(previousPacket.OpCode, OpcodeEnum.S_PING);
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }
        }

    }
}
