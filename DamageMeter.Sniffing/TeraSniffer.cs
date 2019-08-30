// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Data;
using NetworkSniffer;
using Tera;
using Tera.Game;
using Tera.Sniffing;
using System.Diagnostics;
using System.Threading;

namespace DamageMeter.Sniffing
{
    public class TeraSniffer : ITeraSniffer
    {
        private static TeraSniffer _instance;

        // Only take this lock in callbacks from tcp sniffing, not in code that can be called by the user.
        // Otherwise this could cause a deadlock if the user calls such a method from a callback that already holds a lock
        //private readonly object _eventLock = new object();
        private readonly IpSniffer _ipSniffer;

        private readonly ConcurrentDictionary<TcpConnection, byte> _isNew = new ConcurrentDictionary<TcpConnection, byte>();

        private readonly Dictionary<string, Server> _serversByIp;
        private TcpConnection _clientToServer;
        private ConnectionDecrypter _decrypter;
        private MessageSplitter _messageSplitter;
        private TcpConnection _serverToClient;
        public int ClientProxyOverhead;
        private bool _connected;

        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                _isNew.Keys.ToList().ForEach(x=>x.RemoveCallback());
                _isNew.Clear();
            }
        }
    
        public ConcurrentQueue<Message> Packets = new ConcurrentQueue<Message>();
        private Queue<Message> PacketsCopyStorage;

        public int ServerProxyOverhead;

        private TeraSniffer()
        {
            var servers = BasicTeraData.Instance.Servers;
            _serversByIp = servers.GetServersByIp();

            if (BasicTeraData.Instance.WindowData.Winpcap)
            {
                var netmasks = _serversByIp.Keys.Select(s => string.Join(".", s.Split('.').Take(3)) + ".0/24").Distinct().ToArray();

                var filter = string.Join(" or ", netmasks.Select(x => $"(net {x})"));
                filter = "tcp and (" + filter + ")";

                try //fallback to raw sockets if no winpcap available
                {
                    _ipSniffer = new IpSnifferWinPcap(filter);
                    ((IpSnifferWinPcap) _ipSniffer).Warning += OnWarning;
                }
                catch { _ipSniffer = new IpSnifferRawSocketMultipleInterfaces(); }
            }
            else { _ipSniffer = new IpSnifferRawSocketMultipleInterfaces(); }

            var tcpSniffer = new TcpSniffer(_ipSniffer);
            tcpSniffer.NewConnection += HandleNewConnection;
            tcpSniffer.EndConnection += HandleEndConnection;
        }


        public static TeraSniffer Instance => _instance ?? (_instance = new TeraSniffer());

        // IpSniffer has its own locking, so we need no lock here.
        public bool Enabled
        {
            get => _ipSniffer.Enabled;
            set => _ipSniffer.Enabled = value;
        }

        public event Action<Server> NewConnection;
        public event Action<Message> MessageReceived;
        public event Action EndConnection;
        public event Action<string> Warning;

        protected virtual void OnNewConnection(Server server)
        {
            PacketsCopyStorage = EnableMessageStorage ? new Queue<Message>() : null;
            var handler = NewConnection;
            handler?.Invoke(server);
        }

        protected virtual void OnEndConnection()
        {
            var handler = EndConnection;
            handler?.Invoke();
        }

        protected virtual void OnMessageReceived(Message message)
        {
            Packets.Enqueue(message);
            PacketsCopyStorage?.Enqueue(message);
        }

        private bool _enableMessageStorage;
        public bool EnableMessageStorage
        {
            get => _enableMessageStorage;
            set
            {
                _enableMessageStorage = value;
                if (!_enableMessageStorage) { PacketsCopyStorage = null; }
            }
        }

        public Queue<Message> GetPacketsLogsAndStop()
        {
            var tmp = PacketsCopyStorage ?? new Queue<Message>();
            EnableMessageStorage = false;
            // Wait for thread to sync, more perf than concurrentQueue
            Thread.Sleep(1);
            return tmp;
        }

        protected virtual void OnWarning(string obj)
        {
            var handler = Warning;
            handler?.Invoke(obj);
        }

        public void CleanupForcefully()
        {
            _clientToServer?.RemoveCallback();
            _serverToClient?.RemoveCallback();
            Connected = false;
            OnEndConnection();
            //_instance.Enabled = false;
            //_instance = null;
        }

        private void HandleEndConnection(TcpConnection connection)
        {
            if (connection == _clientToServer || connection == _serverToClient)
            {
                _clientToServer?.RemoveCallback();
                _serverToClient?.RemoveCallback();
                Connected = false;
                OnEndConnection();
            }
            else connection.RemoveCallback();
            connection.DataReceived -= HandleTcpDataReceived;
        }

        // called from the tcp sniffer, so it needs to lock
        private void HandleNewConnection(TcpConnection connection)
        {
            {
                if (Connected || !_serversByIp.ContainsKey(connection.Destination.Address.ToString()) &&
                    !_serversByIp.ContainsKey(connection.Source.Address.ToString())) { return; }
                _isNew.TryAdd(connection, 1);
                connection.DataReceived += HandleTcpDataReceived;
            }
        }

        // called from the tcp sniffer, so it needs to lock
        private void HandleTcpDataReceived(TcpConnection connection, byte[] data, int needToSkip)
        {
            {
                if (data.Length == 0)
                {
                    if (needToSkip == 0 || !(connection == _clientToServer || connection == _serverToClient)) { return; }
                    _decrypter?.Skip(connection == _clientToServer ? MessageDirection.ClientToServer : MessageDirection.ServerToClient, needToSkip);
                    return;
                }
                if (!Connected && _isNew.ContainsKey(connection))
                {
                    if (_serversByIp.ContainsKey(connection.Source.Address.ToString()) && data.Take(4).SequenceEqual(new byte[] {1, 0, 0, 0}))
                    {
                        byte q;
                        _isNew.TryRemove(connection, out q);
                        var server = _serversByIp[connection.Source.Address.ToString()];
                        _serverToClient = connection;
                        _clientToServer = null;

                        ServerProxyOverhead = (int) connection.BytesReceived;
                        _decrypter = new ConnectionDecrypter(server.Region);
                        _decrypter.ClientToServerDecrypted += HandleClientToServerDecrypted;
                        _decrypter.ServerToClientDecrypted += HandleServerToClientDecrypted;

                        _messageSplitter = new MessageSplitter();
                        _messageSplitter.MessageReceived += HandleMessageReceived;
                        _messageSplitter.Resync += OnResync;
                    }
                    if (_serverToClient != null && _clientToServer == null && _serverToClient.Destination.Equals(connection.Source) &&
                        _serverToClient.Source.Equals(connection.Destination))
                    {
                        ClientProxyOverhead = (int) connection.BytesReceived;
                        byte q;
                        _isNew.TryRemove(connection, out q);
                        _clientToServer = connection;
                        var server = _serversByIp[connection.Destination.Address.ToString()];
                        _isNew.Clear();
                        OnNewConnection(server);
                    }
                    if (connection.BytesReceived > 0x10000) //if received more bytes but still not recognized - not interesting.
                    {
                        byte q;
                        _isNew.TryRemove(connection, out q);
                        connection.DataReceived -= HandleTcpDataReceived;
                        connection.RemoveCallback();
                    }
                }

                if (!(connection == _clientToServer || connection == _serverToClient)) { return; }
                if (_decrypter == null) { return; }
                if (connection == _clientToServer) { _decrypter.ClientToServer(data, needToSkip); }
                else { _decrypter.ServerToClient(data, needToSkip); }
            }
        }

        private void OnResync(MessageDirection direction, int skipped, int size)
        {
            BasicTeraData.LogError("Resync occured " + direction + ", skipped:" + skipped + ", block size:" + size, false, true);
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