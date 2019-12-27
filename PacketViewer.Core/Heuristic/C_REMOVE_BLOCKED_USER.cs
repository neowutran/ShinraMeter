using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_REMOVE_BLOCKED_USER : AbstractPacketHeuristic
    {
        public static uint LastRemovedBlockedUser;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (message.Payload.Count < 2 + 4) return;
            var nameOffset = Reader.ReadUInt16();
            try
            {
                var name = Reader.ReadTeraString();
                if (S_USER_BLOCK_LIST.BlockedUsers.ContainsValue(name))
                {
                    LastRemovedBlockedUser = S_USER_BLOCK_LIST.BlockedUsers.FirstOrDefault(x => x.Value == name).Key;
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                }
            }
            catch (Exception e) { return; }
        }
    }
}
