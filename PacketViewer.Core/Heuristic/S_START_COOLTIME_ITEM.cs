using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_START_COOLTIME_ITEM : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count != 8) return;
            var item = Reader.ReadUInt32();
            var cd = Reader.ReadUInt32();
            /*
             * // we could also check on minor battle solution, since we use it for sAbnormalityRefresh already
             * if (item != 200997 && cd != 5) return; 
             */
            if (OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_USE_ITEM))
            {
                if (C_USE_ITEM.LatestItem == item)
                {
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                }
            }

        }
    }
}
