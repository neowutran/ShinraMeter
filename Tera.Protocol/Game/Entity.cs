using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Protocol.Game
{
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
