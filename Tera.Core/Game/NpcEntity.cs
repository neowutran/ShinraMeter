// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Tera.Game.Messages;

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class NpcEntity : Entity, IHasOwner
    {
        public EntityId OwnerId { get; private set; }
        public Entity Owner { get; private set; }

        public NpcEntity(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }
    }
}
