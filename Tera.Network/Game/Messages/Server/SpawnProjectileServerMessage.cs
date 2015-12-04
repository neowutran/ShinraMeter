namespace Tera.Game.Messages
{
    public class SpawnProjectileServerMessage : ParsedMessage
    {
        internal SpawnProjectileServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
            reader.Skip(37);
            OwnerId = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
        public EntityId OwnerId { get; private set; }
    }
}