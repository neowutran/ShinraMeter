using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_DELETE_FRIEND : AbstractPacketHeuristic
    {
        public static uint LastRemovedFriend;
        public static ushort PossibleOpcode;
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
                if (S_FRIEND_LIST.Friends.ContainsValue(name))
                {
                    LastRemovedFriend = S_FRIEND_LIST.Friends.FirstOrDefault(x => x.Value == name).Key;
                    //OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                    PossibleOpcode = message.OpCode;
                }
            }
            catch (Exception e) { return; }
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_DELETE_FRIEND);
        }
    }
}
