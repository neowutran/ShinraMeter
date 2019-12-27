using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_REPLY_NONDB_ITEM_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4+1+1+4+4+4+4+4+4) return;
            var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if(msg.OpCode != C_REQUEST_NONDB_ITEM_INFO.PossibleOpcode) return;
            var item = Reader.ReadUInt32();
            if(C_REQUEST_NONDB_ITEM_INFO.LastItemId != item) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            OpcodeFinder.Instance.SetOpcode(msg.OpCode, OpcodeEnum.C_REQUEST_NONDB_ITEM_INFO);
        }
    }
}
