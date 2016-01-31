using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
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

        public delegate void UpdateUiHandler(
            long interval, long totaldamage, Dictionary<Entity, EntityInfo> entities, List<PlayerInfo> stats);

        private static NetworkController _instance;

        private long _lastTick;
        private MessageFactory _messageFactory;
        private PlayerTracker _playerTracker;

        private NetworkController()
        {
            //TeraSniffer.Instance.MessageReceived += HandleMessageReceived;
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            var packetAnalysis = new Thread(PacketAnalysisLoop);
            packetAnalysis.Start();
        }

        public TeraData TeraData { get; private set; }

        public Entity Encounter { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public IPEndPoint ServerIpEndPoint { get; private set; }
        public IPEndPoint ClientIpEndPoint { get; private set; }


        public EntityTracker EntityTracker { get; private set; }

        public void Exit()
        {
            BasicTeraData.Instance.WindowData.Save();
            BasicTeraData.Instance.HotkeysData.Save();
            TeraSniffer.Instance.Enabled = false;
            Application.Exit();
        }

        public event ConnectedHandler Connected;
        public event UpdateUiHandler TickUpdated;

        public void ForceUpdate()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            ForceUpdateUi changeUi = UpdateUi;
            dispatcher.Invoke(changeUi);
        }

        private void HandleNewConnection(Server server, IPEndPoint serverIpEndPoint, IPEndPoint clientIpEndPoint)
        {
            TeraData = BasicTeraData.Instance.DataForRegion(server.Region);
            EntityTracker = new EntityTracker();
            _playerTracker = new PlayerTracker(EntityTracker);
            _messageFactory = new MessageFactory(TeraData.OpCodeNamer);
            ServerIpEndPoint = serverIpEndPoint;
            ClientIpEndPoint = clientIpEndPoint;
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
            var interval = DamageTracker.Instance.Interval;
            var entities =
                DamageTracker.Instance.GetEntityStats();
            handler?.Invoke(interval, damage, entities, stats);
        }


        private void PacketAnalysisLoop()
        {
            while (true)
            {
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
                AbnormalityTracker.Instance.ApplyEnduranceDebuff(message.Time.Ticks);

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
                }

                var changeMp = message as SPlayerChangeMp;
                if (changeMp != null)
                {
                    AbnormalityTracker.Instance.Update(changeMp);
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
                CheckUpdateUi();
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
                EntityTracker,
                _playerTracker
                );
        }

        private delegate void ForceUpdateUi();
    }
}