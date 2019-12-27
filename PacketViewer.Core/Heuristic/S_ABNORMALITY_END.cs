using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ABNORMALITY_END : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 8 +4) return;
            var target = Reader.ReadUInt64();
            var id = Reader.ReadUInt32();
            
            //we check it on current player
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var result))
            {
                var ch = (LoggedCharacter)result;
                if (ch.Cid != target) return;
                if(OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacterAbnormalities, out var result2))
                {
                    var abs = (List<uint>) result2;
                    if (!abs.Contains(id)) return;
                }
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacterAbnormalities, out var garbage);
            }
        }
    }
}
