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
using Tera.RichPresence;

namespace DamageMeter
{
    public class PacketProcessor
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void GuildIconEvent(Bitmap icon);

        public delegate void SetClickThrouEvent();

        public delegate void UnsetClickThrouEvent();

        public delegate void PauseEvent(bool paused);

        public delegate void UpdateUiHandler(UiUpdateMessage message);

        public event Action<bool, EntityId> DisplayGeneralDataChanged;

        private static volatile PacketProcessor _instance;
        private static readonly object _lock = new object();
        private static readonly object _pasteLock = new object();
        internal AbnormalityStorage AbnormalityStorage;

        internal readonly List<Player> MeterPlayers = new List<Player>();
        internal bool ForceUiUpdate;
        private bool _keepAlive = true;
        private long _lastTick;
        internal AbnormalityTracker AbnormalityTracker;
        public ConcurrentDictionary<UploadData, NpcEntity> BossLink = new ConcurrentDictionary<UploadData, NpcEntity>();
        public GlyphBuild Glyphs = new GlyphBuild();
        internal MessageFactory MessageFactory = new MessageFactory();
        internal bool NeedInit = true;
        public CopyKey NeedToCopy;

        public DataExporter.Dest NeedToExport = DataExporter.Dest.None;
        public bool NeedToReset;
        public bool NeedToResetCurrent;
        public bool NeedPause;

        internal PacketProcessingFactory PacketProcessing = new PacketProcessingFactory();
        public Server Server;
        internal UserLogoTracker UserLogoTracker = new UserLogoTracker();

        private PacketProcessor()
        {
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            TeraSniffer.Instance.EndConnection += HandleEndConnection;
            AbnormalityStorage = new AbnormalityStorage();
            var packetAnalysis = new Thread(PacketAnalysisLoop);
            packetAnalysis.Start();
            TeraSniffer.Instance.EnableMessageStorage = BasicTeraData.Instance.WindowData.PacketsCollect;
        }

        public List<NotifyFlashMessage> FlashMessage = new List<NotifyFlashMessage>();
        public PlayerTracker PlayerTracker { get; internal set; }

        public TeraData TeraData { get; internal set; }
        public NpcEntity Encounter { get; private set; }
        public NpcEntity NewEncounter { get; set; }

        public bool TimedEncounter { get; set; }

        private bool _overloaded;

        public bool Overloaded
        {
            get => _overloaded;
            private set
            {
                if (_overloaded == value) return;
                _overloaded = value;
                OverloadedChanged?.Invoke();
            }
        }

        public static PacketProcessor Instance
        {
            get
            {
                if (_instance == null)
                    lock (_lock)
                        if (_instance == null)
                            _instance = new PacketProcessor();
                return _instance;
            }
        }

        public EntityTracker EntityTracker { get; internal set; }
        public bool SendFullDetails { get; set; }

        public event SetClickThrouEvent SetClickThrouAction;
        public event GuildIconEvent GuildIconAction;
        public event UnsetClickThrouEvent UnsetClickThrouAction;
        public event PauseEvent PauseAction;
        public event Action MapChangedAction;
        public event Action OverloadedChanged;

        public void Exit()
        {
            if (_keepAlive)
            {
                BasicTeraData.Instance.WindowData.Save();
                BasicTeraData.Instance.WindowData.Close();
                BasicTeraData.Instance.HotkeysData.Save();
            }
            TeraSniffer.Instance.Enabled = false;
            _keepAlive = false;
            Thread.Sleep(500);
            HudManager.Instance.CurrentBosses.DisposeAll();
            RichPresence.Instance.Deinitialize();
            Application.Exit();
        }

        internal void RaiseConnected(string message)
        {
            Connected?.Invoke(message);
        }

        public void RaisePause(bool pause)
        {
            PauseAction?.Invoke(pause);
        }
        public void RaiseMapChanged()
        {
            MapChangedAction?.Invoke();
        }

        public event ConnectedHandler Connected;
        public event UpdateUiHandler TickUpdated;

        protected virtual void HandleEndConnection()
        {
            NeedInit = true;
            MessageFactory = new MessageFactory();
            NotifyProcessor.Instance.S_LOAD_TOPO(null);
            RichPresence.Instance.HandleEndConnection();
            Connected?.Invoke(LP.SystemTray_No_server);
            OnGuildIconAction(null);
        }

        protected virtual void HandleNewConnection(Server server)
        {
            Server = server;
            NeedInit = true;
            MessageFactory = new MessageFactory();
            RichPresence.Instance.HandleConnected(server);
            Connected?.Invoke(server.Name);
        }

        public void Reset()
        {
            DamageTracker.Instance.Reset();
            AbnormalityStorage.ClearEnded();
            ForceUiUpdate = true;
        }

        public void ResetCurrent()
        {
            DamageTracker.Instance.DeleteEntity(Encounter);
            ForceUiUpdate = true;
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

            var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter, BasicTeraData.Instance.WindowData.DisplayTimerBasedOnAggro);
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

