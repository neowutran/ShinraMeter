using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DamageMeter.Database.Structures;
using DamageMeter.Processing;
using DamageMeter.Sniffing;
using DamageMeter.TeraDpsApi;
using Data;
using Data.Actions.Notify;
using Lang;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;
using Message = Tera.Message;

namespace DamageMeter
{
    public class NetworkController
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void GuildIconEvent(Bitmap icon);

        public delegate void SetClickThrouEvent();

        public delegate void UnsetClickThrouEvent();

        public delegate void PauseEvent(bool paused);

        public delegate void UpdateUiHandler(UiUpdateMessage message);

        private static NetworkController _instance;
        private static readonly object _pasteLock = new object();
        internal AbnormalityStorage AbnormalityStorage;

        internal readonly List<Player> MeterPlayers = new List<Player>();

        private bool _clickThrou;
        private bool _forceUiUpdate;
        private bool _keepAlive = true;
        private long _lastTick;
        internal AbnormalityTracker AbnormalityTracker;
        public ConcurrentDictionary<string, NpcEntity> BossLink = new ConcurrentDictionary<string, NpcEntity>();
        public GlyphBuild Glyphs = new GlyphBuild();
        internal MessageFactory MessageFactory = new MessageFactory();
        internal bool NeedInit = true;
        public CopyKey NeedToCopy;

        public DataExporter.Dest NeedToExport = DataExporter.Dest.None;
        public bool NeedToReset;
        public bool NeedToResetCurrent;

        internal PacketProcessingFactory PacketProcessing = new PacketProcessingFactory();
        public Server Server;
        internal UserLogoTracker UserLogoTracker = new UserLogoTracker();

        private NetworkController()
        {
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            TeraSniffer.Instance.EndConnection += HandleEndConnection;
            AbnormalityStorage = new AbnormalityStorage();
            var packetAnalysis = new Thread(PacketAnalysisLoop);
            packetAnalysis.Start();
        }

        public List<NotifyFlashMessage> FlashMessage = new List<NotifyFlashMessage>();
        public PlayerTracker PlayerTracker { get; internal set; }

        public TeraData TeraData { get; internal set; }
        public NpcEntity Encounter { get; private set; }
        public NpcEntity NewEncounter { get; set; }

