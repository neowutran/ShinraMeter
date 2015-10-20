using System;

namespace NetworkSniffer.Packets
{
    [Flags]
    public enum TcpFlags : byte
    {
        Cwr = 1 << 7,
        Ecu = 1 << 6,
        Urg = 1 << 5,
        Ack = 1 << 4,
        Psh = 1 << 3,
        Rst = 1 << 2,
        Syn = 1 << 1,
        Fin = 1 << 0
    }
}