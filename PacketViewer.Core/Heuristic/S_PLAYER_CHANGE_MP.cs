using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PLAYER_CHANGE_MP : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 4+4+4+4+8+8) return;

            var curMp = Reader.ReadUInt32();
            var maxMp = Reader.ReadUInt32();
            var diff = Reader.ReadUInt32();
            var type = Reader.ReadUInt32();
            var target = Reader.ReadUInt64();
            var source = Reader.ReadUInt64();
            if (type > 4) return;
            var ch = (LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter];
            if (ch.Cid != target) { return; } //the packet applies to any entity, but we use logged player for simplicity
            if (ch.MaxMp == maxMp) { OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE); }
        }
    }
}
