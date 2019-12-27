using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_DESPAWN_USER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    var id = Reader.ReadUInt64();
                    RemoveUserFromDatabase(id);
                }
                return;
            }
            if (message.Payload.Count != 8 + 4) return;

            var cid = Reader.ReadUInt64();
            var type = Reader.ReadUInt32();
            if (type != 1) return;
            if (!IsUserSpanwed(cid)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            RemoveUserFromDatabase(cid);
        }

        private bool IsUserSpanwed(ulong id)
        {
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers, out var result))
            {
                var list = (List<ulong>)result;
                if (list.Contains(id)) return true;
            }
            return false;
        }
        private void RemoveUserFromDatabase(ulong id)
        {
            var list = (List<ulong>)OpcodeFinder.Instance.KnowledgeDatabase.Where(x => x.Key == OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers).First().Value;
            if (list.Contains(id)) list.Remove(id);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers] = list;

        }

    }
}
