
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Game.Messages
{
    public class StartUserProjectileServerMessage : ParsedMessage
    {
        public EntityId Id { get; private set; }
        public EntityId OwnerId { get; private set; }

        internal StartUserProjectileServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            OwnerId = reader.ReadEntityId();
            reader.Skip(8);
            Id = reader.ReadEntityId();
        }
    }
}