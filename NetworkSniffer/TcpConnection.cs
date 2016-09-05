using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Data;

namespace NetworkSniffer
{
    public class TcpConnection
    {
        private readonly SortedDictionary<long, byte[]> _bufferedPackets = new SortedDictionary<long, byte[]>();
        public readonly IPEndPoint Destination;
        public readonly IPEndPoint Source;

        internal TcpConnection(ConnectionId connectionId, uint sequenceNumber)
        {
            Source = connectionId.Source.ToIpEndpoint();
            Destination = connectionId.Destination.ToIpEndpoint();
            InitialSequenceNumber = sequenceNumber;
        }

        public long BytesReceived { get; private set; }
        public uint InitialSequenceNumber { get; }

        public bool HasSubscribers => DataReceived != null;

        internal string BufferedPacketDescription
        {
            get
            {
                return string.Join(", ", _bufferedPackets.OrderBy(x => x.Key).Select(x => x.Key + "+" + x.Value.Length));
            }
        }

        public uint CurrentSequenceNumber => unchecked((uint) (InitialSequenceNumber + 1 + BytesReceived));

        public static uint NextSequenceNumber { get; private set; }

        public event Action<TcpConnection, ArraySegment<byte>> DataReceived;

        public long SequenceNumberToBytesReceived(uint sequenceNumber)
        {
            var offsetToCurrent = unchecked((int) (sequenceNumber - CurrentSequenceNumber));
            return BytesReceived + offsetToCurrent;
        }

        internal void OnDataReceived(ArraySegment<byte> data)
        {
            var dataReceived = DataReceived;
            dataReceived?.Invoke(this, data);
        }

        internal void HandleTcpReceived(uint sequenceNumber, ArraySegment<byte> data)
        {
            var dataPosition = SequenceNumberToBytesReceived(sequenceNumber);
            NextSequenceNumber = (uint) (sequenceNumber + data.Count);
            if (dataPosition == BytesReceived)
            {
                OnDataReceived(data);
                BytesReceived += data.Count;
            }
            else
            {
                var dataArray = new byte[data.Count];
                Array.Copy(data.Array, data.Offset, dataArray, 0, data.Count);
                if (!_bufferedPackets.ContainsKey(dataPosition) ||
                    _bufferedPackets[dataPosition].Length < dataArray.Length)
                {
                    _bufferedPackets[dataPosition] = dataArray;
                }
            }

            if (_bufferedPackets.Count > 300)
            {
                BasicTeraData.LogError("Received: "+BytesReceived+"\r\n"+String.Join("\r\n",_bufferedPackets.Take(10).Select(x=>""+x.Key+": "+x.Value.Length)),false,true);
                BytesReceived = _bufferedPackets.Keys.First();
            }
            long firstBufferedPosition;
            while (_bufferedPackets.Any() && ((firstBufferedPosition = _bufferedPackets.Keys.First()) <= BytesReceived))
            {
                var dataArray = _bufferedPackets[firstBufferedPosition];
                _bufferedPackets.Remove(firstBufferedPosition);

                var alreadyReceivedBytes = BytesReceived - firstBufferedPosition;
                Debug.Assert(alreadyReceivedBytes >= 0);

                if (alreadyReceivedBytes >= dataArray.Length) continue;
                var count = dataArray.Length - alreadyReceivedBytes;
                OnDataReceived(new ArraySegment<byte>(dataArray, (int) alreadyReceivedBytes, (int) count));
                BytesReceived += count;
            }
        }

        public override string ToString()
        {
            return $"{Source} -> {Destination}";
        }
    }
}