// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace NetworkSniffer
{
    internal struct ConnectionId : IEquatable<ConnectionId>
    {
        public readonly EndpointIpv4 Source;
        public readonly EndpointIpv4 Destination;

        public ConnectionId(uint sourceIp, ushort sourcePort, uint destinationIp, ushort destinationPort)
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
            return string.Format("{0} -> {1}", Source, Destination);
        }
    }
}