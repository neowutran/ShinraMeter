using System;

namespace Tera.Game.Messages
{
    public class SDespawnUser : ParsedMessage
    {
        internal SDespawnUser(TeraMessageReader reader) : base(reader)
        {
            User = reader.ReadEntityId();
            Console.WriteLine(User);
        }

        public EntityId User { get; }
    }
}
