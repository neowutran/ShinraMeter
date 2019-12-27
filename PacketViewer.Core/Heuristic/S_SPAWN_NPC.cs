using System.Collections.Generic;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_SPAWN_NPC : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Reader.Skip(10);
                    var id = Reader.ReadUInt64();
                    Reader.Skip(8+4+4+4+2+4); 
                    var templateId0 = Reader.ReadUInt32();
                    var zoneId0 = Reader.ReadUInt16();

                    AddNpcToDatabase(id, zoneId0, templateId0);
                }
                return;
            }
            if (message.Payload.Count < 100) return;

            Reader.Skip(10);
            var cid = Reader.ReadUInt64();
            Reader.Skip(8 + 4 + 4 + 4 + 2); //skipped pos, may be useful for more accuracy -- subtracted 2
            var unk1 = Reader.ReadInt32();
            if (unk1 != 0xC && unk1 != 0xA) return;
            var templateId = Reader.ReadUInt32();
            var zoneId = Reader.ReadUInt16(); //can check on current player location, need to add player location to db tho
            Reader.Skip(4);
            var unk5 = Reader.ReadUInt16();
            if (unk5 != 0 && unk5 != 4) return;
            Reader.Skip(2);
            var unk7 = Reader.ReadUInt32();
            if (unk7 != 0 && unk7 != 5) return;
            var unk8 = Reader.ReadByte();
            if (unk8 != 1) return;
            Reader.Skip(1);
            var unk10 = Reader.ReadUInt32();
            if (unk10 != 0 && unk10 != 1 && unk10 != 3 && unk10 != 4) return;
            Reader.Skip(8 + 2 + 2);
            var unk14 = Reader.ReadInt32();
            if (unk14 != 0) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            AddNpcToDatabase(cid, zoneId, templateId);
        }

        private void AddNpcToDatabase(ulong id, uint zoneId,  uint templId)
        {
            var newNpc = new Npc(id, zoneId, templId);
            List<Npc> list = new List<Npc>();
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, out var result))
            {
                OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, out var garbage);
                list = (List<Npc>)result;
            }
            if (!list.Contains(newNpc)) list.Add(newNpc);
            OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, list);
        }

    }

    public struct Npc
    {
        public ulong Cid { get; }
        public uint ZoneId { get; }
        public uint TemplateId { get; }

        public Npc(ulong cid, uint zoneId, uint templId)
        {
            Cid = cid;
            ZoneId = zoneId;
            TemplateId = templId;
        }
    }
}
