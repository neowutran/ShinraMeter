using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_FIN_INTER_PARTY_MATCH : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (message.Payload.Count != 4) return;
            var zoneId = Reader.ReadUInt32();
            if (zoneId != 9777 && zoneId != 777) return; // TODO CW, need to remove this hardcoded check once we have enough data to detect without it

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
