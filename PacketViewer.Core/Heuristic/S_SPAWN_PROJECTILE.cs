using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class S_SPAWN_PROJECTILE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            // 65 - current packet size from NA (EU should be too), 67 will be in future (maybe?)
            //TODO: ADD check with projectilOwnerId from sEachSkillResult
            if (message.Payload.Count != 65 && message.Payload.Count != 67) { return; }

            var id = Reader.ReadUInt64();
            var unk1 = Reader.ReadInt32();
            var skill = Reader.ReadInt32();
            var startPosition = Reader.ReadVector3f();
            var endPosition = Reader.ReadVector3f();
            var unk2 = Reader.ReadByte();
            var speed = Reader.ReadSingle();
            var source = Reader.ReadUInt64();
            var model = Reader.ReadUInt32();
            var unk4 = Reader.ReadInt32();
            var unk5 = Reader.ReadInt32();

            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var currChar))
            {
                //throw new Exception("Logger character should be know at this point."); //you could still receive packet matching this size before login, no need to crash it
                var ch = (LoggedCharacter) currChar;
                if (ch.Cid != source) { return; }
                if (ch.Model != model) { return; }
            }
            else return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}