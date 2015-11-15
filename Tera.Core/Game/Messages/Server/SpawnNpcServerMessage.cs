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
            Position = reader.ReadVector3F();
            Heading = reader.ReadAngle();
            reader.Skip(4);
            NpcId = reader.ReadUInt32();
            NpcType = reader.ReadUInt16();
            ModelId = reader.ReadUInt32();
            reader.Skip(31);
            OwnerId = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
        public EntityId OwnerId { get; private set; }
        public EntityId TargetId { get; private set; }
        public Vector3F Position { get; private set; }
        public Angle Heading { get; private set; }
        public uint NpcId { get; private set; }
        public ushort NpcType { get; private set; }
        public uint ModelId { get; private set; }
    }
}