using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DamageMeter.Database.Structures;
using DamageMeter.Sniffing;
using Data;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;
using Message = Tera.Message;

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
            ConcurrentDictionary<string, NpcEntity> bossHistory, List<ChatMessage> chatbox);

        private static NetworkController _instance;
        private readonly AbnormalityStorage _abnormalityStorage;
        private AbnormalityTracker _abnormalityTracker;
        private CharmTracker _charmTracker;

        private bool _clickThrou;

        private bool _forceUiUpdate;

        private long _lastTick;
        private MessageFactory _messageFactory;

        public ConcurrentDictionary<string, NpcEntity> BossLink = new ConcurrentDictionary<string, NpcEntity>();
        public CopyKey NeedToCopy;

        public bool NeedToReset;
        public bool NeedToResetCurrent;
        public PlayerTracker PlayerTracker;
        public Server Server;

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
        public event UnsetClickThrouEvent UnsetClickThrouAction;

        public int SendFullDetails { get; set; }
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

        private void HandleNewConnection(Server server)
        {
            Server = server;
            TeraData = BasicTeraData.Instance.DataForRegion(server.Region);
            EntityTracker = new EntityTracker(BasicTeraData.Instance.MonsterDatabase);
            PlayerTracker = new PlayerTracker(EntityTracker, BasicTeraData.Instance.Servers);
            _messageFactory = new MessageFactory(TeraData.OpCodeNamer);
            var handler = Connected;
            handler?.Invoke(server.Name);
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

        private void UpdateUi()
        {
            _lastTick = DateTime.UtcNow.Ticks;
            var handler = TickUpdated;
            var currentBoss = Encounter;
            var timedEncounter = TimedEncounter;


            var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter);
            Skills skills = null; 
            if (SendFullDetails != 0)
            {
                skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
            }
            var playersInfo = timedEncounter
                ? Database.Database.Instance.PlayerInformation(entityInfo.BeginTime, entityInfo.EndTime)
                : Database.Database.Instance.PlayerInformation(currentBoss);

            var entities = Database.Database.Instance.AllEntity();
            var filteredEntities = entities.Select(entityid => EntityTracker.GetOrNull(entityid)).OfType<NpcEntity>().Where(npc => npc.Info.Boss).ToList();
            var statsSummary = new StatsSummary(playersInfo, entityInfo);
            var teradpsHistory = BossLink;
            var chatbox = Chat.Instance.Get();
            var abnormals = _abnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
            handler?.Invoke(statsSummary, skills, filteredEntities, timedEncounter, abnormals, teradpsHistory, chatbox);
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

        private void SetClickThrou()
        {
            var handler = SetClickThrouAction;
            handler?.Invoke();
        }

        private void UnsetClickThrou()
        {
            var handler = UnsetClickThrouAction;
            handler?.Invoke();
        }

        public static void CopyThread(StatsSummary stats, Skills skills, AbnormalityStorage abnormals,
            bool timedEncounter, CopyKey copy)
        {
            var text = CopyPaste.Copy(stats, skills, abnormals, timedEncounter, copy.Header, copy.Content, copy.Footer,
                copy.OrderBy, copy.Order);
            CopyPaste.Paste(text);
        }

        private void PacketAnalysisLoop()
        {
            while (true)
            {
                if (NeedToCopy != null)
                {
                    var currentBoss = Encounter;
                    var timedEncounter = TimedEncounter;

                    var entityInfo = Database.Database.Instance.GlobalInformationEntity(currentBoss, timedEncounter);
                    var skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
                    var playersInfo = timedEncounter
                        ? Database.Database.Instance.PlayerInformation(entityInfo.BeginTime, entityInfo.EndTime)
                        : Database.Database.Instance.PlayerInformation(currentBoss);
                    var statsSummary = new StatsSummary(playersInfo, entityInfo);

                    var tmpcopy = NeedToCopy;
                    var abnormals = _abnormalityStorage.Clone(currentBoss, entityInfo.BeginTime, entityInfo.EndTime);
                    var pasteThread =
                        new Thread(() => CopyThread(statsSummary, skills, abnormals, timedEncounter, tmpcopy))
                        {
                            Priority = ThreadPriority.Highest
                        };
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

                Encounter = NewEncounter;

                if (_forceUiUpdate)
                {
                    UpdateUi();
                    _forceUiUpdate = false;
                }

                CheckUpdateUi();


                Message obj;
                var successDequeue = TeraSniffer.Instance.Packets.TryDequeue(out obj);
                if (!successDequeue)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (TeraSniffer.Instance.Packets.Count > 3000)
                {
                    MessageBox.Show(
                        "Your computer is too slow to use this DPS meter. Can't analyse all those packet in decent amount of time. Shutting down now.");
                    Exit();
                }

                var message = _messageFactory.Create(obj);

                var skillResultMessage = message as EachSkillResultServerMessage;
                if (skillResultMessage != null)
                {
                    var skillResult = new SkillResult(skillResultMessage, EntityTracker, PlayerTracker,
                        BasicTeraData.Instance.SkillDatabase, BasicTeraData.Instance.PetSkillDatabase);
                    DamageTracker.Instance.Update(skillResult);
                    continue;
                }

                EntityTracker.Update(message);

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

                var spawnMe = message as SpawnMeServerMessage;
                if (spawnMe != null)
                {
                    Reset();
                    _abnormalityStorage.EndAll(message.Time.Ticks);
                    _abnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker,
                        BasicTeraData.Instance.HotDotDatabase, _abnormalityStorage, DamageTracker.Instance.Update);
                    _charmTracker = new CharmTracker(_abnormalityTracker);
                    _abnormalityTracker.RegisterDead(spawnMe.Id, spawnMe.Time.Ticks, spawnMe.Dead);
                    continue;
                }

              
                var sLogin = message as LoginServerMessage;
                if (sLogin == null) continue;
                _abnormalityStorage.EndAll(message.Time.Ticks);
                _abnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker,
                    BasicTeraData.Instance.HotDotDatabase, _abnormalityStorage, DamageTracker.Instance.Update);
                _charmTracker = new CharmTracker(_abnormalityTracker);
                Connected(BasicTeraData.Instance.Servers.GetServerName(sLogin.ServerId, Server));
                //Debug.WriteLine(sLogin.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sLogin.Id.Id)));
            }
        }
     
        public void CheckUpdateUi()
        {
            var second = DateTime.UtcNow.Ticks;
            if (second - _lastTick < TimeSpan.TicksPerSecond) return;
            UpdateUi();
        }
    }
}