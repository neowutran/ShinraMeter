// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace NetworkSniffer
{
    public abstract class IpSniffer
    {
        public event Action<ArraySegment<byte>> PacketReceived;

        protected void OnPacketReceived(ArraySegment<byte> data)
        {
            var packetReceived = PacketReceived;
            if (packetReceived != null)
                packetReceived(data);
        }

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

        protected abstract void SetEnabled(bool value);
    }
}
