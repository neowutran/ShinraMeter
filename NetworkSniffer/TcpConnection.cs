using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Data;
using Lang;

namespace NetworkSniffer
{
    public class TcpConnection
    {
        private readonly SortedDictionary<long, byte[]> _bufferedPackets = new SortedDictionary<long, byte[]>();
        public readonly IPEndPoint Destination;
        public readonly IPEndPoint Source;
        public Action RemoveCallback;
        public string SnifferType;

        internal TcpConnection(ConnectionId connectionId, uint sequenceNumber, Action<TcpConnection>removeCallback, string snifferType)
        {
            Source = connectionId.Source.ToIpEndpoint();
            Destination = connectionId.Destination.ToIpEndpoint();
            InitialSequenceNumber = sequenceNumber;
            RemoveCallback = () => removeCallback(this);
            SnifferType = snifferType;
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

            if (_bufferedPackets.Count > 500)
            {
                string debug = (BasicTeraData.Instance.WindowData.LowPriority ? "Low priority " : "Normal priority ") + SnifferType +
                               " Received: " + BytesReceived + "\r\n" +_bufferedPackets.Take(1).Select(x => "" + x.Key + ": " + x.Value.Length) + "\r\nQueue length:" + _bufferedPackets.Count;
                BasicTeraData.LogError(debug);
                MessageBox.Show(LP.MissedPacket);
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