using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Parsing;

namespace Tera.Protocol.Game.Messages
{
    public class SpawnProjectileServerMessage:ParsedMessage
    {
        public EntityId Id { get; private set; }

        internal SpawnProjectileServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
        }
    }
}
