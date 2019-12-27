using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
   
    public class S_NPC_LOCATION : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (message.Payload.Count != 40) { return; }
            var target = Reader.ReadUInt64();
            var origin = Reader.ReadVector3f();
            var w = Reader.ReadUInt16();
            var speed = Reader.ReadUInt16();
            var destination = Reader.ReadVector3f();

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                return;
            }
            var type = Reader.ReadUInt32();
            var distance = origin.DistanceTo(destination);
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs)) { return; }
            var npc = (List<Npc>)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs];
            if (npc.All(x => x.Cid != target)) { return; }
            if (AcceptedTypeValue.Contains(type) && distance < 200 && distance >= 0)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }
        }

        public List<uint> AcceptedTypeValue = new List<uint>()
        {
            0, // run
            1, // walk
            2, // fall
            5, // jump
            6, // jump on steep terrain?
            7, // stop moving
            10 // fall w/ recovery
        };
    }
}
