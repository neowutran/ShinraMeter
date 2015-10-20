using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkSniffer
{
    public class IpSnifferRawSocketMultipleInterfaces : IpSniffer
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

        readonly List<IpSnifferRawSocketSingleInterface> _individualSniffers = new List<IpSnifferRawSocketSingleInterface>();

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
    }
}