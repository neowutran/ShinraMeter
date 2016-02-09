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
    public class IpSnifferRawSocketMultipleInterfaces : IpSniffer
    {
        private readonly List<IpSnifferRawSocketSingleInterface> _individualSniffers =
            new List<IpSnifferRawSocketSingleInterface>();

        private readonly IEnumerable<IPAddress> _ipAddresses;

        public IpSnifferRawSocketMultipleInterfaces()
            : this(DefaultInterfaceIPs)
        {
        }

        public IpSnifferRawSocketMultipleInterfaces(IEnumerable<IPAddress> ipAddresses)
        {
            _ipAddresses = ipAddresses;
        }

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

        public static IEnumerable<IPAddress> DefaultInterfaceIPs => AllInterfaceIPs;

        protected override void SetEnabled(bool value)
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


        public event Action<string> Warning;

        protected virtual void OnWarning(string obj)
        {
            var handler = Warning;
            handler?.Invoke(obj);
        }
    }
}