using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DamageMeter.Sniffing;
using Data;
using Tera.Game;
using Tera.Game.Messages;
using Message = Tera.Message;

namespace DamageMeter
{
    public class NetworkController
    {
        public delegate void ConnectedHandler(string serverName);
        public delegate void SetClickThrouEvent();
        public delegate void UnsetClickThrouEvent();

        public event SetClickThrouEvent SetClickThrouAction;
        public event UnsetClickThrouEvent UnsetClickThrouAction;


        public delegate void UpdateUiHandler(
            long firsthit, long lastHit, long totaldamage, long partyDps, Dictionary<Entity, EntityInfo> entities,
            List<PlayerInfo> stats, Entity currentBoss, bool timedEncounter, AbnormalityStorage abnormals, ConcurrentDictionary<string ,Entity> bossHistory, List<ChatMessage> chatbox);

        private static NetworkController _instance;

        private long _lastTick;
        private MessageFactory _messageFactory;
        public PlayerTracker PlayerTracker;
        public Server Server;
        private CharmTracker CharmTracker;
        private AbnormalityTracker AbnormalityTracker;
        private AbnormalityStorage _abnormalityStorage;

        private NetworkController()
        {
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            _abnormalityStorage = new AbnormalityStorage();
            var packetAnalysis = new Thread(PacketAnalysisLoop);

            packetAnalysis.Start();
        }

        public ConcurrentDictionary<string, Entity> BossLink = new ConcurrentDictionary<string, Entity>();

        public TeraData TeraData { get; private set; }

        public Entity Encounter { get; private set; }
        public Entity NewEncounter { get; set; }

        public bool TimedEncounter { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public EntityTracker EntityTracker { get; private set; }

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
            PlayerTracker = new PlayerTracker(EntityTracker,BasicTeraData.Instance.Servers);
            _messageFactory = new MessageFactory(TeraData.OpCodeNamer);
            var handler = Connected;
            handler?.Invoke(server.Name);
        }

        private bool _forceUiUpdate = false;


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
            var currentBossFight = Encounter;
            var timedEncounter = TimedEncounter;
            var damage = DamageTracker.Instance.TotalDamage(currentBossFight, timedEncounter);
            var stats =
                DamageTracker.Instance.GetPlayerStats()
                    .OrderByDescending(playerStats => playerStats.Dealt.Damage(currentBossFight, timedEncounter))
                    .ToList();
            var firstHit = DamageTracker.Instance.FirstHit(currentBossFight);
            var lastHit = DamageTracker.Instance.LastHit(currentBossFight);
            var entities =
                DamageTracker.Instance.GetEntityStats();
            var partyDps = DamageTracker.Instance.PartyDps(currentBossFight, timedEncounter);
            var teradpsHistory = BossLink;
            var chatbox = Chat.Instance.Get();
            var abnormals = _abnormalityStorage.Clone(currentBossFight?.NpcE, firstHit*TimeSpan.TicksPerSecond, (lastHit + 1)*TimeSpan.TicksPerSecond - 1);
            handler?.Invoke(firstHit, lastHit, damage, partyDps, entities, stats, currentBossFight, timedEncounter, abnormals, teradpsHistory, chatbox);
        }

        private bool _clickThrou = false;
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

        public bool NeedToReset = false;
        public bool NeedToResetCurrent = false;
        public CopyKey NeedToCopy = null;

        public static void CopyThread(EntityInfo info, List<PlayerInfo> stats, AbnormalityStorage abnormals, long total, Entity currentBoss, bool timedEncounter, CopyKey copy)
        {

            var text = CopyPaste.Copy(info, stats, abnormals, total, currentBoss, timedEncounter, copy.Header, copy.Content, copy.Footer, copy.OrderBy, copy.Order);
            CopyPaste.Paste(text);
            
        }

