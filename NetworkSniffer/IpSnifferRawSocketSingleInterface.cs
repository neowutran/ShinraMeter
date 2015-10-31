// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetworkSniffer
{
    // Doesn't work since Microsoft crippled raw sockets on the Desktop variants of Windows.
    // In particular it doesn't receive incoming TCP packets
    // Might work on Server variants of Windows, but I didn't test that
    public class IpSnifferRawSocketSingleInterface : IpSniffer
    {
        private Socket _socket;
        private readonly IPAddress _localIp;
        private readonly byte[] _buffer;

        public IpSnifferRawSocketSingleInterface(IPAddress localIp)
        {
            _localIp = localIp;
            _buffer = new byte[1024 * 64];
        }

        private void Init()
        {
            Debug.Assert(_socket == null);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            if (_localIp != null)
                _socket.Bind(new IPEndPoint(_localIp, 0));
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            var receiveAllOn = BitConverter.GetBytes(1);
            _socket.IOControl(IOControlCode.ReceiveAll, receiveAllOn, null);
            Read();
        }

        private void Finish()
        {
            Debug.Assert(_socket != null);
            _socket.Close();
            _socket = null;
        }

        private void Read()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, Receive, _socket);
        }

        private void Receive(IAsyncResult ar)
        {
            if (!Enabled)
                return;
            var socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            OnPacketReceived(new ArraySegment<byte>(_buffer, 0, count));
            Read();
        }

        protected override void SetEnabled(bool value)
        {
            if (value)
                Init();
            else
                Finish();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), _localIp);
        }
    }
}