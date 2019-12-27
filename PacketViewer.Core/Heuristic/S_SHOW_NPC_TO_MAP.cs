using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_SHOW_NPC_TO_MAP : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if(message.Payload.Count != 4) { return; }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) { return; }
            if(!OpcodeFinder.Instance.PacketSeenInTheLastXms(OpcodeEnum.S_LOGIN, message.Time, 20)) { return; }
            var unk = Reader.ReadUInt32();
            if(unk != 0 && unk != 1) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
