using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DamageMeter.Sniffing;
using Data;
using Tera.Game;
using Tera.Game.Messages;
using Application = System.Windows.Forms.Application;
using Message = Tera.Message;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DamageMeter
{
    public class NetworkController
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void UpdateUiHandler(
            long interval, long totaldamage, LinkedList<Entity> entities, List<PlayerInfo> stats);

        private static NetworkController _instance;
        private EntityTracker _entityTracker;

        private long _lastTick;
        private MessageFactory _messageFactory;
        private PlayerTracker _playerTracker;

        public void Exit()
        {
            BasicTeraData.Instance.WindowData.Save();
            BasicTeraData.Instance.HotkeysData.Save();
            TeraSniffer.Instance.Enabled = false;
            Application.Exit();
        }

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
            _entityTracker = new EntityTracker();
            _playerTracker = new PlayerTracker(_entityTracker);
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

        private void UpdateUi()
        {
            _lastTick = DateTime.UtcNow.Ticks/10000000;
            var handler = TickUpdated;
            var damage = DamageTracker.Instance.TotalDamage;
            var stats =
                DamageTracker.Instance.GetStats().OrderByDescending(playerStats => playerStats.Dealt.Damage).ToList();
            var interval = DamageTracker.Instance.Interval;
            var entities = new LinkedList<Entity>(DamageTracker.Instance.Entities);
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
                    Thread.Sleep(20);
                    continue;
                }

                if (TeraSniffer.Instance.Packets.Count > 3000)
                {
                    MessageBox.Show(
                        "Your computer is too slow to use this DPS meter. Can't analyse all those packet in decent amount of time. Shutting down now.");
                    Exit();
                }

                var message = _messageFactory.Create(obj);
                _entityTracker.Update(message);
                var npcOccupier = message as SNpcOccupierInfo;
                if (npcOccupier != null)
                {
                    DamageTracker.Instance.UpdateEntities(new NpcOccupierResult(npcOccupier));
                }
                var skillResultMessage = message as EachSkillResultServerMessage;
                if (skillResultMessage == null) continue;
                var skillResult = new SkillResult(skillResultMessage, _entityTracker, _playerTracker);
                DamageTracker.Instance.Update(skillResult);

                var second = DateTime.UtcNow.Ticks/10000000;
                if (second - _lastTick < 1) continue;
                UpdateUi();
            }
        }

        private delegate void ForceUpdateUi();
    }
}