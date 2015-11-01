using System;

namespace Tera.Game.Messages
{
    public class SpawnNpcServerMessage : ParsedMessage
    {
        internal SpawnNpcServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(6);
            Id = reader.ReadEntityId();
            reader.Skip(67);
            OwnerId = reader.ReadEntityId();
            Console.WriteLine("gunner turrel:" + OwnerId);
        }

        public EntityId OwnerId { get; }

        public EntityId Id { get; private set; }
    }
}