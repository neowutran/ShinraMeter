// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
                if (_enabled != value)
                {
                    _enabled = value;
                    SetEnabled(value);
                }
            }
        }

        public IEnumerable<string> Status()
        {
            return
                _devices.Select(
                    device =>
                        $"Device {device.LinkType} {(device.Opened ? "Open" : "Closed")} {device.LastError}\r\n{device}");
        }

        protected void SetEnabled(bool value)
        {
            if (value)
            {
                Start();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private static bool IsInteresting(WinPcapDevice device)
        {
            return true;
        }

        private void Start()
        {
            Debug.Assert(_devices == null);
            _devices = WinPcapDeviceList.New();

            var interestingDevices = _devices.Where(IsInteresting);

            foreach (var device in interestingDevices)
            {
                device.OnPacketArrival += device_OnPacketArrival;
                device.Open(DeviceMode.Promiscuous, 1000);
                device.Filter = _filter;
                try
                {
                    device.KernelBufferSize = 2000000000;
                }
                catch
                {
                }
                device.StartCapture();
            }
        }

        public event Action<string> Warning;

        protected virtual void OnWarning(string obj)
        {
            var handler = Warning;
            handler?.Invoke(obj);
        }

        public event Action<IpPacket> PacketReceived;

        protected void OnPacketReceived(IpPacket data)
        {
            var packetReceived = PacketReceived;
            packetReceived?.Invoke(data);
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
                device.Statistics.InterfaceDroppedPackets == _interfaceDroppedPackets)
                return;
            _droppedPackets = device.Statistics.DroppedPackets;
            _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
            File.AppendAllText("TCP",
                $"DroppedPackets {device.Statistics.DroppedPackets}, InterfaceDroppedPackets {device.Statistics.InterfaceDroppedPackets}");
        }
    }
}