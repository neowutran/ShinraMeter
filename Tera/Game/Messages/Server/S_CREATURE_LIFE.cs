using System;

namespace Tera.Game.Messages
{
    public class SCreatureLife : ParsedMessage
    {
        internal SCreatureLife(TeraMessageReader reader) : base(reader)
        {
            User = reader.ReadEntityId();
            Position = reader.ReadVector3f();
            Status = reader.ReadByte();
        }

        public EntityId User { get; }
        public Vector3f Position { get; private set; }
        public byte Status { get; } //0=dead,1=alive
    }
}