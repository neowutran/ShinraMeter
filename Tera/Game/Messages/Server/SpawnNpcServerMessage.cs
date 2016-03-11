// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game.Messages
{
    public class SpawnNpcServerMessage : ParsedMessage
    {
        internal SpawnNpcServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(6);
            Id = reader.ReadEntityId();
            TargetId = reader.ReadEntityId();
            Position = reader.ReadVector3f();
            Heading = reader.ReadAngle();
            reader.Skip(4);
            NpcId = reader.ReadUInt32();
            NpcArea = reader.ReadUInt16();
            CategoryId = reader.ReadUInt32();
            reader.Skip(31);
            OwnerId = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
        public EntityId OwnerId { get; private set; }
        public EntityId TargetId { get; private set; }
        public Vector3f Position { get; private set; }
        public Angle Heading { get; private set; }
        public uint NpcId { get; private set; }
        public ushort NpcArea { get; private set; }
        public uint CategoryId { get; private set; }
    }
}