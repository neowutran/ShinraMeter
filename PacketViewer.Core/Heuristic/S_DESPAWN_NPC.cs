using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_DESPAWN_NPC : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    var id = Reader.ReadUInt64();
                    RemoveNpcFromDatabase(id);
                }
                return;
            }
            if (message.Payload.Count != 8 + 4 + 4 + 4 + 4 + 4) return;

            var cid = Reader.ReadUInt64();
            if(cid == 0) return;
            //var pos = Reader.ReadVector3f();

            var x = Reader.ReadUInt32(); // these 2 are probably an uint64 for some cId
            var y = Reader.ReadUInt32(); // (in the other 28B length packet)

            var z = Reader.ReadUInt32();
            if(z == 2 || z == 0) return;
            var type = Reader.ReadUInt32();
            if (type != 1 && type != 5) return;
            var unk = Reader.ReadUInt32();
            if (unk != 0) return;
            if (!DbUtils.IsNpcSpawned(cid)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            RemoveNpcFromDatabase(cid);
        }

        private void RemoveNpcFromDatabase(ulong id)
        {
            OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, out var result);
            var list = (List<Npc>)result;
            if (list.Any(x => x.Cid == id)) list.Remove(list.FirstOrDefault(x => x.Cid == id));
            OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, list);
        }

    }
}
