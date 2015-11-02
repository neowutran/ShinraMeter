// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PacketDotNet;
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
        private int _bufferSize;

        public IpSnifferWinPcap(string filter)
        {
            _filter = filter;
        }

        public IEnumerable<string> Status()
        {
            return _devices.Select(device => string.Format("Device {0} {1} {2}\r\n{3}", device.LinkType, device.Opened ? "Open" : "Closed", device.LastError, device));
        }

        public int BufferSize { get; set; }

        protected override void SetEnabled(bool value)
        {
            if (value)
                Start();
            else
                Finish();
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
                device.KernelBufferSize = (uint)BufferSize;
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

        void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var linkPacket = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var ipPacket = linkPacket.PayloadPacket as IpPacket;
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
                OnWarning(string.Format("DroppedPackets {0}, InterfaceDroppedPackets {1}", device.Statistics.DroppedPackets, device.Statistics.InterfaceDroppedPackets));
            }
        }
    }
}
