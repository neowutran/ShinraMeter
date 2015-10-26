using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using PacketDotNet.Utils;

namespace NetworkSniffer
{
    public class TcpConnection
    {
        public readonly IPEndPoint Source;
        public readonly IPEndPoint Destination;
        private readonly SortedDictionary<long, byte[]> _bufferedPackets = new SortedDictionary<long, byte[]>();
        public long BytesReceived { get; private set; }
        public uint InitialSequenceNumber { get; }

        public event Action<TcpConnection, ByteArraySegment> DataReceived;

        public bool HasSubscribers => DataReceived != null;
        internal string BufferedPacketDescription { get { return string.Join(", ", _bufferedPackets.OrderBy(x => x.Key).Select(x => x.Key + "+" + x.Value.Length)); } }
        public uint CurrentSequenceNumber => unchecked((uint) (InitialSequenceNumber + 1 + BytesReceived));

        public long SequenceNumberToBytesReceived(uint sequenceNumber)
        {
            var offsetToCurrent = unchecked((int)(sequenceNumber - CurrentSequenceNumber));
            return BytesReceived + offsetToCurrent;
        }

        internal void OnDataReceived(ByteArraySegment data)
        {
            var dataReceived = DataReceived;
            dataReceived?.Invoke(this, data);
        }

        internal void HandleTcpReceived(uint sequenceNumber, ByteArraySegment data)
        {
            var dataPosition = SequenceNumberToBytesReceived(sequenceNumber);
            if (dataPosition == BytesReceived)
            {
                OnDataReceived(data);
                BytesReceived += data.Length;
            }
            else
            {
                var dataArray = new byte[data.Length];
                Array.Copy(data.Bytes, data.Offset, dataArray, 0, data.Length);
                if (!_bufferedPackets.ContainsKey(dataPosition) || _bufferedPackets[dataPosition].Length < dataArray.Length)
                {
                    _bufferedPackets[dataPosition] = dataArray;
                }
            }

            long firstBufferedPosition;
            while (_bufferedPackets.Any() && ((firstBufferedPosition = _bufferedPackets.Keys.First()) <= BytesReceived))
            {
                var dataArray = _bufferedPackets[firstBufferedPosition];
                _bufferedPackets.Remove(firstBufferedPosition);

                var alreadyReceivedBytes = BytesReceived - firstBufferedPosition;
                Debug.Assert(alreadyReceivedBytes >= 0);

                if (alreadyReceivedBytes < dataArray.Length)
                {
                    var count = dataArray.Length - alreadyReceivedBytes;
                    OnDataReceived(new ByteArraySegment(dataArray, (int)alreadyReceivedBytes, (int)count));
                    BytesReceived += count;
                }
            }
        }

        internal TcpConnection(ConnectionId connectionId, uint sequenceNumber)
        {
            Source = connectionId.Source.ToIpEndpoint();
            Destination = connectionId.Destination.ToIpEndpoint();
            Console.WriteLine(Source);
            Console.WriteLine(Destination);
            InitialSequenceNumber = sequenceNumber;
        }

        public override string ToString()
        {
            return $"{Source} -> {Destination}";
        }
    }
}