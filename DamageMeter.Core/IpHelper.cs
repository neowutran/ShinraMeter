using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace DamageMeter
{
    public class IpHelper
    {
        /// <summary>
        ///     Error codes GetIpNetTable returns that we recognise
        /// </summary>
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        /// <summary>
        ///     GetIpNetTable external method
        /// </summary>
        /// <param name="pIpNetTable"></param>
        /// <param name="pdwSize"></param>
        /// <param name="bOrder"></param>
        /// <returns></returns>
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int GetIpNetTable(IntPtr pIpNetTable,
            [MarshalAs(UnmanagedType.U4)] ref int pdwSize, bool bOrder);

        /// <summary>
        ///     Get the IP and MAC addresses of all known devices on the LAN
        /// </summary>
        /// <remarks>
        ///     1) This table is not updated often - it can take some human-scale time
        ///     to notice that a device has dropped off the network, or a new device
        ///     has connected.
        ///     2) This discards non-local devices if they are found - these are multicast
        ///     and can be discarded by IP address range.
        /// </remarks>
        /// <returns></returns>
        public static Dictionary<IPAddress, PhysicalAddress> GetAllDevicesOnLan()
        {
            var all = new Dictionary<IPAddress, PhysicalAddress> {{GetIPAddress(), GetMacAddress()}};
            // Add this PC to the list...
            var spaceForNetTable = 0;
            // Get the space needed
            // We do that by requesting the table, but not giving any space at all.
            // The return value will tell us how much we actually need.
            GetIpNetTable(IntPtr.Zero, ref spaceForNetTable, false);
            // Allocate the space
            // We use a try-finally block to ensure release.
            var rawTable = IntPtr.Zero;
            try
            {
                rawTable = Marshal.AllocCoTaskMem(spaceForNetTable);
                // Get the actual data
                var errorCode = GetIpNetTable(rawTable, ref spaceForNetTable, false);
                if (errorCode != 0)
                {
                    // Failed for some reason - can do no more here.
                    throw new Exception($"Unable to retrieve network table. Error code {errorCode}");
                }
                // Get the rows count
                var rowsCount = Marshal.ReadInt32(rawTable);
                var currentBuffer = new IntPtr(rawTable.ToInt64() + Marshal.SizeOf(typeof (int)));
                // Convert the raw table to individual entries
                var rows = new MIB_IPNETROW[rowsCount];
                for (var index = 0; index < rowsCount; index++)
                {
                    rows[index] = (MIB_IPNETROW) Marshal.PtrToStructure(new IntPtr(currentBuffer.ToInt64() +
                                                                                   (index*
                                                                                    Marshal.SizeOf(typeof (MIB_IPNETROW)))
                        ),
                        typeof (MIB_IPNETROW));
                }
                // Define the dummy entries list (we can discard these)
                var virtualMac = new PhysicalAddress(new byte[] {0, 0, 0, 0, 0, 0});
                var broadcastMac = new PhysicalAddress(new byte[] {255, 255, 255, 255, 255, 255});
                foreach (var row in rows)
                {
                    var ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                    var rawMac = new[] {row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5};
                    var pa = new PhysicalAddress(rawMac);
                    if (pa.Equals(virtualMac) || pa.Equals(broadcastMac) || IsMulticast(ip)) continue;
                    //Console.WriteLine("IP: {0}\t\tMAC: {1}", ip.ToString(), pa.ToString());
                    if (!all.ContainsKey(ip))
                    {
                        all.Add(ip, pa);
                    }
                }
            }
            finally
            {
                // Release the memory.
                Marshal.FreeCoTaskMem(rawTable);
            }
            return all;
        }

        /// <summary>
        ///     Gets the IP address of the current PC
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetIPAddress()
        {
            var strHostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(strHostName);
            var addr = ipEntry.AddressList;
            foreach (var ip in addr.Where(ip => !ip.IsIPv6LinkLocal))
            {
                return (ip);
            }
            return addr.Length > 0 ? addr[0] : null;
        }

        /// <summary>
        ///     Gets the MAC address of the current PC.
        /// </summary>
        /// <returns></returns>
        public static PhysicalAddress GetMacAddress()
        {
            return (from nic in NetworkInterface.GetAllNetworkInterfaces()
                where
                    nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress()).FirstOrDefault();
        }

        /// <summary>
        ///     Returns true if the specified IP address is a multicast address
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsMulticast(IPAddress ip)
        {
            var result = true;
            if (ip.IsIPv6Multicast) return true;
            var highIp = ip.GetAddressBytes()[0];
            if (highIp < 224 || highIp > 239)
            {
                result = false;
            }
            return result;
        }

        public static IPAddress GetIPAddress(PhysicalAddress physicalAddress)
        {
            var localIPs = GetAllDevicesOnLan();
            return (from pair in localIPs where pair.Value.Equals(physicalAddress) select pair.Key).FirstOrDefault();
        }

        /// <summary>
        ///     MIB_IPNETROW structure returned by GetIpNetTable
        ///     DO NOT MODIFY THIS STRUCTURE.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_IPNETROW
        {
            [MarshalAs(UnmanagedType.U4)] public readonly int dwIndex;
            [MarshalAs(UnmanagedType.U4)] public readonly int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac0;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac1;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac2;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac3;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac4;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac5;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac6;
            [MarshalAs(UnmanagedType.U1)] public readonly byte mac7;
            [MarshalAs(UnmanagedType.U4)] public readonly int dwAddr;
            [MarshalAs(UnmanagedType.U4)] public readonly int dwType;
        }
    }
}