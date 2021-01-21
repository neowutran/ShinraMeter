using System;
using System.Collections.Generic;
using System.Threading;
using DamageMeter.Sniffing;
using Tera.Game;
using Tera.Game.Messages;
using OpcodeId = System.UInt16;
using Tera.PacketLog;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using Tera.Sniffing;

namespace PacketViewer
{
    public class NetworkController
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void GuildIconEvent(Bitmap icon);
        public delegate void UpdateUiHandler(Tuple<List<ParsedMessage>, Dictionary<OpcodeId, OpcodeEnum>, int> message);
        public event UpdateUiHandler TickUpdated;
        public event Action ResetUi;
        private static NetworkController _instance;

        private bool _keepAlive = true;
        internal MessageFactory MessageFactory = new MessageFactory();
        internal bool NeedInit = true;
        public Server Server;
        internal UserLogoTracker UserLogoTracker = new UserLogoTracker();
        public  ITeraSniffer Sniffer;

        private NetworkController()
        {
            Sniffer = SnifferFactory.Create();
            Sniffer.NewConnection += HandleNewConnection;
            Sniffer.EndConnection += HandleEndConnection;
            var packetAnalysis = new Thread(PacketAnalysisLoop);
            packetAnalysis.Start();
        }

        public PlayerTracker PlayerTracker { get; internal set; }

        public NpcEntity Encounter { get; private set; }
        public NpcEntity NewEncounter { get; set; }

        public bool TimedEncounter { get; set; }

        public string LoadFileName { get; set; }
        public bool NeedToSave { get; set; }
        public string LoadOpcodeCheck { get; set; }
        public string LoadOpcodeForViewing { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public EntityTracker EntityTracker { get; internal set; }
        public bool SendFullDetails { get; set; }

        public event GuildIconEvent GuildIconAction;

        public void Exit()
        {
            _keepAlive = false;
            Sniffer.Enabled = false;
            Thread.Sleep(500);
            Application.Exit();
        }

        internal void RaiseConnected(string message)
        {
            Connected?.Invoke(message);
        }
            
        public event ConnectedHandler Connected;

        protected virtual void HandleEndConnection()
        {
            NeedInit = true;
            MessageFactory = new MessageFactory();
            Connected?.Invoke("no server");
            OnGuildIconAction(null);
        }

        protected virtual void HandleNewConnection(Server server)
        {
            Server = server;
            NeedInit = true;
            MessageFactory = new MessageFactory();
            Connected?.Invoke(server.Name);
        }
        public Dictionary<OpcodeId, OpcodeEnum> UiUpdateKnownOpcode;
        public List<ParsedMessage> UiUpdateData;
        private void UpdateUi()
        {
            var currentLastPacket = OpcodeFinder.Instance.PacketCount;
            TickUpdated?.Invoke(new Tuple<List<ParsedMessage>, Dictionary<OpcodeId, OpcodeEnum>, int> (UiUpdateData, UiUpdateKnownOpcode, Sniffer.Packets.Count));
            UiUpdateData = new List<ParsedMessage>();
            UiUpdateKnownOpcode = new Dictionary<OpcodeId, OpcodeEnum>();
        }

        public uint Version { get; private set; }

        private void SaveLog()
        {
            if (!NeedToSave) { return; }
            NeedToSave = false;
            if(AnalysisType  == AnalysisTypeEnum.Unknown) { return; }
            if (AnalysisType == AnalysisTypeEnum.LogFile)
            {
                MessageBox.Show("Saving saved log is retarded");
                return;
            }
            var header = new LogHeader { Region =  Version.ToString()};
            PacketLogWriter writer = new PacketLogWriter(string.Format("{0}.TeraLog", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_"+Version, CultureInfo.InvariantCulture)), header);
            foreach(var message in OpcodeFinder.Instance.AllPackets)
            {
                writer.Append(message.Value);
            }
            writer.Dispose();
            MessageBox.Show("Saved");
        }
        public enum AnalysisTypeEnum
        {
            Unknown = 0,
            Network = 1,
            LogFile = 2
        }

        public bool StrictCheck = false;
        private AnalysisTypeEnum AnalysisType = 0;
        private void PacketAnalysisLoop()
        {
     
            while (_keepAlive)
            {
                LoadFile();
                SaveLog();
                if (LoadOpcodeCheck != null) {
                    if (OpcodeFinder.Instance.OpcodePartialMatch())
                    {
                        MessageBox.Show("Partial match: SUCCESS");
                    }
                    else
                    {
                        MessageBox.Show("Partial match: FAIL");
                    }
                }

                if(LoadOpcodeForViewing != null)
                {
                    OpcodeFinder.Instance.OpcodeLoadKnown();
                }

                // Update the UI at every packet if the backend it not overload & if we are recording the network
                if (AnalysisType == AnalysisTypeEnum.Network && Sniffer.Packets.Count < 2000)
                {
                    UpdateUi();
                }
                // If loading log file, wait until completion before display
                if (AnalysisType == AnalysisTypeEnum.LogFile && Sniffer.Packets.Count == 0)
                {
                    UpdateUi();
                }
                var successDequeue = Sniffer.Packets.TryDequeue(out var obj);
                if (!successDequeue)
                {
                    Thread.Sleep(1);
                    continue;
                }
                
                // Network
                if (AnalysisType == AnalysisTypeEnum.Unknown) { AnalysisType = AnalysisTypeEnum.Network; }

                if(AnalysisType == AnalysisTypeEnum.LogFile && Sniffer.Connected)
                {
                    throw new Exception("Not allowed to record network while reading log file");
                }

                var message = MessageFactory.Create(obj);
                //message.PrintRaw();

                if(message is C_CHECK_VERSION)
                {
                    Version = (message as C_CHECK_VERSION).Versions[0];
                    // TODO reset backend & UI

                }
                OpcodeFinder.Instance.Find(message);
            }
        }

     

        internal virtual void OnGuildIconAction(Bitmap icon)
        {
            GuildIconAction?.Invoke(icon);
        }

        void LoadFile()
        {
            if (LoadFileName == null) { return; }
            if(AnalysisType == AnalysisTypeEnum.Network) { throw new Exception("Not allowed to load a log file while recording in the network"); }
            AnalysisType = AnalysisTypeEnum.LogFile;
            OpcodeFinder.Instance.Reset();
            ResetUi?.Invoke();
            LogReader.LoadLogFromFile(LoadFileName).ForEach(x => Sniffer.Packets.Enqueue(x));
            LoadFileName = null;

        }
    }
}