using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Data;
using NetworkSniffer.Packets;

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

        private void Receive(ArraySegment<byte> ipData)
        {
            var ipPacket = new Ip4Packet(ipData);
            var protocol = ipPacket.Protocol;
            if (protocol != IpProtocol.Tcp)
                return;
            var tcpPacket = new TcpPacket(ipPacket.Payload);
            if (tcpPacket.Bad) return;
            var isFirstPacket = (tcpPacket.Flags & TcpFlags.Syn) != 0;
            var connectionId = new ConnectionId(ipPacket.SourceIp, tcpPacket.SourcePort, ipPacket.DestinationIp,
                tcpPacket.DestinationPort);


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
                    Debug.Assert(tcpPacket.Payload.Count == 0);
                }
                else
                {
                    isInterestingConnection = _connections.TryGetValue(connectionId, out connection);
                    if (!isInterestingConnection)
                        return;

                    if ((ipPacket.Flags & 1) == 1)
                    {
                        BasicTeraData.LogError("Fragmented packet");
                    }

                    if (!string.IsNullOrEmpty(TcpLogFile))
                        File.AppendAllText(TcpLogFile,
                            string.Format("{0} {1}+{4} | {2} {3}+{4} ACK {5} ({6})\r\n",
                                connection.CurrentSequenceNumber, tcpPacket.SequenceNumber, connection.BytesReceived,
                                connection.SequenceNumberToBytesReceived(tcpPacket.SequenceNumber),
                                tcpPacket.Payload.Count, tcpPacket.AcknowledgementNumber,
                                connection.BufferedPacketDescription));
                    connection.HandleTcpReceived(tcpPacket.SequenceNumber, tcpPacket.Payload);
                }
            }
        }
    }
}