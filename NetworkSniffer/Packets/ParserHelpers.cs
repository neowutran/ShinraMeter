// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetworkSniffer.Packets
{
    internal static class ParserHelpers
    {
        public static uint GetUInt32BigEndian(byte[] array, int offset)
        {
            return ((uint) array[offset + 0] << 24) |
                   ((uint) array[offset + 1] << 16) |
                   ((uint) array[offset + 2] << 8) |
                   array[offset + 3];
        }

        public static ushort GetUInt16BigEndian(byte[] array, int offset)
        {
            return (ushort) (
                (array[offset + 0] << 8) |
                array[offset + 1]);
        }
    }
}