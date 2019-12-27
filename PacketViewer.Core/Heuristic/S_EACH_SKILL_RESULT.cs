using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class S_EACH_SKILL_RESULT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 97) { return; }

            var source = Reader.ReadUInt64();
            var projectilOwnerId = Reader.ReadUInt64();
            var target = Reader.ReadUInt64();
            var model = Reader.ReadUInt32();
            var skill = Reader.ReadUInt32();
            var stage = Reader.ReadUInt32();
            var unk1 = Reader.ReadUInt32();
            var id = Reader.ReadUInt32();
            var time = Reader.ReadUInt32();
            var damage = Reader.ReadUInt64();
            var type1 = Reader.ReadUInt16();
            var type2 = Reader.ReadUInt16();
            var crit = Reader.ReadUInt16();
            var blocked = Reader.ReadByte();
            var unk2 = Reader.ReadUInt32();
            

            if (unk2 == 0)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }

       
    }
}
