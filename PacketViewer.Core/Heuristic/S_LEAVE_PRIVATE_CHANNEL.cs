using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_LEAVE_PRIVATE_CHANNEL : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
            if (message.Payload.Count != 4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_JOIN_PRIVATE_CHANNEL)) return;
            var id = Reader.ReadUInt32();
            if (!S_JOIN_PRIVATE_CHANNEL.JoinedChannelId.Contains(id)) return;
            //need check on client packet?
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

        private void Parse()
        {
            var id = Reader.ReadUInt32();
            S_JOIN_PRIVATE_CHANNEL.JoinedChannelId.Remove(id);
        }
    }
}
