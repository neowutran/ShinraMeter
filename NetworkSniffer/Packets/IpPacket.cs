using System;

namespace NetworkSniffer.Packets
{
    public enum IpProtocol : byte
    {
        Tcp = 6,
        Udp = 17
    }

    public struct Ip4Packet
    {
        public readonly ArraySegment<byte> Packet;
        public byte VersionAndHeaderLength { get { return Packet.Array[Packet.Offset + 0]; } }
        public byte DscpAndEcn { get { return Packet.Array[Packet.Offset + 1]; } }
        public ushort TotalLength { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 2); } }
        public ushort Identification { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 4); } }
        public byte Flags { get { return (byte)(Packet.Array[Packet.Offset + 6] >> 13); } }
        public ushort FragmentOffset { get { return (ushort)(ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 6) & 0x1FFF); } }
        public byte TimeToLive { get { return Packet.Array[Packet.Offset + 8]; } }
        public IpProtocol Protocol { get { return (IpProtocol)Packet.Array[Packet.Offset + 9]; } }
        public ushort HeaderChecksum { get { return ParserHelpers.GetUInt16BigEndian(Packet.Array, Packet.Offset + 10); } }
        public uint SourceIp { get { return ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 12); } }
        public uint DestinationIp { get { return ParserHelpers.GetUInt32BigEndian(Packet.Array, Packet.Offset + 16); } }

        public int HeaderLength { get { return (VersionAndHeaderLength & 0x0F) * 4; } }


        public ArraySegment<byte> Payload
        {
            get
            {
                var headerLength = HeaderLength;
                return new ArraySegment<byte>(Packet.Array, Packet.Offset + headerLength, TotalLength - headerLength);
            }
        }

        public Ip4Packet(ArraySegment<byte> packet)
        {
            Packet = packet;
        }
    }
}