namespace Tera.Game.Messages
{
    public class SpawnMeServerMessage: ParsedMessage
    {
        public EntityId Id { get; private set; }

        internal SpawnMeServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
        }
    }
}
