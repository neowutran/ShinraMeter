using Tera.Game.Messages;

namespace Tera.Game
{
    // A player character, including your own
    public class User : Entity
    {
        public string Name { get; set; }
        public string GuildName { get; set; }
        public RaceGenderClass RaceGenderClass { get; set; }

        public PlayerClass Class { get { return RaceGenderClass.Class; } }

        public User(EntityId id)
            : base(id)
        {
        }

        internal User(SpawnUserServerMessage message)
            : this(message.Id)
        {
            Name = message.Name;
            GuildName = message.GuildName;
            RaceGenderClass = message.RaceGenderClass;
        }

        internal User(LoginServerMessage message)
            : this(message.Id)
        {
            Name = message.Name;
            GuildName = message.GuildName;
            RaceGenderClass = message.RaceGenderClass;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} [{2}]", Class, Name, GuildName);
        }

        public static User ForEntity(Entity entity)
        {
            var projectile = entity as Projectile;
            if (projectile != null)
                entity = projectile.Owner;

            return entity as User;
        }
    }
}
