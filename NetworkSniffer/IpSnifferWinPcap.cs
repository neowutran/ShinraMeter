// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    public class IpSnifferWinPcap : IpSniffer
    {
        private readonly string _filter;
        private WinPcapDeviceList _devices;
        private volatile uint _droppedPackets;
        private volatile uint _interfaceDroppedPackets;

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
            _devices = null;
        }

        void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (e.Device.LinkType != LinkLayers.Ethernet)
                return;
            var ethernetPacket = new EthernetPacket(new ByteArraySegment(e.Packet.Data));

            var ipPacket = ethernetPacket.PayloadPacket as IpPacket;
            if (ipPacket == null)
                return;

            var ipData = ipPacket.BytesHighPerformance;
            var ipData2 = new ArraySegment<byte>(ipData.Bytes, ipData.Offset, ipData.Length);

            OnPacketReceived(ipData2);

            var device = (WinPcapDevice)sender;
            if (device.Statistics.DroppedPackets != _droppedPackets || device.Statistics.InterfaceDroppedPackets != _interfaceDroppedPackets)
            {
                _droppedPackets = device.Statistics.DroppedPackets;
                _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
                File.AppendAllText("TCP", string.Format("DroppedPackets {0}, InterfaceDroppedPackets {1}", device.Statistics.DroppedPackets, device.Statistics.InterfaceDroppedPackets));
            }
        }
    }
}
