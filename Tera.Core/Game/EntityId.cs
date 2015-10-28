using System;
using System.Linq;

namespace Tera.Game
{
    // Identifies an entity
    // It might be a good idea to split this into two 32 bit words, since one of them seems to be the actual Id, the other consisting of flags.
    public struct EntityId
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
    }
}