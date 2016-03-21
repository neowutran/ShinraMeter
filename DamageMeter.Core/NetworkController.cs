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
            long firsthit, long lastHit, long totaldamage, Dictionary<Entity, EntityInfo> entities,
            List<PlayerInfo> stats, Entity currentBoss);

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

        public TeraData TeraData { get; private set; }

        public Entity Encounter { get; set; }

        public bool TimedEncounter { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public EntityTracker EntityTracker { get; private set; }

        public bool ForceUpdate { get; set; }

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
            _messageFactory = new MessageFactory(TeraData.OpCodeNamer);
            var handler = Connected;
            handler?.Invoke(server.Name);
        }


        public void Reset()
        {
            DamageTracker.Instance.Reset();
            UpdateUi();
        }

        public void ResetCurrent()
        {
            DamageTracker.Instance.DeleteEntity(Encounter);
            UpdateUi();
        }

        private void UpdateUi()
        {
            _lastTick = Utils.Now();
            var handler = TickUpdated;
            var damage = DamageTracker.Instance.TotalDamage;
            var stats =
                DamageTracker.Instance.GetPlayerStats()
                    .OrderByDescending(playerStats => playerStats.Dealt.Damage)
                    .ToList();
            var firstHit = DamageTracker.Instance.FirstHit;
            var lastHit = DamageTracker.Instance.LastHit;
            var entities =
                DamageTracker.Instance.GetEntityStats();
            var currentBossFight = DamageTracker.Instance.CurrentBoss;
            handler?.Invoke(firstHit, lastHit, damage, entities, stats, currentBossFight);
            DamageTracker.Instance.CurrentBoss = null;
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

        public static void CopyThread(List<PlayerInfo> stats, long total, long interval, CopyKey copy)
        {
                   
            CopyPaste.Copy(stats, total, interval, copy.Header, copy.Content, copy.Footer, copy.OrderBy, copy.Order);
            var text = Clipboard.GetText();
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
                    var totaldamage = DamageTracker.Instance.TotalDamage;
                    var interval = DamageTracker.Instance.Interval;
                    var tmpcopy = NeedToCopy;
                    var pasteThread = new Thread(() => CopyThread(stats, totaldamage, interval, tmpcopy));
                    pasteThread.SetApartmentState(ApartmentState.STA);
                    pasteThread.Start();
                    NeedToCopy = null;
                    
                }

                if (ForceUpdate)
                {
                    ForceUpdate = false;
                    UpdateUi();
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
                EntityTracker.Update(message);

                //var sSpawnUser = message as SpawnUserServerMessage;
                //if (sSpawnUser != null)
                //{
                //    Console.WriteLine(sSpawnUser.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.Id.Id))+" : " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.ServerId)) + " " + BitConverter.ToString(BitConverter.GetBytes(sSpawnUser.PlayerId)));
                //    continue;
                //}

                var sLogin = message as LoginServerMessage;
                if (sLogin != null)
                {
                    Connected(BasicTeraData.Instance.Servers.GetServerName(sLogin.ServerId,Server));
                    //Console.WriteLine(sLogin.Name + " : " + BitConverter.ToString(BitConverter.GetBytes(sLogin.Id.Id)));
                    continue;
                }

                var npcOccupier = message as SNpcOccupierInfo;
                if (npcOccupier != null)
                {
                    DamageTracker.Instance.UpdateEntities(new NpcOccupierResult(npcOccupier), npcOccupier.Time.Ticks);
                    continue;
                }


                var changeHp = message as SCreatureChangeHp;
                if (changeHp != null)
                {
                    AbnormalityTracker.Instance.Update(changeHp);
                    continue;
                }

                var changeMp = message as SPlayerChangeMp;
                if (changeMp != null)
                {
                    AbnormalityTracker.Instance.Update(changeMp);
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

                var despawnNpc = message as SDespawnNpc;
                if (despawnNpc != null)
                {
                    AbnormalityTracker.Instance.DeleteAbnormality(despawnNpc);
                    if (despawnNpc.Dead)
                    {
                        var entity = DamageTracker.Instance.GetEntity(despawnNpc.Npc);
                        Console.WriteLine(entity.Name + " is dead.");
                        //TODO: call teradps.io API
                    }
                    continue;
                }

                var despawnUser = message as SDespawnUser;
                if (despawnUser != null)
                {
                    AbnormalityTracker.Instance.DeleteAbnormality(despawnUser);
                    continue;
                }

                var skillResultMessage = message as EachSkillResultServerMessage;
                if (skillResultMessage == null) continue;

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