// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.PacketLog
{
    internal enum BlockType : byte
    {
        MagicBytes = 1,
        Start = 2,
        Timestamp = 3,
        Client = 4,
        Server = 5,
        Region = 6
    }
}
