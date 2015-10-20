using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Protocol
{
    internal enum BlockType : byte
    {
        MagicBytes = 1,
        Start = 2,
        Timestamp = 3,
        Client = 4,
        Server = 5
    }
}
