using System;
using System.Diagnostics.Contracts;
using System.Net;

namespace NetworkSniffer
{
    internal struct EndpointIpv4 : IEquatable<EndpointIpv4>
    {
        private readonly string _ip;
        private readonly ushort _port;

        public static bool operator ==(EndpointIpv4 x, EndpointIpv4 y)
        {
            return (x._ip == y._ip) && (x._port == y._port);
        }

        public static bool operator !=(EndpointIpv4 x, EndpointIpv4 y)
        {
            return !(x == y);
        }

        public bool Equals(EndpointIpv4 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is EndpointIpv4)
                return Equals((EndpointIpv4)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int)(_ip.GetHashCode() + (uint)_port * 1397));
        }

        public EndpointIpv4(string ip, ushort port)
        {
            _ip = ip;
            _port = port;
        }

        [Pure]
        public IPEndPoint ToIpEndpoint()
        {
            return new IPEndPoint(IPAddress.Parse(_ip), _port);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", _ip, _port);
        }
    }
}