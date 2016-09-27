using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DamageMeter.AutoUpdate;
using DamageMeter.Database.Structures;
using DamageMeter.Sniffing;
using DamageMeter.TeraDpsApi;
using Data;
using log4net;
using Lang;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;
using Message = Tera.Message;
using DamageMeter.Processing;

namespace DamageMeter
{
    public class NetworkController
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void SetClickThrouEvent();

        public delegate void UnsetClickThrouEvent();


        public delegate void UpdateUiHandler(
            StatsSummary statsSummary, Skills skills, List<NpcEntity> entities, bool timedEncounter,
            AbnormalityStorage abnormals,
            ConcurrentDictionary<string, NpcEntity> bossHistory, List<ChatMessage> chatbox, int packetWaiting, Tuple<string, string> flash);

        public delegate void GuildIconEvent(Bitmap icon);

        private static NetworkController _instance;
        private readonly AbnormalityStorage _abnormalityStorage;
        private AbnormalityTracker _abnormalityTracker;
        private CharmTracker _charmTracker;
        public Tuple<string, string> FlashMessage { get; set; }

        private bool _clickThrou;

        private bool _forceUiUpdate;
        private bool _needInit;
        private long _lastTick;
        private MessageFactory _messageFactory = new MessageFactory();
        private UserLogoTracker UserLogoTracker = new UserLogoTracker();

        public ConcurrentDictionary<string, NpcEntity> BossLink = new ConcurrentDictionary<string, NpcEntity>();
        public CopyKey NeedToCopy;

        public bool NeedToExport;
        public bool NeedToReset;
        public bool NeedToResetCurrent;
        public PlayerTracker PlayerTracker;
        public Server Server;
        public GlyphBuild Glyphs = new GlyphBuild();

        private NetworkController()
        {
         
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            _abnormalityStorage = new AbnormalityStorage();
            var packetAnalysis = new Thread(PacketAnalysisLoop);

            packetAnalysis.Start();
        }

        public TeraData TeraData { get; private set; }
        public NpcEntity Encounter { get; private set; }
        public NpcEntity NewEncounter { get; set; }

