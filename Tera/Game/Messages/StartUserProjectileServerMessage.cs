namespace Tera.Game.Messages
{
    public class StartUserProjectileServerMessage : ParsedMessage
    {
        internal StartUserProjectileServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            OwnerId = reader.ReadEntityId();
            reader.Skip(8);
            Id = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
        public EntityId OwnerId { get; private set; }
    }
}