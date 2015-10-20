using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Messages;

namespace Tera.Protocol.Game
{
    public class EntityRegistry:IEnumerable<Entity>
    {
        private readonly Dictionary<EntityId, Entity> _dictionary = new Dictionary<EntityId, Entity>();

        public void Update(ParsedMessage message)
        {
            if (message is SpawnUserServerMessage)
                Update((SpawnUserServerMessage)message);
            if (message is LoginServerMessage)
                Update((LoginServerMessage)message);
            if (message is SpawnNpcServerMessage)
                Update((SpawnNpcServerMessage)message);
            if (message is SpawnProjectileServerMessage)
                Update((SpawnProjectileServerMessage)message);
        }

        public void Update(SpawnUserServerMessage message)
        {
            _dictionary[message.Id] = new User(message);
        }

        public void Update(LoginServerMessage message)
        {
            _dictionary[message.Id] = new User(message);
        }

        public void Update(SpawnNpcServerMessage message)
        {
            _dictionary[message.Id] = new Npc(message);
        }

        public void Update(SpawnProjectileServerMessage message)
        {
            _dictionary[message.Id] = new Projectile(message);
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
