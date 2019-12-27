using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_UPDATE_NPCGUILD : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4*8) return;
            var user = Reader.ReadUInt64();
            Reader.Skip(8);
            var type = Reader.ReadInt32();
            if(type != 609) return; //hardcoding vanguard since it's the easiest to test
            Reader.Skip(8);
            var credits = Reader.ReadInt32();
            if(credits > 9000) return;
            if(user != DbUtils.GetPlayercId()) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
