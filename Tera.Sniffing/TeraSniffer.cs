// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NetworkSniffer;
using PacketDotNet.Utils;
using Tera.Game;

namespace Tera.Sniffing
{
    public class TeraSniffer : ITeraSniffer
    {
        // Only take this lock in callbacks from tcp sniffing, not in code that can be called by the user.
        // Otherwise this could cause a deadlock if the user calls such a method from a callback that already holds a lock
        private readonly object _eventLock = new object();

        private readonly Dictionary<string, Server> _serversByIp;
        private readonly HashSet<TcpConnection> _isNew = new HashSet<TcpConnection>();
        private TcpConnection _clientToServer;
        private TcpConnection _serverToClient;
        private ConnectionDecrypter _decrypter;
        private MessageSplitter _messageSplitter;
        private readonly IpSniffer _ipSniffer;

        public TeraSniffer(IEnumerable<Server> servers)
        {
            _serversByIp = servers.ToDictionary(x => x.Ip);
            var netmasks =
                _serversByIp.Keys.Select(s => string.Join(".", s.Split('.').Take(3)) + ".0/24").Distinct().ToArray();
            string filter = string.Join(" or ", netmasks.Select(x => string.Format("(net {0})", x)));
            filter = "tcp and (" + filter + ")";

            _ipSniffer = new IpSniffer(filter);
            _ipSniffer.Warning += OnWarning;
            var tcpSniffer = new TcpSniffer(_ipSniffer);
            tcpSniffer.NewConnection += HandleNewConnection;
        }

        // IpSniffer has its own locking, so we need no lock here.
        public bool Enabled
        {
            get
            {
                return _ipSniffer.Enabled;
            }
            set
            {
                _ipSniffer.Enabled = value;
            }
        }

        public IEnumerable<string> SnifferStatus()
        {
            return _ipSniffer.Status();
        }

        public event Action<Message> MessageReceived;
        public event Action<Server> NewConnection;
        public event Action<string> Warning;

        protected virtual void OnNewConnection(Server server)
        {
            var handler = NewConnection;
            if (handler != null) handler(server);
        }

        protected virtual void OnMessageReceived(Message message)
        {
            var handler = MessageReceived;
            if (handler != null) handler(message);
        }

        protected virtual void OnWarning(string obj)
        {
            Action<string> handler = Warning;
            if (handler != null) handler(obj);
        }


        // called from the tcp sniffer, so it needs to lock
        void HandleNewConnection(TcpConnection connection)
        {
            lock (_eventLock)
            {
                if (!_serversByIp.ContainsKey(connection.Destination.Address.ToString()) &&
                    !_serversByIp.ContainsKey(connection.Source.Address.ToString()))
                    return;
                _isNew.Add(connection);
                connection.DataReceived += HandleTcpDataReceived;
            }
        }

        // called from the tcp sniffer, so it needs to lock
        private void HandleTcpDataReceived(TcpConnection connection, ByteArraySegment data)
        {
            lock (_eventLock)
            {
                if (data.Length == 0)
                    return;
                if (_isNew.Contains(connection))
                {
                    _isNew.Remove(connection);
                    if (_serversByIp.ContainsKey(connection.Source.Address.ToString()) &&
                        data.Bytes.Skip(data.Offset).Take(4).SequenceEqual(new byte[] { 1, 0, 0, 0 }))
                    {
                        var server = _serversByIp[connection.Source.Address.ToString()];
                        _serverToClient = connection;
                        _clientToServer = null;

                        _decrypter = new ConnectionDecrypter();
                        _decrypter.ClientToServerDecrypted += HandleClientToServerDecrypted;
                        _decrypter.ServerToClientDecrypted += HandleServerToClientDecrypted;

                        _messageSplitter = new MessageSplitter();
                        _messageSplitter.MessageReceived += HandleMessageReceived;
                        OnNewConnection(server);
                    }
                    if (_serverToClient != null && _clientToServer == null &&
                        (_serverToClient.Destination.Equals(connection.Source) &&
                         _serverToClient.Source.Equals(connection.Destination)))
                    {
                        _clientToServer = connection;
                    }
                }

                if (!(connection == _clientToServer || connection == _serverToClient))
                    return;
                if (_decrypter == null)
                    return;
                var dataArray = data.Bytes.Skip(data.Offset).Take(data.Length).ToArray();
                if (connection == _clientToServer)
                    _decrypter.ClientToServer(dataArray);
                else
                    _decrypter.ServerToClient(dataArray);
            }
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        private void HandleMessageReceived(Message message)
        {
            OnMessageReceived(message);
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        void HandleServerToClientDecrypted(byte[] data)
        {
            _messageSplitter.ServerToClient(DateTime.UtcNow, data);
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        void HandleClientToServerDecrypted(byte[] data)
        {
            _messageSplitter.ClientToServer(DateTime.UtcNow, data);
        }
    }
}