        public bool TimedEncounter { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public EntityTracker EntityTracker { get; internal set; }
        public bool SendFullDetails { get; set; }

        public event SetClickThrouEvent SetClickThrouAction;
        public event GuildIconEvent GuildIconAction;
        public event UnsetClickThrouEvent UnsetClickThrouAction;
        public event PauseEvent PauseAction;

        public void Exit()
        {
            if (_keepAlive)
            {
                BasicTeraData.Instance.WindowData.Save();
                BasicTeraData.Instance.HotkeysData.Save();
            }
            TeraSniffer.Instance.Enabled = false;
            _keepAlive = false;
            Thread.Sleep(500);
            HudManager.Instance.CurrentBosses.DisposeAll();
            Application.Exit();
        }

        internal void RaiseConnected(string message)
        {
            Connected?.Invoke(message);
        }

        internal void RaisePause(bool pause)
        {
            PauseAction?.Invoke(pause);
        }

        public event ConnectedHandler Connected;
        public event UpdateUiHandler TickUpdated;

        protected virtual void HandleEndConnection()
        {
            NeedInit = true;
            MessageFactory = new MessageFactory();
            NotifyProcessor.Instance.S_LOAD_TOPO(null);
            Connected?.Invoke(LP.SystemTray_No_server);
            OnGuildIconAction(null);
        }

        protected virtual void HandleNewConnection(Server server)
        {
            Server = server;
            NeedInit = true;
            MessageFactory = new MessageFactory();
            Connected?.Invoke(server.Name);
        }

        public void Reset()
        {
            DamageTracker.Instance.Reset();
            AbnormalityStorage.ClearEnded();
            _forceUiUpdate = true;
        }

        public void ResetCurrent()
        {
            DamageTracker.Instance.DeleteEntity(Encounter);
            _forceUiUpdate = true;
        }

        private void UpdateUi(int packetsWaiting = 0)
        {
            if (!NeedInit)
            {
                if (BasicTeraData.Instance.WindowData.EnableChat != MessageFactory.ChatEnabled)
                {
                    MessageFactory.ChatEnabled = BasicTeraData.Instance.WindowData.EnableChat;
                    if (BasicTeraData.Instance.WindowData.EnableChat)
                    {
                        AbnormalityTracker.AbnormalityAdded += NotifyProcessor.Instance.AbnormalityNotifierAdded;
                        AbnormalityTracker.AbnormalityRemoved += NotifyProcessor.Instance.AbnormalityNotifierRemoved;
                    }
                    else
                    {
                        AbnormalityTracker.AbnormalityAdded -= NotifyProcessor.Instance.AbnormalityNotifierAdded;
                        AbnormalityTracker.AbnormalityRemoved -= NotifyProcessor.Instance.AbnormalityNotifierRemoved;
                        HudManager.Instance.CurrentBosses.DisposeAll();
                    }
                    if (!PacketProcessing.Paused) PacketProcessing.Update();
                }
                NotifyProcessor.Instance.AbnormalityNotifierMissing();
            }
            _lastTick = DateTime.UtcNow.Ticks;
            var handler = TickUpdated;
            var currentBoss = Encounter;
            var timedEncounter = TimedEncounter;

            var entities = Database.Database.Instance.AllEntity();
            var filteredEntities = entities.Select(entityid => EntityTracker.GetOrNull(entityid)).OfType<NpcEntity>().Where(npc => npc.Info.Boss).ToList();
            if (packetsWaiting > 2500 && filteredEntities.Count > 1)
            {
                Database.Database.Instance.DeleteAllWhenTimeBelow(Encounter);
                entities = Database.Database.Instance.AllEntity();
                filteredEntities = entities.Select(entityid => EntityTracker.GetOrNull(entityid)).OfType<NpcEntity>().Where(npc => npc.Info.Boss).ToList();
            }

            var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter);
            if (currentBoss != null)
            {
                NotifyProcessor.Instance._lastBosses.TryGetValue(currentBoss.Id, out long entityHP);
                var entityDamaged = currentBoss.Info.HP - entityHP;
                entityInfo.TimeLeft = entityDamaged == 0 ? 0 : entityInfo.Interval * entityHP / entityDamaged;
            }
            Skills skills = null;
            if (SendFullDetails)
            {
                skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
                SendFullDetails = false;
            }
            var playersInfo = timedEncounter
                ? Database.Database.Instance.PlayerDamageInformation(entityInfo.BeginTime, entityInfo.EndTime)
                : Database.Database.Instance.PlayerDamageInformation(currentBoss);
            if (BasicTeraData.Instance.WindowData.MeterUserOnTop)
            {
                playersInfo = playersInfo.OrderBy(x => MeterPlayers.Contains(x.Source) ? 0 : 1).ThenByDescending(x => x.Amount).ToList();
            }

            var heals = Database.Database.Instance.PlayerHealInformation(entityInfo.BeginTime, entityInfo.EndTime);

            var flash = FlashMessage;
            FlashMessage = new List<NotifyFlashMessage>();

            var statsSummary = new StatsSummary(playersInfo, heals, entityInfo);
            var teradpsHistory = BossLink;
            var chatbox = Chat.Instance.Get();
            var abnormals = AbnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
            var uiMessage = new UiUpdateMessage(statsSummary, skills, filteredEntities, timedEncounter, abnormals, teradpsHistory, chatbox, flash);
            handler?.Invoke(uiMessage);
        }
        
        public List<DpsServer> Initialize()
        {
            var listForUi = new List<DpsServer>();
            DataExporter.DpsServers = new List<DpsServer> { DpsServer.NeowutranAnonymousServer };
            foreach(var dpsServer in BasicTeraData.Instance.WindowData.DpsServers)
            {
                var server = new DpsServer(dpsServer, false);
                listForUi.Add(server);
                DataExporter.DpsServers.Add(server);
            }
            return listForUi;
        }

        public void SwitchClickThrou()
        {
            if (_clickThrou)
            {
                UnsetClickThrou();
                _clickThrou = false;
                return;
            }
            SetClickThrou();
            _clickThrou = true;
        }

        public void SwitchClickThrou(bool value)
        {
            if (value)
            {
                SetClickThrou();
                _clickThrou = true;
                return;
            }
            UnsetClickThrou();
            _clickThrou = false;
        }

