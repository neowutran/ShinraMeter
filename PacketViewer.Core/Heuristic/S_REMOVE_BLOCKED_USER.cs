using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_REMOVE_BLOCKED_USER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4) return;
            var id = Reader.ReadUInt32();
            if (S_USER_BLOCK_LIST.BlockedUsers.ContainsKey(id) && C_REMOVE_BLOCKED_USER.LastRemovedBlockedUser == id)
            {
                if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_REMOVE_BLOCKED_USER)) return;
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }
        }
    }
}
