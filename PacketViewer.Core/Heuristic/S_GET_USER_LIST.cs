using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public struct Character
    {
        public uint Id;
        public RaceGenderClass RaceGenderClass;
        public uint Level;
        public string Name;
        
        public int GuildId;

        public Character(uint id, RaceGenderClass raceGenderClass, uint level, string name, int guildId)
        {
            Id = id;
            RaceGenderClass = raceGenderClass;
            Level = level;
            Name = name;
            GuildId = guildId;
        }
    }
    public class S_GET_USER_LIST : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if(OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse();}
                return;
            }

            if (OpcodeFinder.Instance.PacketCount >= 10 && OpcodeFinder.Instance.PacketCount <= 15 && message.Payload.Count > 100)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

                //create a <PlayerId, Name> dictionary and add to KnowledgeDB
                var chars = new Dictionary<uint, Character>();
                var count = Reader.ReadUInt16();
                var offset = Reader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    Reader.BaseStream.Position = offset - 4;
                    Reader.Skip(2);
                    offset = Reader.ReadUInt16();
                    Reader.Skip(4);
                    var nameOffset = Reader.ReadUInt16();
                    Reader.Skip(10);
                    var id = Reader.ReadUInt32();
                    var gender = Reader.ReadUInt32();
                    var race = Reader.ReadUInt32();
                    var cl = Reader.ReadUInt32();
                    var level = Reader.ReadUInt32();
                    Reader.Skip(8);
                    var worldMapWorldId = Reader.ReadUInt32();
                    var worldMapGuardId = Reader.ReadUInt32();
                    var areaNameId = Reader.ReadUInt32();
                    Reader.Skip(8 + 1 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 8 + 4 + 4 + 4 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 2 + 4 + 1 + 4);

                    // KR latest change
                    if (NetworkController.Instance.Version == 319977)
                    {
                        Reader.Skip(8);
                    }
                    var achievementPoints = Reader.ReadInt32();
                    var laurel = Reader.ReadUInt32();
                    var position = Reader.ReadInt32();
                    var guildId = Reader.ReadInt32();

                    Reader.BaseStream.Position = nameOffset - 4;
                    var name = Reader.ReadTeraString();
                    // Unknown gender = baraka
                    if(gender == 0)
                    {
                        gender = 2;
                    }
                    var ch = new Character(id, new RaceGenderClass((Race)race,(Gender)gender -1,(PlayerClass)cl+1), level, name, guildId);
                    chars.Add(id, ch);
                }
                OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(OpcodeFinder.KnowledgeDatabaseItem.Characters, chars);
            }

        }

        private void Parse()
        {
            //not parsing, just checking on sReturnToLobby
            if(OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_RETURN_TO_LOBBY)) return;
            var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (msg.Payload.Count == 0 && msg.Direction == MessageDirection.ServerToClient && msg.OpCode == S_RETURN_TO_LOBBY.PossibleOpcode)
            {
                S_RETURN_TO_LOBBY.Confirm();
            }
        }
    }
}
