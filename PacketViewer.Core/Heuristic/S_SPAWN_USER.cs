using Data;
using System;
using System.Collections.Generic;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_SPAWN_USER:AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Reader.Skip(10+2+2+2+2+2+2+2+4+4);
                    var id = Reader.ReadUInt64();
                    AddUserToDatabase(id);
                }
                return;
            }

            if (message.Payload.Count < 200) return;
            Reader.Skip(2+2+2+2);
            var nameOffset = Reader.ReadUInt16();
            Reader.Skip(2+2+2+2+2+2+2);
            var serverId = Reader.ReadUInt32();
            if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;
            //if (serverId != NetworkController.Instance.Server.ServerId) return; //assume that we are not in IM
            Reader.Skip(4);
            var id2 = Reader.ReadUInt64();
            Reader.Skip(4+4+4+2+4);
            var model = Reader.ReadUInt32();
            if (model < 10101 || model > 11108) return; //could me made more accurate by checking actual race/gender/class ranges
            var unk1 = Reader.ReadInt16();
            if(unk1 != 0) return;
            Reader.Skip(2+2);
            var unk4 = Reader.ReadInt16();
            if (unk4 != 0) return;
            var unk5 = Reader.ReadInt16();
            if (unk5 != 0 && unk5 != 3) return;
            Reader.Skip(1+1+4+4+4+4+4+4+4+4+4+4+4+4+4);
            var unk13 = Reader.ReadInt32();
            if (unk13 != 0) return;
            var unk14 = Reader.ReadInt32();
            if (unk14 != 0) return;
            var unk15 = Reader.ReadByte();
            if (unk15 != 0) return;
            Reader.Skip(2+4+4+4+4+4+4+4+4);
            var unk25 = Reader.ReadInt32();
            if (unk25 != 0) return;
            Reader.Skip(4+4+4+4);
            var wepEnch = Reader.ReadUInt32();
            if(wepEnch > 15 || wepEnch < 0) return;
            var unk27 = Reader.ReadInt16();
            if (unk27 != 0) return;
            var level = Reader.ReadInt32();
            if (level < 0 || level > 65) return;
            var unk28 = Reader.ReadInt32();
            if (unk28 != 0) return;
            var unk29 = Reader.ReadInt32();
            if (unk29 != 0) return;
            Reader.Skip(1+4+4+4+4+4+4+4+4+1+1+4);
            var unk45 = Reader.ReadInt32();
            if (unk45 != 0) return;
            var unk46 = Reader.ReadInt32();
            if (unk46 != 0 && unk46 != 100) return;
            var unk47 = Reader.ReadSingle();
            if (unk47 != 1.0) return;
            Reader.BaseStream.Position = nameOffset - 4;
            try { Reader.ReadTeraString(); }
            catch (Exception e) { return; }    

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            AddUserToDatabase(id2);

        }
        private void AddUserToDatabase(ulong id)
        {
            List<ulong> list = new List<ulong>();
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers, out var result))
            {
                list = (List<ulong>)result;
            }
            if (!list.Contains(id)) list.Add(id);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers] = list; 
        }
    }
}
