// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Tera.Game
{
    // Identifies an entity
    // It might be a good idea to split this into two 32 bit words, since one of them seems to be the actual Id, the other consisting of flags.
    public struct EntityId : IEquatable<EntityId>
    {
        private readonly ulong _id;

        public EntityId(ulong id)
        {
            _id = id;
        }

        public override string ToString()
        {
            // fixme: uses native endian instead of little endian
            return BitConverter.ToString(BitConverter.GetBytes(_id).ToArray());
        }

        public static bool operator ==(EntityId x, EntityId y)
        {
            return x._id == y._id;
        }

        public static bool operator !=(EntityId x, EntityId y)
        {
            return !(x == y);
        }

        public bool Equals(EntityId other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityId))
                return false;
            return this == (EntityId)obj;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static readonly EntityId Empty = new EntityId(0);
    }
}
