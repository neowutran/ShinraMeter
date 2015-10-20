using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Messages;

namespace Tera.Protocol.Game
{
    public class User : Entity
    {
        public string Name { get; private set; }
        public string GuildName { get; private set; }
        public RaceGenderClass RaceGenderClass { get; private set; }

        public PlayerClass Class { get { return RaceGenderClass.Class; } }

        internal User(SpawnUserServerMessage message)
            : base(message.Id)
        {
            Name = message.Name;
            GuildName = message.GuildName;
            RaceGenderClass = message.RaceGenderClass;
        }

        internal User(LoginServerMessage message)
            : base(message.Id)
        {
            Name = message.Name;
            GuildName = message.GuildName;
            RaceGenderClass = message.RaceGenderClass;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} [{2}]", Class, Name, GuildName);
        }
    }
}
