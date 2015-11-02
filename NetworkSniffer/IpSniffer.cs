// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        private volatile uint _interfaceDroppedPackets;

        public IpSniffer(string filter)
        {
            _filter = filter;
        }

        public IEnumerable<string> Status()
        {
            return _devices.Select(device => string.Format("Device {0} {1} {2}\r\n{3}", device.LinkType, device.Opened ? "Open" : "Closed", device.LastError, device));
        }

        protected void SetEnabled(bool value)
        {
            if (value)
            {
                Start();
            }
            else
            {
                Finish();
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

        public event Action<string> Warning;

        protected virtual void OnWarning(string obj)
        {
            Action<string> handler = Warning;
            if (handler != null) handler(obj);
        }
        public event Action<IpPacket> PacketReceived;

        protected void OnPacketReceived(IpPacket data)
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
        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (e.Device.LinkType != LinkLayers.Ethernet)
                return;
            var ethernetPacket = new EthernetPacket(new ByteArraySegment(e.Packet.Data));

            var ipPacket = ethernetPacket.PayloadPacket as IpPacket;
            if (ipPacket == null)
                return;

            OnPacketReceived(ipPacket);

            var device = (WinPcapDevice)sender;
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
