using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PacketDotNet;
using PacketDotNet.Utils;
using SharpPcap;
using SharpPcap.WinPcap;

namespace NetworkSniffer
{
    public class IpSnifferWinPcap : IpSniffer
    {
        private readonly string _filter;
        private WinPcapDeviceList _devices;

        public IpSnifferWinPcap(string filter)
        {
            _filter = filter;
        }

        protected override void SetEnabled(bool value)
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
        }

        private volatile uint droppedPackets;
        private volatile uint interfaceDroppedPackets;
        void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (e.Device.LinkType != LinkLayers.Ethernet)
                return;
            EthernetPacket ethernetPacket = new EthernetPacket(new ByteArraySegment(e.Packet.Data));

            var ipPacket = ethernetPacket.PayloadPacket as IpPacket;
            if (ipPacket == null)
                return;

            var ipData = ipPacket.BytesHighPerformance;
            var ipData2 = new ArraySegment<byte>(ipData.Bytes, ipData.Offset, ipData.Length);

            OnPacketReceived(ipData2);

            var device = (WinPcapDevice)sender;
            if (device.Statistics.DroppedPackets != droppedPackets || device.Statistics.InterfaceDroppedPackets != interfaceDroppedPackets)
            {
                droppedPackets = device.Statistics.DroppedPackets;
                interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
                File.AppendAllText("TCP", string.Format("DroppedPackets {0}, InterfaceDroppedPackets {1}", device.Statistics.DroppedPackets, device.Statistics.InterfaceDroppedPackets));
            }
        }
    }
}
