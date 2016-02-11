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
        public EntityId(ulong id)
        {
            Id = id;
        }

        public ulong Id { get; }

        public override string ToString()
        {
            // fixme: uses native endian instead of little endian
            return BitConverter.ToString(BitConverter.GetBytes(Id).ToArray());
        }

        public static bool operator ==(EntityId x, EntityId y)
        {
            return x.Id == y.Id;
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
            return this == (EntityId) obj;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static readonly EntityId Empty = new EntityId(0);
    }
}