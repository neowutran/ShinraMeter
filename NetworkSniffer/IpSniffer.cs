using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PacketDotNet;
using PacketDotNet.Utils;
using SharpPcap;
using SharpPcap.WinPcap;

namespace NetworkSniffer
{
    // Only works when WinPcap is installed
    public class IpSniffer
    {
        private readonly string _filter;
        private WinPcapDeviceList _devices;
        private volatile uint _droppedPackets;

        private bool _enabled;
        private volatile uint _interfaceDroppedPackets;

        public IpSniffer(string filter)
        {
            _filter = filter;
        }

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

        public event Action<IpPacket> PacketReceived;

        protected void OnPacketReceived(IpPacket data)
        {
            var packetReceived = PacketReceived;
            packetReceived?.Invoke(data);
        }

        protected void SetEnabled(bool value)
        {
            if (value)
                Start();
            else
                Finish();
        }

        private void Start()
        {
            Debug.Assert(_devices == null);
            _devices = WinPcapDeviceList.New();
            var interestingDevices = _devices;
            foreach (var device in interestingDevices)
            {
                device.OnPacketArrival += device_OnPacketArrival;
                device.Open(DeviceMode.Promiscuous, 1000);
                device.Filter = _filter;
                device.StartCapture();
            }
        }

        private void Finish()
        {
            Debug.Assert(_devices != null);
            foreach (var device in _devices.Where(device => device.Opened))
            {
                device.StopCapture();
                device.Close();
            }
            _devices = null;
        }

        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (e.Device.LinkType != LinkLayers.Ethernet)
                return;
            var ethernetPacket = new EthernetPacket(new ByteArraySegment(e.Packet.Data));

            var ipPacket = ethernetPacket.PayloadPacket as IpPacket;
            if (ipPacket == null)
                return;

            OnPacketReceived(ipPacket);

            var device = (WinPcapDevice) sender;
            if (device.Statistics.DroppedPackets == _droppedPackets &&
                device.Statistics.InterfaceDroppedPackets == _interfaceDroppedPackets) return;
            _droppedPackets = device.Statistics.DroppedPackets;
            _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
            File.AppendAllText("TCP",
                $"DroppedPackets {device.Statistics.DroppedPackets}, InterfaceDroppedPackets {device.Statistics.InterfaceDroppedPackets}");
        }
    }
}