        protected virtual void SetClickThrou()
        {
            SetClickThrouAction?.Invoke();
        }

        protected virtual void UnsetClickThrou()
        {
            UnsetClickThrouAction?.Invoke();
        }

        public static void CopyThread(StatsSummary stats, Skills skills, AbnormalityStorage abnormals, bool timedEncounter, CopyKey copy)
        {
            if (BasicTeraData.Instance.HotDotDatabase == null)
            {
                return; //no database loaded yet => no need to do anything
            }
            lock (_pasteLock)
            {
                var text = CopyPaste.Copy(stats, skills, abnormals, timedEncounter, copy);
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        Clipboard.SetText(text.Item2);
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        //Ignore
                    }
                }
                CopyPaste.Paste(text.Item1);
            }
        }

        private void PacketAnalysisLoop()
        {
            try { Database.Database.Instance.DeleteAll(); }
            catch (Exception ex)
            {
                BasicTeraData.LogError(
                    ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException + "\r\n" + ex.TargetSite,
                    true);
                MessageBox.Show(LP.MainWindow_Fatal_error);
                Exit();
            }

            while (_keepAlive)
            {
                if (NeedToCopy != null)
                {
                    var currentBoss = Encounter;
                    var timedEncounter = TimedEncounter;

                    var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter);
                    var skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
                    var playersInfo = timedEncounter
                        ? Database.Database.Instance.PlayerDamageInformation(entityInfo.BeginTime, entityInfo.EndTime)
                        : Database.Database.Instance.PlayerDamageInformation(currentBoss);
                    var heals = Database.Database.Instance.PlayerHealInformation(entityInfo.BeginTime, entityInfo.EndTime);
                    var statsSummary = new StatsSummary(playersInfo, heals, entityInfo);

                    var tmpcopy = NeedToCopy;
                    var abnormals = AbnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
                    var pasteThread = new Thread(() => CopyThread(statsSummary, skills, abnormals, timedEncounter, tmpcopy)) {Priority = ThreadPriority.Highest};
                    pasteThread.SetApartmentState(ApartmentState.STA);
                    pasteThread.Start();

                    NeedToCopy = null;
                }

                if (NeedToReset)
                {
                    Reset();
                    NeedToReset = false;
                }

                if (NeedToResetCurrent)
                {
                    ResetCurrent();
                    NeedToResetCurrent = false;
                }

                if (!NeedToExport.HasFlag(DataExporter.Dest.None))
                {
                    DataExporter.ManualExport(Encounter, AbnormalityStorage, NeedToExport);
                    NeedToExport = DataExporter.Dest.None;
                }

                Encounter = NewEncounter;

                var packetsWaiting = TeraSniffer.Instance.Packets.Count;
                if (packetsWaiting > 5000)
                {
                    PacketProcessing.Pause();
                    Database.Database.Instance.DeleteAll();
                    AbnormalityStorage = new AbnormalityStorage();
                    AbnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker, BasicTeraData.Instance.HotDotDatabase, AbnormalityStorage, DamageTracker.Instance.Update);
                    HudManager.Instance.CurrentBosses.DisposeAll();
                    TeraSniffer.Instance.Packets=new ConcurrentQueue<Message>();
                    NotifyProcessor.Instance.S_LOAD_TOPO(null);
                    RaisePause(true);
                }

                if (_forceUiUpdate)
                {
                    UpdateUi(packetsWaiting);
                    _forceUiUpdate = false;
                }

                CheckUpdateUi(packetsWaiting);

                Message obj;
                var successDequeue = TeraSniffer.Instance.Packets.TryDequeue(out obj);
                if (!successDequeue)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var message = MessageFactory.Create(obj);
                if (message.GetType() == typeof(UnknownMessage)) { continue; }

                if (!PacketProcessing.Process(message))
                {
                    //Unprocessed packet
                }
            }
        }

        public void CheckUpdateUi(int packetsWaiting)
        {
            var second = DateTime.UtcNow.Ticks;
            if (second - _lastTick < TimeSpan.TicksPerSecond) { return; }
            UpdateUi(packetsWaiting);
        }

        internal virtual void OnGuildIconAction(Bitmap icon)
        {
            GuildIconAction?.Invoke(icon);
        }
    }
}