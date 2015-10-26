using System;

namespace NetworkSniffer
{
    internal struct ConnectionId : IEquatable<ConnectionId>
    {
        public readonly EndpointIpv4 Source;
        public readonly EndpointIpv4 Destination;

        public ConnectionId(string sourceIp, ushort sourcePort, string destinationIp, ushort destinationPort)
        {
            Source = new EndpointIpv4(sourceIp, sourcePort);
            Destination = new EndpointIpv4(destinationIp, destinationPort);
        }

        public static bool operator ==(ConnectionId x, ConnectionId y)
        {
            return (x.Source == y.Source) && (x.Destination == y.Destination);
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
                return Equals((ConnectionId)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode() * 37 + Destination.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Source} -> {Destination}";
        }
    }
}