        private void PacketAnalysisLoop()
        {
            while (true)
            {

                if (NeedToCopy != null)
                {
                    var stats = DamageTracker.Instance.GetPlayerStats();
                    var currentBoss = Encounter;
                    var timedEncounter = TimedEncounter;
                    var totaldamage = DamageTracker.Instance.TotalDamage(currentBoss, timedEncounter);
                    var firstHit = DamageTracker.Instance.FirstHit(currentBoss);
                    var lastHit = DamageTracker.Instance.LastHit(currentBoss);
                    var info = currentBoss == null ? new EntityInfo { FirstHit = firstHit * TimeSpan.TicksPerSecond, LastHit = (lastHit + 1) * TimeSpan.TicksPerSecond - 1 } : DamageTracker.Instance.GetEntityStats()[currentBoss];
                    var tmpcopy = NeedToCopy;
                    var abnormals = _abnormalityStorage.Clone(currentBoss?.NpcE, info.FirstHit, info.LastHit);
                    var pasteThread = new Thread(() => CopyThread(info, stats, abnormals, totaldamage, currentBoss, timedEncounter, tmpcopy));
                    pasteThread.Priority = ThreadPriority.Highest;
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
                    var skillResult = new SkillResult(skillResultMessage, EntityTracker, PlayerTracker, BasicTeraData.Instance.SkillDatabase, BasicTeraData.Instance.PetSkillDatabase);
                    DamageTracker.Instance.Update(skillResult);
                    continue;
                }
                var changeHp = message as SCreatureChangeHp;
                if (changeHp != null)
                {
                    AbnormalityTracker.Update(changeHp);
                    continue;
                }

                var pchangeHp = message as SPartyMemberChangeHp;
                if (pchangeHp != null)
                {
                    var user = PlayerTracker.GetOrNull(pchangeHp.ServerId, pchangeHp.PlayerId);
                    if (user==null)continue;
                    AbnormalityTracker.RegisterSlaying(user.User, pchangeHp.Slaying, pchangeHp.Time.Ticks);
                    continue;
                }

                var pmstatupd = message as S_PARTY_MEMBER_STAT_UPDATE;
                if (pmstatupd != null)
                {
                    var user = PlayerTracker.GetOrNull(pmstatupd.ServerId, pmstatupd.PlayerId);
                    if (user == null) continue;
                    AbnormalityTracker.RegisterSlaying(user.User, pmstatupd.Slaying, pmstatupd.Time.Ticks);
                    continue;
                }

                var pstatupd = message as S_PLAYER_STAT_UPDATE;
                if (pstatupd != null)
                {
                    AbnormalityTracker.RegisterSlaying(EntityTracker.MeterUser, pstatupd.Slaying, pstatupd.Time.Ticks);
                    continue;
                }

                var changeMp = message as SPlayerChangeMp;
                if (changeMp != null)
                {
                    AbnormalityTracker.Update(changeMp);
                    continue;
                }

                var npcStatus = message as SNpcStatus;
                if (npcStatus != null)
                {
                    AbnormalityTracker.RegisterNpcStatus(npcStatus);
                    continue;
                }

                var dead = message as SCreatureLife;
                if (dead != null)
                {
                    AbnormalityTracker.RegisterDead(dead);
                    continue;
                }

                var abnormalityBegin = message as SAbnormalityBegin;
                if (abnormalityBegin != null)
                {
                    AbnormalityTracker.AddAbnormality(abnormalityBegin);
                    continue;
                }

                var abnormalityEnd = message as SAbnormalityEnd;
                if (abnormalityEnd != null)
                {
                    AbnormalityTracker.DeleteAbnormality(abnormalityEnd);
                    continue;
                }

                var abnormalityRefresh = message as SAbnormalityRefresh;
                if (abnormalityRefresh != null)
                {
                    AbnormalityTracker.RefreshAbnormality(abnormalityRefresh);
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
                    AbnormalityTracker.StopAggro(despawnNpc);
                    AbnormalityTracker.DeleteAbnormality(despawnNpc);

                    DataExporter.Export(despawnNpc, _abnormalityStorage);
           

                    continue;
                }

                var chatMessage = message as S_CHAT;
                if(chatMessage != null)
                {
                    Chat.Instance.Add(chatMessage);
                    continue;
                }

                var whisperMessage = message as S_WHISPER;
                if(whisperMessage != null)
                {
                    Chat.Instance.Add(whisperMessage);
                    continue;
                }

                var despawnUser = message as SDespawnUser;
                if (despawnUser != null)
                {
                    CharmTracker.CharmReset(despawnUser.User, new List<CharmStatus>(), despawnUser.Time.Ticks);
                    AbnormalityTracker.DeleteAbnormality(despawnUser);
                    continue;
                }

                var charmEnable = message as SEnableCharmStatus;
                if (charmEnable != null)
                {
                    CharmTracker.CharmEnable(EntityTracker.MeterUser.Id, charmEnable.CharmId,charmEnable.Time.Ticks);
                    continue;
                }
                var pcharmEnable = message as SPartyMemberCharmEnable;
                if (pcharmEnable != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmEnable.ServerId, pcharmEnable.PlayerId);
                    if (player == null) continue;
                    CharmTracker.CharmEnable(player.User.Id, pcharmEnable.CharmId, pcharmEnable.Time.Ticks);
                    continue;
                }
                var charmReset = message as SResetCharmStatus;
                if (charmReset != null)
                {
                    CharmTracker.CharmReset(charmReset.TargetId, charmReset.Charms, charmReset.Time.Ticks);
                    continue;
                }
                var pcharmReset = message as SPartyMemberCharmReset;
                if (pcharmReset != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmReset.ServerId, pcharmReset.PlayerId);
                    if (player == null) continue;
                    CharmTracker.CharmReset(player.User.Id, pcharmReset.Charms, pcharmReset.Time.Ticks);
                    continue;
                }
                var charmDel = message as SRemoveCharmStatus;
                if (charmDel != null)
                {
                    CharmTracker.CharmDel(EntityTracker.MeterUser.Id, charmDel.CharmId, charmDel.Time.Ticks);
                    continue;
                }
                var pcharmDel = message as SPartyMemberCharmDel;
                if (pcharmDel != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmDel.ServerId, pcharmDel.PlayerId);
                    if (player == null) continue;
                    CharmTracker.CharmDel(player.User.Id, pcharmDel.CharmId, pcharmDel.Time.Ticks);
                    continue;
                }
                var charmAdd = message as SAddCharmStatus;
                if (charmAdd != null)
                {
                    CharmTracker.CharmAdd(charmAdd.TargetId, charmAdd.CharmId, charmAdd.Status, charmAdd.Time.Ticks);
                    continue;
                }
                var pcharmAdd = message as SPartyMemberCharmAdd;
                if (pcharmAdd != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmAdd.ServerId, pcharmAdd.PlayerId);
                    if (player == null) continue;
                    CharmTracker.CharmAdd(player.User.Id, pcharmAdd.CharmId, pcharmAdd.Status, pcharmAdd.Time.Ticks);
                    continue;
                }

                EntityTracker.Update(message);
                PlayerTracker.UpdateParty(message);
                var sSpawnUser = message as SpawnUserServerMessage;
                if (sSpawnUser != null)
                {
                    AbnormalityTracker.RegisterDead(sSpawnUser.Id, sSpawnUser.Time.Ticks, sSpawnUser.Dead);
                    //Debug.WriteLine(sSpawnUser.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.Id.Id)) + " : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.ServerId)) + " " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.PlayerId)));
                    continue;
                }

                var spawnMe = message as SpawnMeServerMessage;
                if (spawnMe != null)
                {
                    _abnormalityStorage.EndAll(message.Time.Ticks);
                    AbnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker, BasicTeraData.Instance.HotDotDatabase, _abnormalityStorage, DamageTracker.Instance.Update);
                    CharmTracker = new CharmTracker(AbnormalityTracker);
                    AbnormalityTracker.RegisterDead(spawnMe.Id, spawnMe.Time.Ticks, spawnMe.Dead);
                    continue;
                }
                var sLogin = message as LoginServerMessage;
                if (sLogin != null)
                {
                    _abnormalityStorage.EndAll(message.Time.Ticks);
                    AbnormalityTracker = new AbnormalityTracker(EntityTracker, PlayerTracker, BasicTeraData.Instance.HotDotDatabase, _abnormalityStorage, DamageTracker.Instance.Update);
                    CharmTracker = new CharmTracker(AbnormalityTracker);
                    Connected(BasicTeraData.Instance.Servers.GetServerName(sLogin.ServerId, Server));
                    //Debug.WriteLine(sLogin.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sLogin.Id.Id)));
                    continue;
                }
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
