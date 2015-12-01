using System;

namespace Tera.Game.Messages
{
    public class SCreatureChangeHp : ParsedMessage
    {
        internal SCreatureChangeHp(TeraMessageReader reader) : base(reader)
        {
           // Console.WriteLine(BitConverter.ToString(reader.ReadBytes(16)));
            Creature = reader.ReadEntityId();
        }

        public EntityId Creature { get; }
    }
}