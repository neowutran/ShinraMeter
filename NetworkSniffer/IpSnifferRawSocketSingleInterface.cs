// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PacketDotNet;
using PacketDotNet.Utils;

namespace NetworkSniffer
{
    public class IpSnifferRawSocketSingleInterface : IpSniffer
    {
        private readonly IPAddress _localIp;

        private bool _isInit;
        private Socket _socket;

        public IpSnifferRawSocketSingleInterface(IPAddress localIp)
        {
            _localIp = localIp;
        }

        private void Init()
        {
            Debug.Assert(_socket == null);
            if (_isInit) { return; }
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);

                if (_localIp != null) { _socket.Bind(new IPEndPoint(_localIp, 0)); }
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
                var receiveAllIp = BitConverter.GetBytes(3);
                _socket.IOControl(IOControlCode.ReceiveAll, receiveAllIp, null);
                _socket.ReceiveBufferSize = 1 << 24;
            }
            catch { return; }
            Task.Run(() => ReadAsync(_socket));
            _isInit = true;
        }

        private async Task ReadAsync(Socket s)
        {
            // Reusable SocketAsyncEventArgs and awaitable wrapper 
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[0x100000], 0, 0x100000);
            var awaitable = new SocketAwaitable(args);
            while (_isInit)
            {
                await s.ReceiveAsync(awaitable);
                var bytesRead = args.BytesTransferred;
                if (bytesRead <= 0) { throw new Exception("Raw socket is disconnected"); }
                IPv4Packet ipPacket;
                try { ipPacket = new IPv4Packet(new ByteArraySegment(args.Buffer, 0, bytesRead)); }
                catch (InvalidOperationException) { continue; }

                if (ipPacket.Version != IpVersion.IPv4 || ipPacket.Protocol != IPProtocolType.TCP) { continue; }
                OnPacketReceived(ipPacket);
            }
            _socket.Close();
            _socket = null;
        }

        private void Finish()
        {
            Debug.Assert(_socket != null);
            _isInit = false;
        }

        protected override void SetEnabled(bool value)
        {
            if (value) { Init(); }
            else { Finish(); }
        }


        public override string ToString()
        {
            return $"{base.ToString()} {_localIp}";
        }
    }

    public sealed class SocketAwaitable : INotifyCompletion
    {
        private static readonly Action SENTINEL = () => { };
        internal Action m_continuation;
        internal SocketAsyncEventArgs m_eventArgs;

        internal bool m_wasCompleted;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            m_eventArgs = eventArgs ?? throw new ArgumentNullException("eventArgs");
            eventArgs.Completed += delegate
            {
                var prev = m_continuation ?? Interlocked.CompareExchange(ref m_continuation, SENTINEL, null);
                prev?.Invoke();
            };
        }

        public bool IsCompleted => m_wasCompleted;

        public void OnCompleted(Action continuation)
        {
            if (m_continuation == SENTINEL || Interlocked.CompareExchange(ref m_continuation, continuation, null) == SENTINEL) { Task.Run(continuation); }
        }

        internal void Reset()
        {
            m_wasCompleted = false;
            m_continuation = null;
        }

        public SocketAwaitable GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
            if (m_eventArgs.SocketError != SocketError.Success) { throw new SocketException((int) m_eventArgs.SocketError); }
        }
    }

    public static class SocketExtensions
    {
        public static SocketAwaitable ReceiveAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ReceiveAsync(awaitable.m_eventArgs)) { awaitable.m_wasCompleted = true; }
            return awaitable;
        }
    }
}