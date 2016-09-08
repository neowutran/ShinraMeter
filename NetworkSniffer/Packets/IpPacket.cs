// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace NetworkSniffer.Packets
{
    public enum IpProtocol : byte
    {
        Tcp = 6,
        Udp = 17,
        Error = 255
    }

    public struct Ip4Packet
    {
        public readonly ArraySegment<byte> Packet;
        public byte VersionAndHeaderLength => Packet.Array[Packet.Offset + 0];
        public byte DscpAndEcn => Packet.Array[Packet.Offset + 1];
        public ushort TotalLength => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 2);
        public ushort Identification => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 4);
        public byte Flags => (byte) (Packet.Array[Packet.Offset + 6] >> 13);

        public ushort FragmentOffset
            => (ushort) (ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 6) & 0x1FFF);

        public byte TimeToLive => Packet.Array[Packet.Offset + 8];

        public IpProtocol Protocol
            =>
                Packet.Offset + TotalLength > Packet.Array.Length || TotalLength <= HeaderLength
                    ? IpProtocol.Error
                    : (IpProtocol) Packet.Array[Packet.Offset + 9];

        public ushort HeaderChecksum => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 10);
        public uint SourceIp => ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 12);
        public uint DestinationIp => ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 16);

        public int HeaderLength => (VersionAndHeaderLength & 0x0F)*4;


        public ArraySegment<byte> Payload
        {
            get
            {
                var headerLength = HeaderLength;
                if (Packet.Offset + TotalLength > Packet.Array.Length || TotalLength <= headerLength)
                {
                    throw new Exception(
                        $"Wrong packet TotalLength:{TotalLength} headerLength:{headerLength} Packet.Array.Length:{Packet.Array.Length} SourceIp:{SourceIp} DestinationIp:{DestinationIp}");
                }
                return new ArraySegment<byte>(Packet.Array, Packet.Offset + headerLength, TotalLength - headerLength);
            }
        }

        public Ip4Packet(ArraySegment<byte> packet)
        {
            Packet = packet;
        }
    }
}   