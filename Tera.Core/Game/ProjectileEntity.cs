// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game
{
    public class ProjectileEntity : Entity, IHasOwner
    {
        public EntityId OwnerId { get; private set; }
        public Entity Owner { get; private set; }

        public ProjectileEntity(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }
    }
}
