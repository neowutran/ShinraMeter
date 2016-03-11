using System;

namespace Tera.Game.Messages
{
    public class SNpcStatus : ParsedMessage

    {
        internal SNpcStatus(TeraMessageReader reader) : base(reader)
        {
            NPC = reader.ReadEntityId();
            Enraged = (reader.ReadByte() & 1) == 1;
	        reader.Skip(4);
            Target = reader.ReadEntityId();
            //Console.WriteLine("NPC:" + NPC + ";Target:" + Target + (Enraged?" Enraged":""));
        }

        public EntityId NPC { get; }
        public bool Enraged { get; }
        public EntityId Target { get; }
    }
}