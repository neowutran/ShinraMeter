using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tera.Data;
using Tera.Protocol;
using Tera.Protocol.Game;

namespace Tera.Sniffer
{
    public partial class SnifferForm : Form
    {
        private TeraSniffer _teraSniffer;
        private LogWriter _logWriter;
        private readonly TeraData _teraData=new TeraData();
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
            _teraSniffer = new TeraSniffer(_teraData.ServerIps);
            _teraSniffer.MessageReceived += teraSniffer_MessageReceived;
            _teraSniffer.NewConnection += _teraSniffer_NewConnection;

            _teraSniffer.Enabled = true;
        }

        void _teraSniffer_NewConnection()
        {
            InvokeAction(() =>
                {
                    _logWriter = new LogWriter(string.Format("{0}.TeraLog", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture)));
                    ConnectionList.Items.Clear();
                    ConnectionList.Items.Add("New connection started...");
                });
        }

        void teraSniffer_MessageReceived(Protocol.Message message)
        {
            InvokeAction(() =>
            {
                Write(string.Format("{0} {1} {2}",
                    message.Direction==MessageDirection.ClientToServer?">":"<",
                    GetOpcodeName(message.OpCode), 
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
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _teraSniffer.Enabled = false;
        }
    }
}
