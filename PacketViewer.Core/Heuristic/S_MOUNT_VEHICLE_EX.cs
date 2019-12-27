using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_MOUNT_VEHICLE_EX : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8+8+4) return;
            var owner = Reader.ReadUInt64();
            var vehicle = Reader.ReadUInt64();
            var unk = Reader.ReadUInt32();
            if(unk != 0) return;
            if(DbUtils.GetPlayercId() != owner) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
