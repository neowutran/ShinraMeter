using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CREATURE_CHANGE_HP : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 4 + 4 + 4 + 4 + 8 + 8 + 1 + 4) return;

            var curHp = Reader.ReadUInt32();
            var maxHp = Reader.ReadUInt32();
            var diff = Reader.ReadUInt32();
            var type = Reader.ReadUInt32();
            var target = Reader.ReadUInt64();
            var source = Reader.ReadUInt64();
            var unk1 = Reader.ReadByte();
            if (unk1 != 0) return;
            var ch = (LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter];
            if (ch.Cid != target) { return; } //the packet applies to any entity, but we use logged player for simplicity
            if (ch.MaxHp == maxHp) { OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE); }
        }
    }
}
