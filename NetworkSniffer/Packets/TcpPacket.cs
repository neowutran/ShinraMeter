using System;

namespace NetworkSniffer.Packets
{
    public struct TcpPacket
    {
        public readonly ArraySegment<byte> Packet;
        public ushort SourcePort { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 0); } }
        public ushort DestinationPort { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 2); } }
        public uint SequenceNumber { get { return ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 4); } }
        public uint AcknowledgementNumber { get { return ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 8); } }
        public byte OffsetAndFlags { get { return Packet.Array[Packet.Offset + 12]; } }
        public TcpFlags Flags { get { return (TcpFlags)Packet.Array[Packet.Offset + 13]; } }
        public ushort WindowSize { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 14); } }
        public ushort Checksum { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 16); } }
        public ushort UrgentPointer { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 18); } }

        public int HeaderLength
        {
            get
            {
                var lengthInWords = OffsetAndFlags >> 4;
                if (lengthInWords < 5)
                    throw new FormatException("Incorrect TcpHeader length");
                return lengthInWords * 4;
            }
        }

        public int OptionsLength { get { return HeaderLength - 20; } }

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
