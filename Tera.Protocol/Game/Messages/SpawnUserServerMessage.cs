using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Parsing;

namespace Tera.Protocol.Game.Messages
{
    public class SpawnUserServerMessage : ParsedMessage
    {
        public EntityId Id { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }
        public PlayerClass Class { get { return RaceGenderClass.Class; } }
        public RaceGenderClass RaceGenderClass { get; private set; }

        internal SpawnUserServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(0x22);
            Id = reader.ReadEntityId();
            reader.Skip(18);
            RaceGenderClass=new RaceGenderClass(reader.ReadInt32());
            reader.Skip(208);
            Name = reader.ReadTeraString();
            GuildName = reader.ReadTeraString();
        }
    }
}
