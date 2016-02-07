// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkSniffer
{
    public class IpSnifferRawSocketMultipleInterfaces
    {
        private readonly IEnumerable<IPAddress> _ipAddresses;

        public static IEnumerable<IPAddress> AllInterfaceIPs
        {
            get
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                                       .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                                       .Select(x => x.Address)
                                       .Where(x => x.AddressFamily == AddressFamily.InterNetwork);
            }
        }

        private bool _enabled;


        public static IEnumerable<IPAddress> DefaultInterfaceIPs
        {
            get
            {
                return AllInterfaceIPs.Where(x => x.ToString() != "127.0.0.1");
            }
        }

        public IpSnifferRawSocketMultipleInterfaces()
            :this(DefaultInterfaceIPs)
        {
        }

        public IpSnifferRawSocketMultipleInterfaces(IEnumerable<IPAddress> ipAddresses)
        {
            _ipAddresses = ipAddresses;
        }

        private readonly List<IpSnifferRawSocketSingleInterface> _individualSniffers = new List<IpSnifferRawSocketSingleInterface>();

        protected void SetEnabled(bool value)
        {
            if (value)
            {
                foreach (var localIp in _ipAddresses)
                {
                    var individualSniffer = new IpSnifferRawSocketSingleInterface(localIp);
                    individualSniffer.PacketReceived += OnPacketReceived;
                    _individualSniffers.Add(individualSniffer);
                }
                foreach (var individualSniffer in _individualSniffers)
                {
                    individualSniffer.Enabled = true;
                }
            }
            else
            {
                foreach (var individualSniffer in _individualSniffers)
                {
                    individualSniffer.Enabled = false;
                }
                _individualSniffers.Clear();
            }
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
      
        public event Action<string> Warning;

        protected virtual void OnWarning(string obj)
        {
            var handler = Warning;
            handler?.Invoke(obj);
        }

        public event Action<ArraySegment<byte>, Socket> PacketReceived;

        protected void OnPacketReceived(ArraySegment<byte> data, Socket device)
        {
            var packetReceived = PacketReceived;
            packetReceived?.Invoke(data, device);
        }

    }
}