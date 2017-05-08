// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using PacketDotNet;
using PacketDotNet.Utils;
using SharpPcap;
using SharpPcap.WinPcap;

namespace NetworkSniffer
{
    // Only works when WinPcap is installed
    public class IpSnifferWinPcap : IpSniffer
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _filter;
        private WinPcapDeviceList _devices;
        private volatile uint _droppedPackets;
        private volatile uint _interfaceDroppedPackets;
        private DateTime _nextCheck;

        public IpSnifferWinPcap(string filter)
        {
            _filter = filter;
            BufferSize = 1 << 24;
            _devices = WinPcapDeviceList.New();
            //BasicTeraData.LogError(string.Join("\r\n",_devices.Select(x=>x.Description)),true,true);
            //check for winpcap installed if not - exception to fallback to rawsockets
            _devices = null;
        }

        public int? BufferSize { get; set; }

        public IEnumerable<string> Status()
        {
            return _devices.Select(device => $"Device {device.LinkType} {(device.Opened ? "Open" : "Closed")} {device.LastError}\r\n{device}");
        }

        protected override void SetEnabled(bool value)
        {
            if (value) { Start(); }
            else { Finish(); }
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

                try { device.Open(DeviceMode.Normal, 100); }
                catch (Exception e)
                {
                    Logger.Warn($"Failed to open device {device.Name}. {e.Message}");
                    continue;
                }
                device.Filter = _filter;
                if (BufferSize != null)
                {
                    try { device.KernelBufferSize = (uint) BufferSize.Value; }
                    catch (Exception e) { Logger.Warn($"Failed to set KernelBufferSize to {BufferSize.Value} on {device.Name}. {e.Message}"); }
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
                try { device.StopCapture(); }
                catch
                {
                    // ignored
                }

                //SharpPcap.PcapException: captureThread was aborted after 00:00:02 - it's normal when there is no traffic while stopping
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
            IPv4Packet ipPacket;
            try
            {
                if (e.Packet.LinkLayerType != LinkLayers.Null)
                {
                    var linkPacket = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                    ipPacket = linkPacket.PayloadPacket as IPv4Packet;
                }
                else { ipPacket = new IPv4Packet(new ByteArraySegment(e.Packet.Data, 4, e.Packet.Data.Length - 4)); }
                if (ipPacket == null) { return; }
            }
            catch
            {
                return;
                // ignored bad packet
            }

            OnPacketReceived(ipPacket);

            var now = DateTime.UtcNow;
            if (now <= _nextCheck) { return; }
            _nextCheck = now + TimeSpan.FromSeconds(20);
            var device = (WinPcapDevice) sender;
            if (device.Statistics.DroppedPackets == _droppedPackets && device.Statistics.InterfaceDroppedPackets == _interfaceDroppedPackets) { return; }
            _droppedPackets = device.Statistics.DroppedPackets;
            _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
            OnWarning(
                $"DroppedPackets {device.Statistics.DroppedPackets}, InterfaceDroppedPackets {device.Statistics.InterfaceDroppedPackets}, ReceivedPackets {device.Statistics.ReceivedPackets}");
        }
    }
}