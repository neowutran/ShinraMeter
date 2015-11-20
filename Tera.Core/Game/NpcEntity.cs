// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class NpcEntity : Entity, IHasOwner
    {
        public NpcEntity(EntityId id, EntityId ownerId, uint modelId, uint npcId, ushort npcType, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
            ModelId = modelId;
            NpcId = npcId;
            NpcType = npcType;
        }

        public uint ModelId { get; }
        public uint NpcId { get; }
        public ushort NpcType { get; }

        public EntityId OwnerId { get; }
        public Entity Owner { get; }
    }
}