// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game
{
    // An object with an Id that can be spawned or deswpawned in the game world
    public class Entity
    {
        public EntityId Id { get; private set; }

        public Entity(EntityId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", GetType().Name, Id);
        }
    }
}
