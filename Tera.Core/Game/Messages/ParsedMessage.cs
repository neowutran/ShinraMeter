namespace Tera.Game.Messages
{
    // Base class for parsed messages
    public abstract class ParsedMessage : Message
    {
        internal ParsedMessage(TeraMessageReader reader)
            : base(reader.Message.Time, reader.Message.Direction, reader.Message.Data)
        {
            OpCodeName = reader.OpCodeName;
        }

        public string OpCodeName { get; private set; }
    }
}