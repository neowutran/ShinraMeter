using System;

namespace Tera.Game.Messages
{
    public class SpawnGunnerTurrel : ParsedMessage
    {
        internal SpawnGunnerTurrel(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
            reader.Skip(75);
            OwnerId = reader.ReadEntityId();
            Console.WriteLine(OwnerId);
        }

        public EntityId Id { get; private set; }
        public EntityId OwnerId { get; private set; }
    }
}