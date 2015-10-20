using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Protocol.Game
{
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
