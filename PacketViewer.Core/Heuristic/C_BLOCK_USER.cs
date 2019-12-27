using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_BLOCK_USER : AbstractPacketHeuristic
    {
        public static string LastBlockedUser;
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN))return;
            if (message.Payload.Count < 2+4) return;
            var nameOffset = Reader.ReadUInt16();
            try
            {
                LastBlockedUser = Reader.ReadTeraString();
                PossibleOpcode = message.OpCode;
            }
            catch (Exception e) { return; }
        }
    }
}
