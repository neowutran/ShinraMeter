using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;
using Data;

namespace PacketViewer.Heuristic
{
    public class S_LOGIN : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse();}
                return;
            }
            if (message.Payload.Count < 268) return;
            var nameOffset = Reader.ReadUInt16();
            Reader.Skip(8);
            var model = Reader.ReadUInt32();
            var cid = Reader.ReadUInt64();
            var serverId = Reader.ReadUInt32();
            if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;
            //if (NetworkController.Instance.Server.ServerId != serverId) return;
            var playerId = Reader.ReadUInt32();
            Reader.Skip(4+1+4+4+4+8+2);
            var level = Reader.ReadUInt16();
            Reader.BaseStream.Position = nameOffset - 4;
            var name = "";
            try
            {
                name = Reader.ReadTeraString();
            }
            catch (Exception) { return; }
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out var chars))
            {
                var list = chars as Dictionary<uint, Character>;
                if (!list.TryGetValue(playerId, out Character c)) { return; }
                if (model != c.RaceGenderClass.Raw) return;
                if (c.Name != name) return;
            }
            else
            {
                throw new Exception("At this point, characters must be known");
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            var ch = new LoggedCharacter(cid, model, name, playerId, level);
            OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, ch);
            //TODO


        }

        private void Parse()
        {
            var nameOffset = Reader.ReadUInt16();
            Reader.Skip(8);
            var model = Reader.ReadUInt32();
            var cid = Reader.ReadUInt64();
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            Reader.Skip(4 + 1 + 4 + 4 + 4 + 8 + 2);
            var level = Reader.ReadUInt16();
            Reader.BaseStream.Position = nameOffset - 4;
            var name = Reader.ReadTeraString();

            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out var chars))
            {
                var list = chars as Dictionary<uint, Character>;
                if (!list.TryGetValue(playerId, out Character c)) { return; }
                if (model != c.RaceGenderClass.Raw) return;
                if (c.Name != name) return;
            }
            var ch = new LoggedCharacter(cid, model, name, playerId, level);
            OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var garbage);
            OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, ch);
        }

    }

    public struct LoggedCharacter
    {
        public ulong Cid;
        public uint Model;
        public string Name;
        public uint PlayerId;
        public uint Level;
        public uint MaxHp;
        public uint MaxMp;
        public uint MaxSt;

        public LoggedCharacter(ulong cid, uint model, string name, uint pId, uint lvl)
        {
            Cid = cid;
            Model = model;
            Name = name;
            PlayerId = pId;
            Level = lvl;
            MaxHp = 0;
            MaxMp = 0;
            MaxSt = 0;

        }
    }
}
