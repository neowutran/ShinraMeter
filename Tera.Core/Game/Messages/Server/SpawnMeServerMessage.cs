namespace Tera.Game.Messages
{
    public class SpawnMeServerMessage : ParsedMessage
    {
        internal SpawnMeServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
    }
}