using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_NPC_STATUS : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count != 8 + 1 + 4 + 8 + 4) return;
            var creature = Reader.ReadUInt64();
            var enraged = Reader.ReadByte();
            var unk1 = Reader.ReadUInt32();
            var target = Reader.ReadUInt64();
            var unk2 = Reader.ReadUInt32();

            if (enraged != 0 && enraged != 1) return;
            if (unk1 != 4 && unk1 != 5) return;
            if (unk2 != 0 && unk2 != 1 && unk2 != 2 && unk2 != 4) return;

            if (!DbUtils.IsNpcSpawned(creature)) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
