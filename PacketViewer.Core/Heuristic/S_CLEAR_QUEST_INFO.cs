using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CLEAR_QUEST_INFO : AbstractPacketHeuristic
    {

        private static bool first = true;
        private static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            //---all this to avoid conflict with S_PING (just checking that we don't get C_PONG after 1st 0-length packet)---//
            var previousPacket = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if(previousPacket.OpCode == OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_PONG)) return;
            if (!first) OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OPCODE);
            //-------//

            if (message.Payload.Count != 0) return;
            //this is most likely to be the 1st 0-length server packet after login
            first = false;
            PossibleOpcode = message.OpCode;
        }
    }
}
