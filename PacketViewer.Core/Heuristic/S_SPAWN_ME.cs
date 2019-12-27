using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    internal class S_SPAWN_ME : AbstractPacketHeuristic
    {
        public Vector3f LatestPos;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Reader.Skip(8);
                    LatestPos = Reader.ReadVector3f();
                    if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOAD_TOPO))
                    {
                        S_LOAD_TOPO.Confirm(LatestPos);
                    }
                }
                return;
            }

            if (message.Payload.Count != 24) return;
            var target = Reader.ReadUInt64();
            LatestPos = Reader.ReadVector3f(); //maybe add more checks based on pos?
            var w = Reader.ReadUInt16();
            var alive = Reader.ReadBoolean();
            var unk = Reader.ReadByte();
            if (unk != 0) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var currChar))
            {
                UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.CharacterSpawnedSuccesfully, false);
                return;
            }
            var ch = (LoggedCharacter)currChar;
            if (target != ch.Cid) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OpcodeEnum.S_SPAWN_ME);
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOAD_TOPO))
            {

                S_LOAD_TOPO.Confirm(LatestPos);

            }
            UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.CharacterSpawnedSuccesfully, true);
        }

        private static void UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem knowledgeDatabaseKey, bool state)
        {
            OpcodeFinder.Instance.KnowledgeDatabase[knowledgeDatabaseKey] =  state;
        }
    }
}