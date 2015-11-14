// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game
{
    public class ProjectileEntity : Entity, IHasOwner
    {
        public ProjectileEntity(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }

        public EntityId OwnerId { get; }
        public Entity Owner { get; }
    }
}