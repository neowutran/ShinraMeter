using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Parsing;

namespace Tera.Protocol.Game.Messages
{
    public class SpawnNpcServerMessage : ParsedMessage
    {
        public EntityId Id { get; private set; }

        internal SpawnNpcServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(6);
            Id = reader.ReadEntityId();
        }
    }
}
