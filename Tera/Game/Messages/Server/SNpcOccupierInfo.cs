namespace Tera.Game.Messages
{
    public class SNpcOccupierInfo : ParsedMessage

    {
        internal SNpcOccupierInfo(TeraMessageReader reader) : base(reader)
        {
            //  PrintRaw();
            NPC = reader.ReadEntityId();
            Engager = reader.ReadEntityId();
            Target = reader.ReadEntityId();

            //  Console.WriteLine("NPC:" + NPC + ";Target:" + Target);
        }

        public EntityId NPC { get; }
        public EntityId Engager { get; }
        public EntityId Target { get; }
    }
}