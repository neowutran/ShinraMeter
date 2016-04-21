using System;
using System.Collections.Generic;
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
            List<PlayerInfo> stats, Entity currentBoss, bool timedEncounter, Dictionary<string ,Entity> bossHistory, List<ChatMessage> chatbox);

        private static NetworkController _instance;

        private long _lastTick;
        private MessageFactory _messageFactory;
        public PlayerTracker PlayerTracker;
        public Server Server;

        private NetworkController()
        {
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            var packetAnalysis = new Thread(PacketAnalysisLoop);

            packetAnalysis.Start();
        }

        public Dictionary<string, Entity> BossLink = new Dictionary<string, Entity>();

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
            BasicTeraData.Instance.Servers.Region = server.Region;
            EntityTracker = new EntityTracker(BasicTeraData.Instance.MonsterDatabase);
            PlayerTracker = new PlayerTracker(EntityTracker,BasicTeraData.Instance.Servers);
            AbnormalityTracker.Instance.Renew();
            CharmTracker.Instance.Renew();
            _messageFactory = new MessageFactory(TeraData.OpCodeNamer);
            var handler = Connected;
            handler?.Invoke(server.Name);
        }

        private bool _forceUiUpdate = false;


        public void Reset()
        {
            DamageTracker.Instance.Reset();
            _forceUiUpdate = true;
        }

        public void ResetCurrent()
        {
            DamageTracker.Instance.DeleteEntity(Encounter);
            _forceUiUpdate = true;
        }

        private void UpdateUi()
        {
            _lastTick = Utils.Now();
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
            var teradpsHistory = BossLink.ToDictionary(x=>x.Key, x=>x.Value);
            var chatbox = Chat.Instance.Get();
            handler?.Invoke(firstHit, lastHit, damage, partyDps, entities, stats, currentBossFight , timedEncounter, teradpsHistory, chatbox);
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

        public static void CopyThread(EntityInfo info, List<PlayerInfo> stats, long total, long partyDps, long firstHit, long lastHit, Entity currentBoss, bool timedEncounter, CopyKey copy)
        {

            var text = CopyPaste.Copy(info, stats, total, partyDps, firstHit, lastHit, currentBoss, timedEncounter, copy.Header, copy.Content, copy.Footer, copy.OrderBy, copy.Order);
            CopyPaste.Paste(text);
            
        }

        private void PacketAnalysisLoop()
        {
            while (true)
            {

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

                if(NeedToCopy != null)
                {
                    var stats = DamageTracker.Instance.GetPlayerStats();
                    var currentBoss = Encounter;
                    var info = currentBoss==null?new EntityInfo():DamageTracker.Instance.GetEntityStats()[currentBoss];
                    var timedEncounter = TimedEncounter;
                    var totaldamage = DamageTracker.Instance.TotalDamage(currentBoss, timedEncounter);
                    var firstHit = DamageTracker.Instance.FirstHit(currentBoss);
                    var lastHit = DamageTracker.Instance.LastHit(currentBoss);
                    var partyDps = DamageTracker.Instance.PartyDps(currentBoss, timedEncounter);
                    var tmpcopy = NeedToCopy;
                    var pasteThread = new Thread(() => CopyThread(info, stats, totaldamage, partyDps, firstHit, lastHit, currentBoss , timedEncounter, tmpcopy));
                    pasteThread.Priority = ThreadPriority.Highest;
                    pasteThread.Start();

                    NeedToCopy = null;
                    
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
                    Thread.Sleep(10);
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
                    var amount = skillResultMessage.Amount;
                    if (!skillResultMessage.IsHeal && skillResultMessage.IsHp && amount > 0)
                    {
                        amount *= -1;
                    }
                    var skillResult = ForgeSkillResult(
                        false,
                        amount,
                        skillResultMessage.IsCritical,
                        skillResultMessage.IsHp,
                        skillResultMessage.SkillId,
                        skillResultMessage.Source,
                        skillResultMessage.Target);
                    DamageTracker.Instance.Update(skillResult, skillResultMessage.Time.Ticks);
                    continue;
                }
                var changeHp = message as SCreatureChangeHp;
                if (changeHp != null)
                {
                    AbnormalityTracker.Instance.Update(changeHp);
                    var user=EntityTracker.GetOrPlaceholder(changeHp.TargetId);
                    if (user is UserEntity)
                    {
                        if (changeHp.Slaying)
                        {
                            AbnormalityTracker.Instance.AddAbnormality(changeHp.TargetId, changeHp.TargetId, 0, 0, 8888889, changeHp.Time.Ticks);
                        }
                        else
                        {
                            AbnormalityTracker.Instance.DeleteAbnormality(changeHp);
                        }
                    }
                    continue;
                }

                var pchangeHp = message as SPartyMemberChangeHp;
                if (pchangeHp != null)
                {
                    var user = PlayerTracker.GetOrNull(pchangeHp.PlayerId);
                    if (user == null) continue;//have not seen user yet, cause he is far away, but in party.
                    if (pchangeHp.Slaying)
                    {
                        AbnormalityTracker.Instance.AddAbnormality(user.User.Id, user.User.Id, 0, 0, 8888889, pchangeHp.Time.Ticks);
                    }
                    else
                    {
                        AbnormalityTracker.Instance.DeleteAbnormality(user.User.Id, 8888889, message.Time.Ticks);
                    }
                    continue;
                }

                var changeMp = message as SPlayerChangeMp;
                if (changeMp != null)
                {
                    AbnormalityTracker.Instance.Update(changeMp);
                    continue;
                }

                var NpcStatus = message as SNpcStatus;
                if (NpcStatus != null)
                {
                    DamageTracker.Instance.RegisterAggro(NpcStatus);
                    if (NpcStatus.Enraged)
                    {
                        AbnormalityTracker.Instance.AddAbnormality(NpcStatus.Npc, NpcStatus.Target,0,0,8888888,NpcStatus.Time.Ticks);
                        continue;
                    }
                    else
                    {
                        AbnormalityTracker.Instance.DeleteAbnormality(NpcStatus);
                        continue;
                    }
                }

                var dead = message as SCreatureLife;
                if (dead != null)
                {
                    DamageTracker.Instance.RegisterDead(dead);
                    continue;
                }

                var abnormalityBegin = message as SAbnormalityBegin;
                if (abnormalityBegin != null)
                {
                    AbnormalityTracker.Instance.AddAbnormality(abnormalityBegin);
                    continue;
                }

                var abnormalityEnd = message as SAbnormalityEnd;
                if (abnormalityEnd != null)
                {
                    AbnormalityTracker.Instance.DeleteAbnormality(abnormalityEnd);
                    continue;
                }

                var abnormalityRefresh = message as SAbnormalityRefresh;
                if (abnormalityRefresh != null)
                {
                    AbnormalityTracker.Instance.RefreshAbnormality(abnormalityRefresh);
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
                    DamageTracker.Instance.StopAggro(despawnNpc);
                    AbnormalityTracker.Instance.DeleteAbnormality(despawnNpc);
                    DataExporter.ToTeraDpsApi(despawnNpc);
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
                    CharmTracker.Instance.CharmReset(despawnUser.User, new List<CharmStatus>(), despawnUser.Time.Ticks);
                    AbnormalityTracker.Instance.DeleteAbnormality(despawnUser);
                    continue;
                }

                var charmEnable = message as SEnableCharmStatus;
                if (charmEnable != null)
                {
                    CharmTracker.Instance.CharmEnable(EntityTracker.MeterUser.Id, charmEnable.CharmId,charmEnable.Time.Ticks);
                    continue;
                }
                var pcharmEnable = message as SPartyMemberCharmEnable;
                if (pcharmEnable != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmEnable.PlayerId);
                    if (player == null) continue;
                    CharmTracker.Instance.CharmEnable(player.User.Id, pcharmEnable.CharmId, pcharmEnable.Time.Ticks);
                    continue;
                }
                var charmReset = message as SResetCharmStatus;
                if (charmReset != null)
                {
                    CharmTracker.Instance.CharmReset(charmReset.TargetId, charmReset.Charms, charmReset.Time.Ticks);
                    continue;
                }
                var pcharmReset = message as SPartyMemberCharmReset;
                if (pcharmReset != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmReset.PlayerId);
                    if (player == null) continue;
                    CharmTracker.Instance.CharmReset(player.User.Id, pcharmReset.Charms, pcharmReset.Time.Ticks);
                    continue;
                }
                var charmDel = message as SRemoveCharmStatus;
                if (charmDel != null)
                {
                    CharmTracker.Instance.CharmDel(EntityTracker.MeterUser.Id, charmDel.CharmId, charmDel.Time.Ticks);
                    continue;
                }
                var pcharmDel = message as SPartyMemberCharmDel;
                if (pcharmDel != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmDel.PlayerId);
                    if (player == null) continue;
                    CharmTracker.Instance.CharmDel(player.User.Id, pcharmDel.CharmId, pcharmDel.Time.Ticks);
                    continue;
                }
                var charmAdd = message as SAddCharmStatus;
                if (charmAdd != null)
                {
                    CharmTracker.Instance.CharmAdd(charmAdd.TargetId, charmAdd.CharmId, charmAdd.Status, charmAdd.Time.Ticks);
                    continue;
                }
                var pcharmAdd = message as SPartyMemberCharmAdd;
                if (pcharmAdd != null)
                {
                    var player = PlayerTracker.GetOrNull(pcharmAdd.PlayerId);
                    if (player == null) continue;
                    CharmTracker.Instance.CharmAdd(player.User.Id, pcharmAdd.CharmId, pcharmAdd.Status, pcharmAdd.Time.Ticks);
                    continue;
                }

                EntityTracker.Update(message);
                //var sSpawnUser = message as SpawnUserServerMessage;
                //if (sSpawnUser != null)
                //{
                //    Console.WriteLine(sSpawnUser.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.Id.Id))+" : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.ServerId)) + " " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.PlayerId)));
                //    continue;
                //}

                var spawnMe = message as SpawnMeServerMessage;
                if (spawnMe != null)
                {
                    AbnormalityTracker.Instance.Renew();
                    CharmTracker.Instance.Renew();
                    continue;
                }
                var sLogin = message as LoginServerMessage;
                if (sLogin != null)
                {
                    AbnormalityTracker.Instance.Renew();
                    CharmTracker.Instance.Renew();
                    Connected(BasicTeraData.Instance.Servers.GetServerName(sLogin.ServerId, Server));
                    //Console.WriteLine(sLogin.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sLogin.Id.Id)));
                    continue;
                }
            }
        }

        public void CheckUpdateUi()
        {
            var second = Utils.Now();
            if (second - _lastTick < 1) return;
            UpdateUi();
        }

        public SkillResult ForgeSkillResult(bool abnormality, int amount, bool isCritical, bool isHeal,
            int skillId, EntityId source, EntityId target)
        {
            return new SkillResult(
                abnormality,
                amount,
                isCritical,
                isHeal,
                skillId,
                source,
                target,
                EntityTracker
                );
        }
    }
}
