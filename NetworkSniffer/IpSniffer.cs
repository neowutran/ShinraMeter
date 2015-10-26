using System;
using PacketDotNet;

namespace NetworkSniffer
{
    public abstract class IpSniffer
    {
        public event Action<IpPacket> PacketReceived;

        protected void OnPacketReceived(IpPacket data)
        {
            var packetReceived = PacketReceived;
            packetReceived?.Invoke(data);
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                SetEnabled(value);
            }
        }

        protected abstract void SetEnabled(bool value);
    }
}
