using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_DELETE_FRIEND : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4) return;
            var id = Reader.ReadUInt32();
            if (S_FRIEND_LIST.Friends.ContainsKey(id) && C_DELETE_FRIEND.LastRemovedFriend == id)
            {
                if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_DELETE_FRIEND)) return;
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

            }
        }
    }
}
