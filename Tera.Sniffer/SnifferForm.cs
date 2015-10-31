// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tera.Data;
using Tera.Game;
using Tera.PacketLog;
using Tera.Sniffing;

namespace Tera.Sniffer
{
    public partial class SnifferForm : Form
    {
        private ITeraSniffer _teraSniffer;
        private PacketLogWriter _logWriter;
        private readonly BasicTeraData _basicTeraData = new BasicTeraData();
        private TeraData _teraData;
        private long _clientMessages = 0;
        private long _serverMessages = 0;

        public SnifferForm()
        {
            InitializeComponent();
        }

        private string GetOpcodeName(ushort opCode)
        {
            return _teraData.OpCodeNamer.GetName(opCode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _teraSniffer = new TeraSniffer(_basicTeraData.Servers);
            _teraSniffer.MessageReceived += teraSniffer_MessageReceived;
            _teraSniffer.NewConnection += _teraSniffer_NewConnection;

            _teraSniffer.Enabled = true;
        }

        void _teraSniffer_NewConnection(Server server)
        {
            InvokeAction(() =>
                {
                    var header = new LogHeader { Region = server.Region };
                    _logWriter = new PacketLogWriter(string.Format("{0}.TeraLog", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture)), header);
                    ConnectionList.Items.Clear();
                    ConnectionList.Items.Add(string.Format("New connection to {0}started...", server.Name));
                    _teraData = _basicTeraData.DataForRegion(server.Region);
                });
        }

        void teraSniffer_MessageReceived(Message message)
        {
            InvokeAction(() =>
            {
                Write(string.Format("{0} {1}({2}) {3}",
                    message.Direction == MessageDirection.ClientToServer ? ">" : "<",
                    GetOpcodeName(message.OpCode),
                    message.OpCode,
                    message.Data.Count));
                _logWriter.Append(message);

                if (message.Direction == MessageDirection.ClientToServer)
                    _clientMessages++;
                else
                    _serverMessages++;

                MessageCount.Text = string.Format("Client {0}    Server {1}", _clientMessages, _serverMessages);
            });
        }

        private void InvokeAction(Action action)
        {
            if (IsDisposed)
                return;
            if (!InvokeRequired)
                throw new InvalidOperationException("Expected InvokeRequired");
            Invoke(action);
        }

        private void Write(string s)
        {
            ConnectionList.Items.Add(s);
            ConnectionList.TopIndex = ConnectionList.Items.Count - 1;
            File.AppendAllLines("OpCode Log.txt", new[] { s });
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _teraSniffer.Enabled = false;
        }
    }
}
