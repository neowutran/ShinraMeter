// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using PacketDotNet;
using SharpPcap;
using SharpPcap.WinPcap;

namespace NetworkSniffer
{
    // Only works when WinPcap is installed
    public class IpSnifferWinPcap : IpSniffer
    {
        private static readonly ILog Logger = LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _filter;
        private WinPcapDeviceList _devices;
        private volatile uint _droppedPackets;
        private volatile uint _interfaceDroppedPackets;
        private List<string> _servers;

        public IpSnifferWinPcap(string filter, List<string> servers)
        {
            _filter = filter;
            _servers = servers;
            BufferSize = 8192*1024;
            _devices = WinPcapDeviceList.New();//check for winpcap installed if not - exception to fallback to rawsockets
            _devices = null;
        }

        public int? BufferSize { get; set; }

        public IEnumerable<string> Status()
        {
            return _devices.Select(device =>
                $"Device {device.LinkType} {(device.Opened ? "Open" : "Closed")} {device.LastError}\r\n{device}");
        }

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
                device.Open(DeviceMode.Normal, 1000);
                device.Filter = _filter;
                if (BufferSize != null)
                {
                    try
                    {
                        device.KernelBufferSize = (uint) BufferSize.Value;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn(
                            $"Failed to set KernelBufferSize to {BufferSize.Value} on {device.Name}. {e.Message}");
                    }
                }
                device.StartCapture();
                Console.WriteLine("winpcap capture");
            }
        }

        private void Finish()
        {
            Debug.Assert(_devices != null);
            foreach (var device in _devices.Where(device => device.Opened))
            {
                try { device.StopCapture(); } catch { };//SharpPcap.PcapException: captureThread was aborted after 00:00:02 - it's normal when there is no traffic while stopping
                device.Close();
            }
            _devices = null;
        }

        public event Action<string> Warning;

        protected virtual void OnWarning(string obj)
        {
            var handler = Warning;
            handler?.Invoke(obj);
        }

        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var linkPacket = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var ipPacket = linkPacket.PayloadPacket as IPv4Packet;
            if (ipPacket == null)
                return;

            if (_servers.IndexOf(ipPacket.SourceAddress.ToString()) != -1)
            {
                if (!ipPacket.ValidChecksum)
                {
                    return;
                }
            }

            var ipData = ipPacket.BytesHighPerformance;
            var ipData2 = new ArraySegment<byte>(ipData.Bytes, ipData.Offset, ipData.Length);

            OnPacketReceived(ipData2);

            var device = (WinPcapDevice) sender;
            if (device.Statistics.DroppedPackets == _droppedPackets &&
                device.Statistics.InterfaceDroppedPackets == _interfaceDroppedPackets)
            {
                return;
            }
            _droppedPackets = device.Statistics.DroppedPackets;
            _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
            OnWarning(
                $"DroppedPackets {device.Statistics.DroppedPackets}, InterfaceDroppedPackets {device.Statistics.InterfaceDroppedPackets}");
        }
    }
}