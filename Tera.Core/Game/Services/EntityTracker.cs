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

        public IEnumerator<Entity> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event Action<Entity> EntityUpdated;

        protected virtual void OnEntityUpdated(Entity entity)
        {
            var handler = EntityUpdated;
            if (handler != null) handler(entity);
        }

        public void Update(ParsedMessage message)
        {
            Entity newEntity = null;
            message.On<SpawnUserServerMessage>(m => newEntity = new UserEntity(m));
            message.On<LoginServerMessage>(m => newEntity = new UserEntity(m));
            //   message.On<SpawnNpcServerMessage>(m => newEntity = new NpcEntity(m));
            message.On<SpawnNpcServerMessage>(
                m => newEntity = new ProjectileEntity(m.Id, m.OwnerId, GetOrNull(m.OwnerId)));
            message.On<SpawnProjectileServerMessage>(
                m => newEntity = new ProjectileEntity(m.Id, m.OwnerId, GetOrNull(m.OwnerId)));
            message.On<StartUserProjectileServerMessage>(
                m => newEntity = new ProjectileEntity(m.Id, m.OwnerId, GetOrNull(m.OwnerId)));

            if (newEntity != null)
            {
                byte[] data = message.Data.Array;
                Console.WriteLine("######");
                foreach (var b in data)
                {
                    Console.Write(b+"-");
                }
                Console.WriteLine("######");
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
            var entity = GetOrNull(id);
            if (entity != null)
                return entity;
            return new PlaceHolderEntity(id);
        }
    }
}