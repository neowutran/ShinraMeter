using System;
using System.IO;
using System.Linq;
using Crypt;

namespace Tera.Protocol.Sniffing
{
    public class ConnectionDecrypter
    {
        private MemoryStream _client = new MemoryStream();
        private MemoryStream _server = new MemoryStream();
        private Session _session;
        private bool Initialized { get { return _session != null; } }

        public event Action<byte[]> ClientToServerDecrypted;
        public event Action<byte[]> ServerToClientDecrypted;

        protected void OnClientToServerDecrypted(byte[] data)
        {
            var action = ClientToServerDecrypted;
            if (action != null)
                action(data);
        }

        protected void OnServerToClientDecrypted(byte[] data)
        {
            var action = ServerToClientDecrypted;
            if (action != null)
                action(data);
        }

        private static Session CreateSession(byte[] clientKey1, byte[] clientKey2, byte[] serverKey1, byte[] serverKey2)
        {
            var session = new Session();
            session.ClientKey1 = clientKey1;
            session.ClientKey2 = clientKey2;
            session.ServerKey1 = serverKey1;
            session.ServerKey2 = serverKey2;
            session.Init();
            return session;
        }

        private void TryInitialize()
        {
            if (Initialized)
                throw new InvalidOperationException("Already initalized");
            if (_client.Length < 256 || _server.Length < 256 + 4)
                return;

            _server.Position = 0;
            _client.Position = 0;

            var magicBytes = _server.ReadBytes(4);
            if (!magicBytes.SequenceEqual(new byte[] { 1, 0, 0, 0 }))
                throw new FormatException("Not a Tera connection");

            var clientKey1 = _client.ReadBytes(128);
            var clientKey2 = _client.ReadBytes(128);
            var serverKey1 = _server.ReadBytes(128);
            var serverKey2 = _server.ReadBytes(128);
            _session = CreateSession(clientKey1, clientKey2, serverKey1, serverKey2);

            ClientToServer(_client.ReadBytes((int)(_client.Length - _client.Position)));
            ServerToClient(_server.ReadBytes((int)(_server.Length - _server.Position)));
            _client = null;
            _server = null;
        }

        public void ClientToServer(byte[] data)
        {
            if (Initialized)
            {
                var result = data.ToArray();
                _session.Decrypt(result);
                OnClientToServerDecrypted(result);
            }
            else
            {
                _client.Write(data, 0, data.Length);
                TryInitialize();
            }
        }

        public void ServerToClient(byte[] data)
        {
            if (Initialized)
            {
                var result = data.ToArray();
                _session.Encrypt(result);
                OnServerToClientDecrypted(result);
            }
            else
            {
                _server.Write(data, 0, data.Length);
                TryInitialize();
            }
        }
    }
}
