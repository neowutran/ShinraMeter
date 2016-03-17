// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Data;
using NetworkSniffer;
using Tera;
using Tera.Game;
using Tera.Sniffing;

namespace DamageMeter.Sniffing
{
    public class TeraSniffer : ITeraSniffer
    {
        private static TeraSniffer _instance;
        // Only take this lock in callbacks from tcp sniffing, not in code that can be called by the user.
        // Otherwise this could cause a deadlock if the user calls such a method from a callback that already holds a lock
        private readonly object _eventLock = new object();
        private readonly IpSniffer _ipSniffer;
        private readonly HashSet<TcpConnection> _isNew = new HashSet<TcpConnection>();

        private readonly Dictionary<string, Server> _serversByIp;
        private TcpConnection _clientToServer;
        private ConnectionDecrypter _decrypter;
        private MessageSplitter _messageSplitter;
        private TcpConnection _serverToClient;

        public ConcurrentQueue<Message> Packets = new ConcurrentQueue<Message>();

        private TeraSniffer()
        {
            var servers = BasicTeraData.Instance.Servers;
            _serversByIp = servers.ToDictionary(x => x.Ip);


            if (BasicTeraData.Instance.WindowData.Winpcap)
            {
                var netmasks =
                    _serversByIp.Keys.Select(s => string.Join(".", s.Split('.').Take(3)) + ".0/24").Distinct().ToArray();

                var filter = string.Join(" or ", netmasks.Select(x => $"(net {x})"));
                filter = "tcp and (" + filter + ")";

                _ipSniffer = new IpSnifferWinPcap(filter);
            }
            else
            {
                _ipSniffer = new IpSnifferRawSocketMultipleInterfaces();
            }

            var tcpSniffer = new TcpSniffer(_ipSniffer);
            tcpSniffer.NewConnection += HandleNewConnection;
        }


        public static TeraSniffer Instance => _instance ?? (_instance = new TeraSniffer());

        // IpSniffer has its own locking, so we need no lock here.
        public bool Enabled
        {
            get { return _ipSniffer.Enabled; }
            set { _ipSniffer.Enabled = value; }
        }

        public event Action<Server> NewConnection;
        public event Action<Message> MessageReceived;
        public event Action<string> Warning;

        protected virtual void OnNewConnection(Server server)
        {
            var handler = NewConnection;
            handler?.Invoke(server);
        }


        protected virtual void OnMessageReceived(Message message)
        {
            Packets.Enqueue(message);
        }

        protected virtual void OnWarning(string obj)
        {
            var handler = Warning;
            handler?.Invoke(obj);
        }


        // called from the tcp sniffer, so it needs to lock
        private void HandleNewConnection(TcpConnection connection)
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
        private void HandleTcpDataReceived(TcpConnection connection, ArraySegment<byte> data)
        {
            lock (_eventLock)
            {
                if (data.Count == 0)
                    return;
                if (_isNew.Contains(connection))
                {
                    if (_serversByIp.ContainsKey(connection.Source.Address.ToString()) &&
                        data.Array.Skip(data.Offset).Take(4).SequenceEqual(new byte[] {1, 0, 0, 0}))
                    {
                        _isNew.Remove(connection);
                        var server = _serversByIp[connection.Source.Address.ToString()];
                        _serverToClient = connection;
                        _clientToServer = null;

                        _decrypter = new ConnectionDecrypter(server.Region);
                        _decrypter.ClientToServerDecrypted += HandleClientToServerDecrypted;
                        _decrypter.ServerToClientDecrypted += HandleServerToClientDecrypted;

                        _messageSplitter = new MessageSplitter();
                        _messageSplitter.MessageReceived += HandleMessageReceived;
                        OnNewConnection(server);
                    }
                    if (_serverToClient != null && _clientToServer == null &&
                        _serverToClient.Destination.Equals(connection.Source) &&
                        _serverToClient.Source.Equals(connection.Destination))
                    {
                        _isNew.Remove(connection);
                        _clientToServer = connection;
                    }
                }

                if (!(connection == _clientToServer || connection == _serverToClient))
                    return;
                if (_decrypter == null)
                    return;
                var dataArray = data.Array.Skip(data.Offset).Take(data.Count).ToArray();
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
        private void HandleServerToClientDecrypted(byte[] data)
        {
            _messageSplitter.ServerToClient(DateTime.UtcNow, data);
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        private void HandleClientToServerDecrypted(byte[] data)
        {
            _messageSplitter.ClientToServer(DateTime.UtcNow, data);
        }
    }
}