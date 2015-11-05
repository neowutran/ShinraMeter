// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Tera.Game.Messages;

namespace Tera.Game
{
    // Tracks which entities we have seen so far and what their properties are
    public class EntityTracker : IEnumerable<Entity>
    {
        private readonly Dictionary<EntityId, Entity> _dictionary = new Dictionary<EntityId, Entity>();
        public event Action<Entity> EntityUpdated;

        protected virtual void OnEntityUpdated(Entity entity)
        {
            Action<Entity> handler = EntityUpdated;
            if (handler != null) handler(entity);
        }

        public void Update(ParsedMessage message)
        {
            Entity newEntity = null;
            message.On<SpawnUserServerMessage>(m => newEntity = new UserEntity(m));
            message.On<LoginServerMessage>(m => newEntity = new UserEntity(m));
            message.On<SpawnNpcServerMessage>(m => newEntity = new NpcEntity(m.Id, m.OwnerId, GetOrPlaceholder(m.OwnerId)));
            message.On<SpawnProjectileServerMessage>(m => newEntity = new ProjectileEntity(m.Id, m.OwnerId, GetOrPlaceholder(m.OwnerId)));
            message.On<StartUserProjectileServerMessage>(m => newEntity = new ProjectileEntity(m.Id, m.OwnerId, GetOrPlaceholder(m.OwnerId)));
            if (newEntity != null)
            {
                _dictionary[newEntity.Id] = newEntity;
                OnEntityUpdated(newEntity);
            }
        }

        public Entity GetOrNull(EntityId id)
        {
            Entity entity;
            _dictionary.TryGetValue(id, out entity);
            return entity;
        }

        public Entity GetOrPlaceholder(EntityId id)
        {
            if (id == EntityId.Empty)
                return null;
            var entity = GetOrNull(id);
            if (entity != null)
                return entity;
            else
                return new PlaceHolderEntity(id);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
