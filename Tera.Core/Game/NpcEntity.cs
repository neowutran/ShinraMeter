// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class NpcEntity : Entity, IHasOwner
    {
        public NpcEntity(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }

        public EntityId OwnerId { get; }
        public Entity Owner { get; }
    }
}