using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows.Documents;
using DamageMeter;
using Data;
using DiscordRPC;
using DiscordRPC.Message;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.RichPresence
{
    public class RichPresence
    {
        private const string ClientId = "448196693964488715";
        private static RichPresence _instance;

        private DiscordRpcClient _client = null;
        public DiscordRpcClient Client => _client ?? InitClient();
        
        public static RichPresence Instance => _instance ?? (_instance = new RichPresence());

        private Player _me;
        private Location _location;
        private bool _isIngame = false;
        private Server _server;

        private List<NpcEntity> _bosses = new List<NpcEntity>();
        private DateTime? _fightStarted;
        

        private string DefaultImage => _me.FullName == "Killian : Roukanken" ? "roukanken_default" : "tera_default";
        private Timestamps Timestamps => _bosses.Count > 0 ? new Timestamps{Start = _fightStarted} : null;
        
        
        private DiscordRPC.RichPresence InGamePresence => new DiscordRPC.RichPresence
        {
            State = GetFightName() != null ? $"Fighting {GetFightName()}" : null,
            Timestamps = Timestamps,
            Assets = new Assets
            {
                LargeImageKey = BasicTeraData.Instance.MapData.GetImageName(_location) ?? DefaultImage,
                LargeImageText = _location == null ? null : BasicTeraData.Instance.MapData.GetFullName(_location),
                SmallImageKey = "class_" + _me.RaceGenderClass.Class.ToString().ToLower(), 
                SmallImageText = $"Lvl {_me.Level} {_me.Name} ({_me.Server})"
            }

        };

        private DiscordRPC.RichPresence CharacterSelectPresence => new DiscordRPC.RichPresence
        {
            Details = "Character selection",
            State = $"{_server.Name}",
            Assets = new Assets
            {
                LargeImageKey = "tera_default",
            }
        };
        
        private DiscordRPC.RichPresence Presence => _isIngame ? InGamePresence : CharacterSelectPresence;

        private string GetFightName()
        {
            if (_bosses.Count == 0) return null;
            
            string firstName = _bosses[0].Info.Name;
            if (_bosses.Count == 1) return firstName;

            if (_bosses.All(boss => boss.Info.Name == firstName)) return firstName + "s";
            return "multiple BAMs";
        }
        
        private DiscordRpcClient InitClient()
        {
            _client = new DiscordRpcClient(ClientId, true, -1);
            _client.Initialize();

            return _client;
        }
        
        private void UpdatePresence(DiscordRPC.RichPresence presence = null)
        {
            presence = presence ?? Presence;
            Client.SetPresence(presence);
        }

        public void Invoke()
        {
            Client.Invoke();
        }

        public void Initialize()
        {
            _location = null;
            _me = null;
            _isIngame = false;
            _server = null;
            
            _bosses = new List<NpcEntity>();
            _fightStarted = null;
        }
        
        public void Deinitialize()
        {
            Client.Dispose();
        }

        public void Login(Player me)
        {
            _me = me;
            _isIngame = true;
            UpdatePresence();
        }
        
        public void HandleConnected(Server server)
        {
            Initialize();
            _server = server;
            UpdatePresence();
        }
        
        public void HandleEndConnection()
        {
            UpdatePresence(new DiscordRPC.RichPresence());
            Deinitialize();
        }
        
        public void VisitNewSection(S_VISIT_NEW_SECTION message)
        {
            _location = new Location(message.MapId, message.GuardId, message.SectionId, 0, 0);
            UpdatePresence();
        }

        public void ReturnToLobby()
        {
            Initialize();
            UpdatePresence();
        }


        public void DespawnNpc(SDespawnNpc message)
        {
            NpcEntity boss = _bosses.Find(entity => entity.Id == message.NPC);
            if (boss == null) return;
            
            _bosses.Remove(boss);
            if (_bosses.Count == 0) _fightStarted = null;
            
            UpdatePresence();
        }

        public void EachSkillResult(SkillResult skillResult)
        {
            if (!PacketProcessor.Instance.PlayerTracker.MyParty(skillResult.SourcePlayer)) return;
            if (skillResult.Target is NpcEntity entity)
            {
                if (!entity.Info.Boss) return;
                if (_bosses.Contains(entity)) return;
                
                _bosses.Add(entity);
                _fightStarted = _fightStarted ?? DateTime.UtcNow;
                UpdatePresence();
            }
            
        }

        public void UserIdle()
        {
            _bosses = new List<NpcEntity>();
            _fightStarted = null;
            
            UpdatePresence();
        }
    }
}