        public bool TimedEncounter { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public EntityTracker EntityTracker { get; private set; }

        public event SetClickThrouEvent SetClickThrouAction;
        public event GuildIconEvent GuildIconAction;
        public event UnsetClickThrouEvent UnsetClickThrouAction;

        public bool SendFullDetails { get; set; }
        public void Exit()
        {
            BasicTeraData.Instance.WindowData.Save();
            BasicTeraData.Instance.HotkeysData.Save();
            TeraSniffer.Instance.Enabled = false;
            Application.Exit();
            Environment.Exit(0);
        }

        public event ConnectedHandler Connected;
        public event UpdateUiHandler TickUpdated;

        protected virtual void HandleNewConnection(Server server)
        {
            Server = server;
            _messageFactory = new MessageFactory();
            _needInit = true;
            Connected?.Invoke(server.Name);
        }

        public void Reset()
        {
            DamageTracker.Instance.Reset();
            _abnormalityStorage.ClearEnded();
            _forceUiUpdate = true;
        }

        public void ResetCurrent()
        {
            DamageTracker.Instance.DeleteEntity(Encounter);
            _forceUiUpdate = true;
        }

        private void UpdateUi(int packetsWaiting = 0)
        {
            if (BasicTeraData.Instance.WindowData.EnableChat != _messageFactory.ChatEnabled) _messageFactory.ChatEnabled = BasicTeraData.Instance.WindowData.EnableChat;
            _lastTick = DateTime.UtcNow.Ticks;
            var handler = TickUpdated;
            var currentBoss = Encounter;
            var timedEncounter = TimedEncounter;

            var entities = Database.Database.Instance.AllEntity();
            var filteredEntities = entities.Select(entityid => EntityTracker.GetOrNull(entityid)).OfType<NpcEntity>().Where(npc => npc.Info.Boss).ToList();
            if (packetsWaiting > 1500 && filteredEntities.Count > 1)
            {
                Database.Database.Instance.DeleteAllWhenTimeBelow(Encounter);
                entities = Database.Database.Instance.AllEntity();
                filteredEntities = entities.Select(entityid => EntityTracker.GetOrNull(entityid)).OfType<NpcEntity>().Where(npc => npc.Info.Boss).ToList();
            }

            var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter);
            Skills skills = null; 
            if (SendFullDetails)
            {
                skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
                SendFullDetails = false;
            }
            var playersInfo = timedEncounter
                ? Database.Database.Instance.PlayerDamageInformation(entityInfo.BeginTime, entityInfo.EndTime)
                : Database.Database.Instance.PlayerDamageInformation(currentBoss);

            var heals = Database.Database.Instance.PlayerHealInformation(entityInfo.BeginTime, entityInfo.EndTime);

            var flash = FlashMessage;
            FlashMessage = null;
       
            var statsSummary = new StatsSummary(playersInfo, heals, entityInfo);
            var teradpsHistory = BossLink;
            var chatbox = Chat.Instance.Get();
            var abnormals = _abnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
            handler?.Invoke(statsSummary, skills, filteredEntities, timedEncounter, abnormals, teradpsHistory, chatbox, packetsWaiting, flash);
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

        public static void CopyThread(StatsSummary stats, Skills skills, AbnormalityStorage abnormals,
            bool timedEncounter, CopyKey copy)
        {
            if (BasicTeraData.Instance.HotDotDatabase == null) return;//no database loaded yet => no need to do anything
            var text = CopyPaste.Copy(stats, skills, abnormals, timedEncounter, copy.Header, copy.Content, copy.Footer,
                copy.OrderBy, copy.Order);
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    //Ignore
                }
            }
            CopyPaste.Paste(text);
        }

        private void PacketAnalysisLoop()
        {
            try { Database.Database.Instance.DeleteAll(); }
            catch (Exception ex)
            {
                BasicTeraData.LogError(ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException + "\r\n" + ex.TargetSite, true);
                MessageBox.Show(LP.MainWindow_Fatal_error);
                Exit();
            }

            while (true)
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
                    var heals = Database.Database.Instance.PlayerHealInformation(entityInfo.BeginTime,
                        entityInfo.EndTime);
                    var statsSummary = new StatsSummary(playersInfo, heals, entityInfo);

                    var tmpcopy = NeedToCopy;
                    var abnormals = _abnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
                    var pasteThread =
                        new Thread(() => CopyThread(statsSummary, skills, abnormals, timedEncounter, tmpcopy))
                        {
                            Priority = ThreadPriority.Highest
                            
                        };
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

                if (NeedToExport)
                {
                    DataExporter.Export(Encounter, _abnormalityStorage);
                    NeedToExport = false;
                }

                Encounter = NewEncounter;

                var packetsWaiting = TeraSniffer.Instance.Packets.Count;
                if (packetsWaiting > 3000)
                {
                    MessageBox.Show(
                        LP.Your_computer_is_too_slow);
                    Exit();
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

                var message = _messageFactory.Create(obj);

                var cVersion = message as C_CHECK_VERSION;
                if (cVersion != null)
                {
                    Console.WriteLine("VERSION0 = " + cVersion.Versions[0]);
                    Console.WriteLine("VERSION1 = " + cVersion.Versions[1]);
                    var opCodeNamer =
                        new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory,
                            $"data/opcodes/{cVersion.Versions[0]}.txt"));
                    _messageFactory = new MessageFactory(opCodeNamer, Server.Region);
                    continue;
                }

                var sLogin = message as LoginServerMessage;
                if (sLogin != null)
                {
                    if (_needInit)
                    {
                        Connected(BasicTeraData.Instance.Servers.GetServerName(sLogin.ServerId, Server));
                        bool trackerreset = true;
                        if (EntityTracker != null)
                        {
                            try
                            {
                                var oldregion = BasicTeraData.Instance.Servers.GetServer(EntityTracker.MeterUser.ServerId).Region;
                                trackerreset = Server.Region != oldregion;
                            }
                            catch (Exception e)
                            {
                                BasicTeraData.LogError("New server:"+Server+";Old server Id:" +EntityTracker.MeterUser.ServerId, false, true);
                                throw;
                            }
                        }
                        Server = BasicTeraData.Instance.Servers.GetServer(sLogin.ServerId, Server);
                        _messageFactory.Version = Server.Region;
                        if (trackerreset)
                        {
                            TeraData = BasicTeraData.Instance.DataForRegion(Server.Region);
                            BasicTeraData.Instance.HotDotDatabase.Get(8888888).Name = LP.Enrage;
                            BasicTeraData.Instance.HotDotDatabase.Get(8888889).Name = LP.Slaying;
                            BasicTeraData.Instance.HotDotDatabase.Get(8888889).Tooltip = LP.SlayingTooltip;
                            EntityTracker = new EntityTracker(BasicTeraData.Instance.MonsterDatabase);
                            PlayerTracker = new PlayerTracker(EntityTracker, BasicTeraData.Instance.Servers);
                            Database.Database.Instance.DeleteAll();
                        }
                        _needInit = false;
                    }
                    _abnormalityStorage.EndAll(message.Time.Ticks);
                    _abnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker,
                        BasicTeraData.Instance.HotDotDatabase, _abnormalityStorage, DamageTracker.Instance.Update);
                    _charmTracker = new CharmTracker(_abnormalityTracker);
                    OnGuildIconAction(UserLogoTracker.GetLogo(sLogin.PlayerId));
                }

                if (_needInit)
                {
                    var guildIcon1 = message as S_GET_USER_GUILD_LOGO;
                    if (guildIcon1 != null)
                    {
                        UserLogoTracker.AddLogo(guildIcon1);
                        continue;
                    }

                    //Wait for initialization
                    continue;
                }

                EntityTracker.Update(message);

                var skillResultMessage = message as EachSkillResultServerMessage;
                if (skillResultMessage != null)
                {
                    var skillResult = new SkillResult(skillResultMessage, EntityTracker, PlayerTracker,
                        BasicTeraData.Instance.SkillDatabase, BasicTeraData.Instance.PetSkillDatabase);
                    DamageTracker.Instance.Update(skillResult);
                    continue;
                }

                var changeHp = message as SCreatureChangeHp;
                if (changeHp != null)
                {
                    _abnormalityTracker.Update(changeHp);
                    continue;
                }

                var pchangeHp = message as SPartyMemberChangeHp;
                if (pchangeHp != null)
                {
                    var user = PlayerTracker.GetOrNull(pchangeHp.ServerId, pchangeHp.PlayerId);
                    if (user == null) continue;
                    _abnormalityTracker.RegisterSlaying(user.User, pchangeHp.Slaying, pchangeHp.Time.Ticks);
                    continue;
                }

                var pmstatupd = message as S_PARTY_MEMBER_STAT_UPDATE;
                if (pmstatupd != null)
                {
                    var user = PlayerTracker.GetOrNull(pmstatupd.ServerId, pmstatupd.PlayerId);
                    if (user == null) continue;
                    _abnormalityTracker.RegisterSlaying(user.User, pmstatupd.Slaying, pmstatupd.Time.Ticks);
                    continue;
                }

                var pstatupd = message as S_PLAYER_STAT_UPDATE;
                if (pstatupd != null)
                {
                    _abnormalityTracker.RegisterSlaying(EntityTracker.MeterUser, pstatupd.Slaying, pstatupd.Time.Ticks);
                    continue;
                }

                var changeMp = message as SPlayerChangeMp;
                if (changeMp != null)
                {
                    if (changeMp.SourceId != EntityTracker.MeterUser.Id &&
                        changeMp.TargetId != EntityTracker.MeterUser.Id &&
                        EntityTracker.GetOrPlaceholder(changeHp.TargetId).RootOwner != EntityTracker.MeterUser)
                    {
                        var source = EntityTracker.GetOrPlaceholder(changeMp.SourceId);
                        BasicTeraData.LogError("SPlayerChangeMp need rootowner update2:" + (source as NpcEntity)?.Info.Name ?? source.GetType() + ": " + source, false, true);
                    }
                    _abnormalityTracker.Update(changeMp);
                    continue;
                }

                var npcStatus = message as SNpcStatus;
                if (npcStatus != null)
                {
                    _abnormalityTracker.RegisterNpcStatus(npcStatus);
                    continue;
                }

                var dead = message as SCreatureLife;
                if (dead != null)
                {
                    _abnormalityTracker.RegisterDead(dead);
                    continue;
                }

                var abnormalityBegin = message as SAbnormalityBegin;
                if (abnormalityBegin != null)
                {
                    _abnormalityTracker.AddAbnormality(abnormalityBegin);
                    continue;
                }

                var abnormalityEnd = message as SAbnormalityEnd;
                if (abnormalityEnd != null)
                {
                    _abnormalityTracker.DeleteAbnormality(abnormalityEnd);
                    continue;
                }

                var abnormalityRefresh = message as SAbnormalityRefresh;
                if (abnormalityRefresh != null)
                {
                    _abnormalityTracker.RefreshAbnormality(abnormalityRefresh);
                    continue;
                }

                var npcOccupier = message as SNpcOccupierInfo;
                if (npcOccupier != null)
                {
                    DamageTracker.Instance.UpdateEntities(new NpcOccupierResult(npcOccupier), npcOccupier.Time.Ticks);
                    continue;
                }

                var despawnNpc = message as SDespawnNpc;
                if (despawnNpc != null)
                {
                    _abnormalityTracker.StopAggro(despawnNpc);
                    _abnormalityTracker.DeleteAbnormality(despawnNpc);
                    DataExporter.Export(despawnNpc, _abnormalityStorage);
                    continue;
                }

                var despawnUser = message as SDespawnUser;
                if (despawnUser != null)
                {
                    _charmTracker.CharmReset(despawnUser.User, new List<CharmStatus>(), despawnUser.Time.Ticks);
                    _abnormalityTracker.DeleteAbnormality(despawnUser);
                    continue;
                }

                var charmEnable = message as SEnableCharmStatus;
                if (charmEnable != null)
                {
                    _charmTracker.CharmEnable(EntityTracker.MeterUser.Id, charmEnable.CharmId, charmEnable.Time.Ticks);
                    continue;
                }
                var pcharmEnable = message as SPartyMemberCharmEnable;
                if (pcharmEnable != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmEnable.ServerId, pcharmEnable.PlayerId);
                    if (player == null) continue;
                    _charmTracker.CharmEnable(player.User.Id, pcharmEnable.CharmId, pcharmEnable.Time.Ticks);
                    continue;
                }
                var charmReset = message as SResetCharmStatus;
                if (charmReset != null)
                {
                    _charmTracker.CharmReset(charmReset.TargetId, charmReset.Charms, charmReset.Time.Ticks);
                    continue;
                }
                var pcharmReset = message as SPartyMemberCharmReset;
                if (pcharmReset != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmReset.ServerId, pcharmReset.PlayerId);
                    if (player == null) continue;
                    _charmTracker.CharmReset(player.User.Id, pcharmReset.Charms, pcharmReset.Time.Ticks);
                    continue;
                }
                var charmDel = message as SRemoveCharmStatus;
                if (charmDel != null)
                {
                    _charmTracker.CharmDel(EntityTracker.MeterUser.Id, charmDel.CharmId, charmDel.Time.Ticks);
                    continue;
                }
                var pcharmDel = message as SPartyMemberCharmDel;
                if (pcharmDel != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmDel.ServerId, pcharmDel.PlayerId);
                    if (player == null) continue;
                    _charmTracker.CharmDel(player.User.Id, pcharmDel.CharmId, pcharmDel.Time.Ticks);
                    continue;
                }
                var charmAdd = message as SAddCharmStatus;
                if (charmAdd != null)
                {
                    _charmTracker.CharmAdd(charmAdd.TargetId, charmAdd.CharmId, charmAdd.Status, charmAdd.Time.Ticks);
                    continue;
                }
                var pcharmAdd = message as SPartyMemberCharmAdd;
                if (pcharmAdd != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmAdd.ServerId, pcharmAdd.PlayerId);
                    if (player == null) continue;
                    _charmTracker.CharmAdd(player.User.Id, pcharmAdd.CharmId, pcharmAdd.Status, pcharmAdd.Time.Ticks);
                    continue;
                }
       
                PlayerTracker.UpdateParty(message);

                var sSpawnUser = message as SpawnUserServerMessage;
                if (sSpawnUser != null)
                {
                    _abnormalityTracker.RegisterDead(sSpawnUser.Id, sSpawnUser.Time.Ticks, sSpawnUser.Dead);
                    //Debug.WriteLine(sSpawnUser.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.Id.Id)) + " : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.ServerId)) + " " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.PlayerId)));
                    continue;
                }

                if (BasicTeraData.Instance.WindowData.EnableChat)
                {
                    var chatMessage = message as S_CHAT;
                    if (chatMessage != null)
                    {
                        Chat.Instance.Add(chatMessage);
                        continue;
                    }

                    var whisperMessage = message as S_WHISPER;
                    if (whisperMessage != null)
                    {
                        Chat.Instance.Add(whisperMessage);
                        continue;
                    }

                    var privateChatMessage = message as S_PRIVATE_CHAT;
                    if (privateChatMessage != null)
                    {
                        Chat.Instance.Add(privateChatMessage);
                        continue;
                    }

                    var readyParty = message as S_CHECK_TO_READY_PARTY;
                    if (readyParty != null)
                    {
                        if (readyParty.Count == 1)
                        {
                            if (!TeraWindow.IsTeraActive())
                            {
                                FlashMessage = new Tuple<string, string>(
                                    LP.CombatReadyCheck,
                                    LP.CombatReadyCheck
                                    );
                            }
                        }
                        continue;
                    }

                    var trading = message as S_TRADE_BROKER_DEAL_SUGGESTED;
                    if (trading != null)
                    {
                        if (!TeraWindow.IsTeraActive())
                        {
                            FlashMessage = new Tuple<string, string>(
                                LP.Trading + ": " + trading.PlayerName,
                                LP.SellerPrice + ": " + S_TRADE_BROKER_DEAL_SUGGESTED.Gold(trading.SellerPrice) +
                                Environment.NewLine +
                                LP.OfferedPrice + ": " + S_TRADE_BROKER_DEAL_SUGGESTED.Gold(trading.OfferedPrice)
                                );
                        }
                        continue;

                    }

                    var userApply = message as S_OTHER_USER_APPLY_PARTY;
                    if (userApply != null)
                    {
                        if (!TeraWindow.IsTeraActive())
                        {

                            FlashMessage = new Tuple<string, string>(
                                userApply.PlayerName + " " + LP.ApplyToYourParty,
                                LP.Class + ": " +
                                LP.ResourceManager.GetString(userApply.PlayerClass.ToString(), LP.Culture) +
                                Environment.NewLine +
                                LP.Lvl + ": " + userApply.Lvl + Environment.NewLine
                                );
                        }

                        if (BasicTeraData.Instance.WindowData.CopyInspect)
                        {
                            var thread = new Thread(() => CopyPaste.CopyInspect(userApply.PlayerName));
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                        }
                        continue;
                    }

                    var guildquest = message as S_GUILD_QUEST_LIST;
                    if(guildquest != null)
                    {
                        GuildQuestList.Process(guildquest);
                    }

                    var contact = message as S_REQUEST_CONTRACT;
                    if (contact != null)
                    {
                        if (!TeraWindow.IsTeraActive())
                        {
                            if (contact.Type == S_REQUEST_CONTRACT.RequestType.PartyInvite)
                            {
                                FlashMessage = new Tuple<string, string>(
                                    LP.PartyInvite + ": " + contact.Sender,
                                    contact.Sender
                                    );
                            }
                            else if (contact.Type == S_REQUEST_CONTRACT.RequestType.TradeRequest)
                            {
                                FlashMessage = new Tuple<string, string>(
                                    LP.Trading + ": " + contact.Sender,
                                    contact.Sender
                                    );
                            }
                            else if (contact.Type != S_REQUEST_CONTRACT.RequestType.Craft)
                            {
                                FlashMessage = new Tuple<string, string>(
                                    LP.ContactTry,
                                    LP.ContactTry
                                    );
                            }
                        }
                        continue;
                    }


                    var partyMatch = message as S_FIN_INTER_PARTY_MATCH;
                    var bgMatch = message as S_BATTLE_FIELD_ENTRANCE_INFO;
                    if (partyMatch != null || bgMatch != null)
                    {
                        if (!TeraWindow.IsTeraActive())
                        {
                            FlashMessage = new Tuple<string, string>(
                                LP.PartyMatchingSuccess,
                                LP.PartyMatchingSuccess
                                );
                        }
                        continue;
                    }
                }
                var spawnMe = message as SpawnMeServerMessage;
                if (spawnMe != null)
                {
                    _abnormalityStorage.EndAll(message.Time.Ticks);
                    _abnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker,
                        BasicTeraData.Instance.HotDotDatabase, _abnormalityStorage, DamageTracker.Instance.Update);
                    _charmTracker = new CharmTracker(_abnormalityTracker);
                    _abnormalityTracker.RegisterDead(spawnMe.Id, spawnMe.Time.Ticks, spawnMe.Dead);
                    continue;
                }

                var guildIcon = message as S_GET_USER_GUILD_LOGO;
                if (guildIcon != null)
                {
                    UserLogoTracker.AddLogo(guildIcon);
                    continue;
                }

                var user_list = message as S_GET_USER_LIST;
                if (user_list != null)
                {
                    UserLogoTracker.SetUserList(user_list);
                    continue;
                }

                var crest_info = message as S_CREST_INFO;
                if (crest_info != null)
                {
                    Glyphs.playerServer = BasicTeraData.Instance.Servers.GetServerName(EntityTracker.MeterUser.ServerId);
                    Glyphs.playerName = EntityTracker.MeterUser.Name;
                    Glyphs.playerClass = EntityTracker.MeterUser.RaceGenderClass.Class.ToString();
                    Glyphs.glyphs = crest_info.Glyphs;
                    continue;
                }
                //Debug.WriteLine(sLogin.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sLogin.Id.Id)));
            }
        }
     
        public void CheckUpdateUi(int packetsWaiting)
        {
            var second = DateTime.UtcNow.Ticks;
            if (second - _lastTick < TimeSpan.TicksPerSecond) return;
            UpdateUi(packetsWaiting);
        }

        protected virtual void OnGuildIconAction(Bitmap icon)
        {
            GuildIconAction?.Invoke(icon);
        }
    }
}