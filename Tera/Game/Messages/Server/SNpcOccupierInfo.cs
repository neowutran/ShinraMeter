namespace Tera.Game.Messages
{
    public class SNpcOccupierInfo : ParsedMessage

    {
        internal SNpcOccupierInfo(TeraMessageReader reader) : base(reader)
        {
            //  PrintRaw();
            NPC = reader.ReadEntityId();
            reader.Skip(8);
            Target = reader.ReadEntityId();

            //  Console.WriteLine("NPC:" + NPC + ";Target:" + Target);
        }

        public EntityId NPC { get; }
        public EntityId Target { get; }
    }
}