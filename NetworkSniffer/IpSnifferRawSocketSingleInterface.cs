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
        private readonly byte[] _buffer;
        private readonly IPAddress _localIp;

        private bool _isInit;
        private Socket _socket;

        public IpSnifferRawSocketSingleInterface(IPAddress localIp)
        {
            _localIp = localIp;
            _buffer = new byte[1 << 17];
        }

        private void Init()
        {
            Debug.Assert(_socket == null);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP)
            {
                ReceiveBufferSize = 8192*1024
            };

            if (_localIp != null)
            {
                _socket.Bind(new IPEndPoint(_localIp, 0));
            }
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            var receiveAllOn = BitConverter.GetBytes(1);
            _socket.IOControl(IOControlCode.ReceiveAll, receiveAllOn, null);

            _socket.ReceiveBufferSize = 1 << 16;
            Read();
        }

        private void Finish()
        {
            if (!_isInit)
            {
                return;
            }
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
            {
                return;
            }
            var socket = (Socket) ar.AsyncState;
            var count = socket.EndReceive(ar);


            OnPacketReceived(new ArraySegment<byte>(_buffer, 0, count));
            Read();
        }

        protected override void SetEnabled(bool value)
        {
            if (value)
            {
                try
                {
                    Init();
                    _isInit = true;
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                Finish();
            }
        }


        public override string ToString()
        {
            return $"{base.ToString()} {_localIp}";
        }
    }
}