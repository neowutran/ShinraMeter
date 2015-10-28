namespace Tera.Game.Messages
{
    public class SpawnUserServerMessage : ParsedMessage
    {
        internal SpawnUserServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(30);
            PlayerId = reader.ReadUInt32();
            Id = reader.ReadEntityId();
            reader.Skip(18);
            RaceGenderClass = new RaceGenderClass(reader.ReadInt32());
            reader.Skip(208);
            Name = reader.ReadTeraString();
            GuildName = reader.ReadTeraString();
        }

        public EntityId Id { get; private set; }
        public uint PlayerId { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }

        public PlayerClass Class
        {
            get { return RaceGenderClass.Class; }
        }

        public RaceGenderClass RaceGenderClass { get; }
    }
}