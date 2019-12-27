using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ABNORMALITY_BEGIN : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    if (OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_ABNORMALITY_END)) return;
                    var targetId = Reader.ReadUInt64();
                    Reader.Skip(8);
                    var abId = Reader.ReadUInt32();
                    if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var result0))
                    {
                        var ch = (LoggedCharacter)result0;
                        if (ch.Cid != targetId) return;
                        AddAbnormToDb(abId);
                    }
                }
                return;
            }

            if (message.Payload.Count != 8+8+4+4+4+4+4) return;
            var target = Reader.ReadUInt64();
            var source = Reader.ReadUInt64();
            var id = Reader.ReadUInt32();
            var duration = Reader.ReadUInt32();
            Reader.Skip(4);
            var stacks = Reader.ReadUInt32();

            // let's check on vanguard login buff (I) -- regardless of login days, all 3 levels of the buffs are applied
            // only problem is if it's completely missing (account not logged in for last 7 days)

            if (id != 99020000) return;
            if(stacks != 1) return;
            if (duration != int.MaxValue) return;
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var result))
            {
                var ch = (LoggedCharacter) result;
                if (ch.Cid != target) return;
                AddAbnormToDb(id);
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }
        }

        private void AddAbnormToDb(uint abId)
        {
            List<uint> list = new List<uint>();
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacterAbnormalities, out var result))
            {
                list = (List<uint>)result;
            }
            if (!list.Contains(abId)) list.Add(abId);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacterAbnormalities] = list;

        }
    }
}
