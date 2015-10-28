namespace Tera.Game.Messages
{
    public class SpawnNpcServerMessage : ParsedMessage
    {
        internal SpawnNpcServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(6);
            Id = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
    }
}