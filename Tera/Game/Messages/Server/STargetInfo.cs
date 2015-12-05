namespace Tera.Game.Messages
{
    public class STargetInfo : ParsedMessage

    {
        internal STargetInfo(TeraMessageReader reader) : base(reader)
        {
            reader.Skip(8);
            Target = reader.ReadEntityId();
        }

        public EntityId Target { get; private set; }
    }
}