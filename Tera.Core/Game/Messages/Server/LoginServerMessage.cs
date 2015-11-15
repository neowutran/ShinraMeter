namespace Tera.Game.Messages
{
    public class LoginServerMessage : ParsedMessage
    {
        internal LoginServerMessage(TeraMessageReader reader, string guildName)
            : base(reader)
        {
            GuildName = guildName;
            reader.Skip(10);
            RaceGenderClass = new RaceGenderClass(reader.ReadInt32());
            Id = reader.ReadEntityId();
            reader.Skip(4);
            PlayerId = reader.ReadUInt32();
            reader.Skip(260);
            Name = reader.ReadTeraString();
        }

        public EntityId Id { get; private set; }
        public uint PlayerId { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }

        public PlayerClass Class => RaceGenderClass.Class;

        public RaceGenderClass RaceGenderClass { get; }
    }
}