using System;

namespace Tera.Game.Messages
{
    public class SDespawnUser : ParsedMessage
    {
        internal SDespawnUser(TeraMessageReader reader) : base(reader)
        {
            User = reader.ReadEntityId();
        }

        public EntityId User { get; }
    }
}