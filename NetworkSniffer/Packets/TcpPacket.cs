// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace NetworkSniffer.Packets
{
    public struct TcpPacket
    {
        public readonly ArraySegment<byte> Packet;
        public ushort SourcePort => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 0);
        public ushort DestinationPort => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 2);
        public uint SequenceNumber => ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 4);
        public uint AcknowledgementNumber => ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 8);
        public byte OffsetAndFlags => Packet.Array[Packet.Offset + 12];
        public TcpFlags Flags => (TcpFlags) Packet.Array[Packet.Offset + 13];
        public ushort WindowSize => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 14);
        public ushort Checksum => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 16);
        public ushort UrgentPointer => ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 18);
        public bool Bad => Packet.Count < HeaderLength;

        public int HeaderLength
        {
            get
            {
                var lengthInWords = OffsetAndFlags >> 4;
                if (lengthInWords < 5)
                    return int.MaxValue; // do not throw exception, but return something bigger than any packet to set Bad=true
                    //throw new FormatException("Incorrect TcpHeader length");
                return lengthInWords*4;
            }
        }

        public int OptionsLength => HeaderLength - 20;

        public ArraySegment<byte> Options
        {
            get
            {
                var optionsLength = OptionsLength;
                return new ArraySegment<byte>(Packet.Array, 20, optionsLength);
            }
        }

        public ArraySegment<byte> Payload
        {
            get
            {
                var headerLength = HeaderLength;
                return new ArraySegment<byte>(Packet.Array, Packet.Offset + headerLength, Packet.Count - headerLength);
            }
        }

        public TcpPacket(ArraySegment<byte> packet)
        {
            Packet = packet;
        }
    }
}