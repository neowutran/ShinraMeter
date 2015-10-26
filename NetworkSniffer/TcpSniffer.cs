using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using PacketDotNet;
using PacketDotNet.Utils;

namespace NetworkSniffer
{
    public class TcpSniffer
    {
        public string TcpLogFile { get; set; }
        private readonly object _lock = new object();

        public event Action<TcpConnection> NewConnection;

        protected void OnNewConnection(TcpConnection connection)
        {
            var handler = NewConnection;
            if (handler != null)
                handler(connection);
        }

        private readonly Dictionary<ConnectionId, TcpConnection> _connections = new Dictionary<ConnectionId, TcpConnection>();

        private void Receive(IpPacket ipPacket)
        {
            var protocol = ipPacket.Protocol;
            if (protocol != IPProtocolType.TCP)
                return;
            if (ipPacket.PayloadPacket == null)
            {
                Console.WriteLine(ipPacket);
                return;
            }
            var tcpPacket = new TcpPacket(new ByteArraySegment(ipPacket.PayloadPacket.BytesHighPerformance));

            bool isFirstPacket = tcpPacket.Syn;
            var source = ipPacket.SourceAddress.GetAddressBytes();
            string sourceIp = new IPAddress(source).ToString();
            Console.WriteLine(sourceIp);

            var destination = ipPacket.DestinationAddress.GetAddressBytes();
            string destinationIp = new IPAddress(destination).ToString();
            Console.WriteLine(destinationIp);

            var connectionId = new ConnectionId(sourceIp, tcpPacket.SourcePort, destinationIp, tcpPacket.DestinationPort);

            lock (_lock)
            {
                TcpConnection connection;
                bool isInterestingConnection;
                if (isFirstPacket)
                {
                    connection = new TcpConnection(connectionId, tcpPacket.SequenceNumber);
                    OnNewConnection(connection);
                    isInterestingConnection = connection.HasSubscribers;
                    if (!isInterestingConnection)
                        return;
                    _connections[connectionId] = connection;
                    Debug.Assert(ipPacket.PayloadPacket.PayloadData.Length == 0);
                }
                else
                {
                    isInterestingConnection = _connections.TryGetValue(connectionId, out connection);
                    if (!isInterestingConnection)
                        return;

                    if (!string.IsNullOrEmpty(TcpLogFile))
                        File.AppendAllText(TcpLogFile, string.Format("{0} {1}+{4} | {2} {3}+{4} ACK {5} ({6})\r\n", connection.CurrentSequenceNumber, tcpPacket.SequenceNumber, connection.BytesReceived, connection.SequenceNumberToBytesReceived(tcpPacket.SequenceNumber), ipPacket.PayloadLength, tcpPacket.AcknowledgmentNumber, connection.BufferedPacketDescription));
                    connection.HandleTcpReceived(tcpPacket.SequenceNumber, new ByteArraySegment(ipPacket.PayloadPacket.PayloadData));
                }
            }
        }

        public TcpSniffer(IpSniffer ipSniffer)
        {
            ipSniffer.PacketReceived += Receive;
        }
    }
}
