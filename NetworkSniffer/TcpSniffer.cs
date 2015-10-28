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
        private readonly Dictionary<ConnectionId, TcpConnection> _connections =
            new Dictionary<ConnectionId, TcpConnection>();

        private readonly object _lock = new object();

        public TcpSniffer(IpSniffer ipSniffer)
        {
            ipSniffer.PacketReceived += Receive;
        }

        public string TcpLogFile { get; set; }

        public event Action<TcpConnection> NewConnection;

        protected void OnNewConnection(TcpConnection connection)
        {
            var handler = NewConnection;
            handler?.Invoke(connection);
        }

        private void Receive(IpPacket ipPacket)
        {
            var protocol = ipPacket.Protocol;
            if (protocol != IPProtocolType.TCP)
            {
                return;
            }
            if (ipPacket.PayloadPacket == null)
            {
                return;
            }
            var tcpPacket = new TcpPacket(new ByteArraySegment(ipPacket.PayloadPacket.BytesHighPerformance));

            var isFirstPacket = tcpPacket.Syn;
            var sourceIp = new IPAddress(ipPacket.SourceAddress.GetAddressBytes()).ToString();
            var destinationIp = new IPAddress(ipPacket.DestinationAddress.GetAddressBytes()).ToString();
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
                    {
                        return;
                    }
                    _connections[connectionId] = connection;
                    Debug.Assert(ipPacket.PayloadPacket.PayloadData.Length == 0);
                }
                else
                {
                    isInterestingConnection = _connections.TryGetValue(connectionId, out connection);
                    if (!isInterestingConnection)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(TcpLogFile))
                    {
                        File.AppendAllText(TcpLogFile,
                            string.Format("{0} {1}+{4} | {2} {3}+{4} ACK {5} ({6})\r\n",
                                connection.CurrentSequenceNumber, tcpPacket.SequenceNumber, connection.BytesReceived,
                                connection.SequenceNumberToBytesReceived(tcpPacket.SequenceNumber),
                                ipPacket.PayloadLength, tcpPacket.AcknowledgmentNumber,
                                connection.BufferedPacketDescription));
                    }
                    connection.HandleTcpReceived(tcpPacket.SequenceNumber,
                        new ByteArraySegment(ipPacket.PayloadPacket.PayloadData));
                }
            }
        }
    }
}