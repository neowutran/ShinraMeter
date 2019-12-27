using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CHECK_TO_READY_PARTY_FIN : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 5) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_CHECK_TO_READY_PARTY)) return;
            var unk1 = Reader.ReadUInt32();
            var unk2 = Reader.ReadByte();
            var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if(msg.OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.S_CHECK_TO_READY_PARTY)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
