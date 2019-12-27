using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
 
    public class C_SELECT_USER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 5) return;
            var id = Reader.ReadUInt32();
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out var chars))
            {
                var list = chars as Dictionary<uint, Character>;
                if (!list.ContainsKey(id)) { return; }
            }
            else
            {
                throw new Exception("At this point, characters must be known");
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

    }

}
