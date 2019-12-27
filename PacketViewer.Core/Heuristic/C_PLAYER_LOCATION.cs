using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class C_PLAYER_LOCATION : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (message.Payload.Count != 39) { return; }
            var origin = Reader.ReadVector3f();
            var w = Reader.ReadUInt16();
            var unknown1 = Reader.ReadUInt16();
            var destination = Reader.ReadVector3f();
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation, destination);
                return;
            }
            var type = Reader.ReadUInt32();
            var speed = Reader.ReadUInt16();
            var unknown2 = Reader.ReadByte();
            var distance = origin.DistanceTo(destination);

            if (unknown1 == 0 && AcceptedTypeValue.Contains(type) && distance < 200 && distance >= 0 && speed == 0 && unknown2 == 0)
            {
                UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation, destination);

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

        private static void UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem knowledgeDatabaseKey, Tera.Game.Vector3f destination)
        {
            OpcodeFinder.Instance.KnowledgeDatabase[knowledgeDatabaseKey]= destination;
        }
    }
}