using System.Collections;
using System.Collections.Generic;
using Tera.Game.Messages;

namespace Tera.Game
{
    // Tracks which entities we have seen so far and what their properties are
    public class EntityRegistry : IEnumerable<Entity>
    {
        private readonly Dictionary<EntityId, Entity> _dictionary = new Dictionary<EntityId, Entity>();

        public void Update(ParsedMessage message)
        {
            Entity newEntity = null;
            message.On<SpawnUserServerMessage>(m => newEntity = new User(m));
            message.On<LoginServerMessage>(m => newEntity = new User(m));
            message.On<SpawnNpcServerMessage>(m => newEntity = new Npc(m));
            message.On<SpawnProjectileServerMessage>(m => newEntity = new Projectile(m.Id, m.OwnerId, Get(m.OwnerId)));
            if (newEntity != null)
            {
                _dictionary[newEntity.Id] = newEntity;
            }
        }

        public Entity Get(EntityId id)
        {
            Entity entity;
            _dictionary.TryGetValue(id, out entity);
            return entity;
        }

        public Entity GetOrPlaceholder(EntityId id)
        {
            var entity = Get(id);
            if (entity != null)
                return entity;
            else
                return new Entity(id);
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
