namespace Tera.Game.Messages
{
    public class SAbnormalityEnd : ParsedMessage
    {
        internal SAbnormalityEnd(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();
        }

        public int AbnormalityId { get; }

        public EntityId TargetId { get; }
    }
}