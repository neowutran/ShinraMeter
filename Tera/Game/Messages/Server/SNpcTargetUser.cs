namespace Tera.Game.Messages
{
    public class SNpcTargetUser : ParsedMessage

    {
        internal SNpcTargetUser(TeraMessageReader reader) : base(reader)
        {
            NPC = reader.ReadEntityId();
        }

        public EntityId NPC { get; private set; }
    }
}