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
    enum State
    {
       Idle, Lfg, Fight 
    }

    enum PartyType
    {
        Solo, Party, Raid
    }
    
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

        private string _lfgMessage = null;
        private DateTime? _lfgStarted = null;
        private Party _lastParty;

        private string DefaultImage => _me.FullName == "Killian : Roukanken" ? "roukanken_default" : "tera_default";
        
        private State State => getState();
        
        private string Details => 
            State == State.Idle ? "Idle" :
            State == State.Fight ? $"Fighting {GetFightName()}" :
            State == State.Lfg ? $"LFG | {_lfgMessage}" : null;

        private Timestamps Timestamps =>
            State == State.Fight ? new Timestamps {Start = _fightStarted} : 
            State == State.Lfg ? new Timestamps { Start = _lfgStarted} : null;


        private int PartySize => PacketProcessor.Instance.PlayerTracker.PartyList().Count;
        private PartyType PartyType => 
            PartySize <= 1 ? PartyType.Party :
            IsRaid ? PartyType.Raid : PartyType.Party;
        
        private string PartyStatus => 
            PartyType == PartyType.Solo ? "Solo" :
            PartyType == PartyType.Party ? "In party" :
            PartyType == PartyType.Raid ? "In raid": null;
        
        
        private bool IsRaid => PacketProcessor.Instance.PlayerTracker.IsRaid;
        private Party Party => PartySize <= 1 ? null: new Party
        {
            ID = "none",
            Size = PartySize,
            Max = IsRaid ? 30 : 5
        };
        
        private DiscordRPC.RichPresence InGamePresence => new DiscordRPC.RichPresence
        {
            Details = Details,
            State = PartyStatus,
            Timestamps = Timestamps,
            Party = Party,
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

        
        private State getState()
        {
            if (_fightStarted != null) return State.Fight;
            if (_lfgStarted != null) return State.Lfg;
            
            return State.Idle;
        }
        
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
            
            _bosses = new List<NpcEntity>();
            _fightStarted = null;

            _lfgMessage = null;
            _lfgStarted = null;
        }
        
        public void Deinitialize()
        {
            Client.Dispose();
        }

        public void Login(Player me)
        {
            _me = me;
            _isIngame = true;
            PacketProcessor.Instance.PlayerTracker.PartyChangedEvent += HandlePartyChanged;
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

        public void HandleUserIdle()
        {
            _bosses = new List<NpcEntity>();
            _fightStarted = null;
            
            UpdatePresence();
        }

        private void HandlePartyChanged()
        {
            // if guy left pt or pt is now full
            if ((_lastParty != null && Party == null) || Party.Size == Party.Max)
            {
                _lfgMessage = null;
                _lfgStarted = null;
            }
            
            _lastParty = Party;
            UpdatePresence();
        }
        
        public void HandleShowLfg(S_SHOW_PARTY_MATCH_INFO sShowPartyMatchInfo)
        {
            try
            {
                var lfg = sShowPartyMatchInfo.Listings.First(listing => PacketProcessor.Instance.PlayerTracker.MyParty(_me.ServerId, listing.LeaderId));
                _lfgMessage = lfg.Message;
                _lfgStarted = _lfgStarted ?? DateTime.UtcNow;
            }
            catch (InvalidOperationException)
            {
                _lfgMessage = null;
                _lfgStarted = null;
            }
            UpdatePresence();
        }

        public void HandlePostLfg(C_REGISTER_PARTY_INFO cRegisterPartyInfo)
        {
            _lfgMessage = cRegisterPartyInfo.Message;
            _lfgStarted = _lfgStarted ?? DateTime.UtcNow;
            UpdatePresence();
        }
    }
}