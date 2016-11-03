// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PacketDotNet;

namespace NetworkSniffer
{
    public abstract class IpSniffer
    {
        private bool _enabled;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    SetEnabled(value);
                }
            }
        }

        public event Action<IPv4Packet> PacketReceived;

        protected void OnPacketReceived(IPv4Packet data)
        {
            var packetReceived = PacketReceived;
            packetReceived?.Invoke(data);
        }

        protected abstract void SetEnabled(bool value);
    }
}