using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NetworkSniffer.Packets;

namespace NetworkSniffer
{
    public class TcpSniffer
    {
        private readonly Dictionary<ConnectionId, TcpConnection> _connections =
            new Dictionary<ConnectionId, TcpConnection>();

        private readonly object _lock = new object();
        private readonly object _bufferLock = new object();
        private string SnifferType;
        private List<Tuple<TcpConnection,TcpPacket>> _buffer =new List<Tuple<TcpConnection, TcpPacket>>();
        public TcpSniffer(IpSniffer ipSniffer)
        {
            ipSniffer.PacketReceived += Receive;
            SnifferType = ipSniffer.GetType().FullName;
            Task.Run(()=>ParsePacketsLoop());
        }

        public string TcpLogFile { get; set; }

        public event Action<TcpConnection> NewConnection;

        protected void OnNewConnection(TcpConnection connection)
        {
            var handler = NewConnection;
            handler?.Invoke(connection);
        }

        internal void RemoveConnection(TcpConnection connection)
        {
            lock (_lock) if (_connections.ContainsValue(connection))
                             _connections.Remove(_connections.First(x => x.Value == connection).Key);
        }

        private void ParsePacketsLoop()
        {
            while (true)
            {
                bool sleep = true;
                lock (_bufferLock) if (_buffer.Count != 0) sleep = false;
                if (sleep) System.Threading.Thread.Sleep(1);
                else
                {
                    List<Tuple<TcpConnection, TcpPacket>> toProcess;
                    lock (_bufferLock)
                    {
                        toProcess = _buffer;
                        _buffer = new List<Tuple<TcpConnection, TcpPacket>>();
                    }
                    foreach (var packet in toProcess)
                        packet.Item1.HandleTcpReceived(packet.Item2.SequenceNumber, packet.Item2.Payload);
                }
            }
        }

        private void Receive(ArraySegment<byte> ipData)
        {
            var ipPacket = new Ip4Packet(ipData);
            var protocol = ipPacket.Protocol;
            if (protocol != IpProtocol.Tcp) return;
            var tcpPacket = new TcpPacket(ipPacket.Payload);
            if (tcpPacket.Bad) return;
            var isFirstPacket = (tcpPacket.Flags & TcpFlags.Syn) != 0;
            var connectionId = new ConnectionId(ipPacket.SourceIp, tcpPacket.SourcePort, ipPacket.DestinationIp, tcpPacket.DestinationPort);


            TcpConnection connection;
            bool isInterestingConnection;
            if (isFirstPacket)
            {
                connection = new TcpConnection(connectionId, tcpPacket.SequenceNumber, RemoveConnection, SnifferType);
                OnNewConnection(connection);
                isInterestingConnection = connection.HasSubscribers;
                if (!isInterestingConnection) return;
                lock (_lock) _connections[connectionId] = connection;
                Debug.Assert(tcpPacket.Payload.Count == 0);
            }
            else
            {
                lock (_lock) isInterestingConnection = _connections.TryGetValue(connectionId, out connection);
                if (!isInterestingConnection) return;
                lock (_bufferLock) _buffer.Add(Tuple.Create(connection,tcpPacket));
                //if (!string.IsNullOrEmpty(TcpLogFile))
                //    File.AppendAllText(TcpLogFile,
                //        string.Format("{0} {1}+{4} | {2} {3}+{4} ACK {5} ({6})\r\n",
                //            connection.CurrentSequenceNumber, tcpPacket.SequenceNumber, connection.BytesReceived,
                //            connection.SequenceNumberToBytesReceived(tcpPacket.SequenceNumber),
                //            tcpPacket.Payload.Count, tcpPacket.AcknowledgementNumber,
                //            connection.BufferedPacketDescription));
            }
        }
    }
}