            var flash = FlashMessage.Where(x => x != null).ToList();
            FlashMessage = new List<NotifyFlashMessage>();

            var statsSummary = new StatsSummary(playersInfo, heals, entityInfo);
            var teradpsHistory = BossLink;
            var chatbox = Chat.Instance.Get();
            var abnormals = AbnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
            var uiMessage = new UiUpdateMessage(statsSummary, skills, filteredEntities, timedEncounter, abnormals, teradpsHistory, chatbox, flash, packetsWaiting);
            handler?.Invoke(uiMessage);
            RichPresence.Instance.Invoke();

        }

        public List<DpsServer> Initialize()
        {
            var listForUi = new List<DpsServer>();
            DataExporter.DpsServers = new List<DpsServer> { DpsServer.NeowutranAnonymousServer };
            foreach (var dpsServer in BasicTeraData.Instance.WindowData.DpsServers)
            {
                var server = new DpsServer(dpsServer, false);
                listForUi.Add(server);
                DataExporter.DpsServers.Add(server);
            }
            return listForUi;
        }

        public void SwitchClickThrou()
        {
            if (BasicTeraData.Instance.WindowData.ClickThrou)
            {
                UnsetClickThrou();
                BasicTeraData.Instance.WindowData.ClickThrou = false;
                return;
            }
            SetClickThrou();
            BasicTeraData.Instance.WindowData.ClickThrou = true;
        }

        public void SwitchClickThrou(bool value)
        {
            if (value)
            {
                SetClickThrou();
                BasicTeraData.Instance.WindowData.ClickThrou = true;
                return;
            }
            UnsetClickThrou();
            BasicTeraData.Instance.WindowData.ClickThrou = false;
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

            RichPresence.Instance.Initialize();

            while (_keepAlive)
            {
                if (NeedToCopy != null)
                {
                    var currentBoss = Encounter;
                    var timedEncounter = TimedEncounter;

                    var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter, BasicTeraData.Instance.WindowData.DisplayTimerBasedOnAggro);
                    var skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
                    var playersInfo = timedEncounter
                        ? Database.Database.Instance.PlayerDamageInformation(entityInfo.BeginTime, entityInfo.EndTime)
                        : Database.Database.Instance.PlayerDamageInformation(currentBoss);
                    var heals = Database.Database.Instance.PlayerHealInformation(entityInfo.BeginTime, entityInfo.EndTime);
                    var statsSummary = new StatsSummary(playersInfo, heals, entityInfo);

                    var tmpcopy = NeedToCopy;
                    var abnormals = AbnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
                    var pasteThread = new Thread(() => CopyThread(statsSummary, skills, abnormals, timedEncounter, tmpcopy)) { Priority = ThreadPriority.Highest };
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
                    if (!BasicTeraData.Instance.WindowData.IgnorePacketsThreshold)
                    {
                        Pause();
                        RaisePause(true);
                    }

                    Overloaded = true;

                }
                else
                {
                    Overloaded = false;
                }

                if (NeedPause)
                {
                    Pause(false);
                    NeedPause = false;
                }

                if (ForceUiUpdate)
                {
                    UpdateUi(packetsWaiting);
                    ForceUiUpdate = false;
                }

                CheckUpdateUi(packetsWaiting);

                var successDequeue = TeraSniffer.Instance.Packets.TryDequeue(out Message obj);
                if (!successDequeue)
                {
                    Thread.Sleep(1);
                    continue;
                }
                //Thread.Sleep(100); // intentional lag

                var message = MessageFactory.Create(obj);
                if (message.GetType() == typeof(UnknownMessage)) { continue; }

                if (!PacketProcessing.Process(message))
                {
                    //Unprocessed packet
                }
            }
        }

        private void Pause(bool reset = true)
        {
            PacketProcessing.Pause();
            if (reset)
            {
                Database.Database.Instance.DeleteAll();
                AbnormalityStorage = new AbnormalityStorage();
                AbnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker, BasicTeraData.Instance.HotDotDatabase, AbnormalityStorage, DamageTracker.Instance.Update);
                if (MessageFactory.ChatEnabled)
                {
                    AbnormalityTracker.AbnormalityAdded += NotifyProcessor.Instance.AbnormalityNotifierAdded;
                    AbnormalityTracker.AbnormalityRemoved += NotifyProcessor.Instance.AbnormalityNotifierRemoved;
                }
            }
            else
            {
                AbnormalityStorage.EndAll(DateTime.UtcNow.Ticks);
            }
            TeraSniffer.Instance.Packets = new ConcurrentQueue<Message>();
            HudManager.Instance.CurrentBosses.DisposeAll();
            NotifyProcessor.Instance.S_LOAD_TOPO(null);
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

        public void InvokeGeneralDataDisplayChanged(bool hide, EntityId eid)
        {
            DisplayGeneralDataChanged?.Invoke(hide, eid);
        }
    }
}