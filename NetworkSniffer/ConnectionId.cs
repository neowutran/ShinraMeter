using System;
using System.Net;

namespace NetworkSniffer
{
    internal struct ConnectionId : IEquatable<ConnectionId>
    {
        public readonly IPEndPoint Source;
        public readonly IPEndPoint Destination;

        public ConnectionId(string sourceIp, ushort sourcePort, string destinationIp, ushort destinationPort)
        {
            Source = new IPEndPoint(IPAddress.Parse(sourceIp), sourcePort);
            Destination = new IPEndPoint(IPAddress.Parse(destinationIp), destinationPort);
        }

        public static bool operator ==(ConnectionId x, ConnectionId y)
        {
            return (x.Source.Equals(y.Source)) && (x.Destination.Equals(y.Destination));
        }

        public static bool operator !=(ConnectionId x, ConnectionId y)
        {
            return !(x == y);
        }

        public bool Equals(ConnectionId other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is ConnectionId)
                return Equals((ConnectionId) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode()*37 + Destination.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Source} -> {Destination}";
        }
    }
}