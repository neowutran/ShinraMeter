using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Documents;
using DamageMeter;
using Data;
using DiscordRPC;
using DiscordRPC.Message;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.RichPresence
{
    enum State
    {
       Idle, Lfg, Fight, Matching
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
        private DiscordRpcClient Client => _client ?? InitClient();
        
        public static RichPresence Instance => _instance ?? (_instance = new RichPresence());

        private Player _me;
        private Location _location;
        private bool _isIngame = false;
        private Server _server;

        private List<NpcEntity> _bosses = new List<NpcEntity>();
        private Dictionary<EntityId, long> _bossHps = new Dictionary<EntityId, long>();

        private DateTime? _fightStarted;

        private string _lfgMessage = null;
        private DateTime? _lfgStarted = null;
        private Party _lastParty;
        
        private MatchingType _matchingType;
        private DateTime? _matchingStarted;

        private string DefaultImage => _me.FullName == "Killian : Roukanken" ? "roukanken_default" : "tera_default";
        
        private State State => getState();

        
        private string Details => 
            State == State.Idle ? "Idle" :
            State == State.Fight ? $"Fight | {GetFightName()} {FightHp()}" :
            State == State.Lfg ? $"LFG | {_lfgMessage}" :
            State == State.Matching ? $"In {_matchingType.ToString().ToLower()} matching" : null;

        private Timestamps Timestamps =>
            State == State.Fight ? new Timestamps {Start = _fightStarted} : 
            State == State.Lfg ? new Timestamps {Start = _lfgStarted} : 
            State == State.Matching ? new Timestamps {Start = _matchingStarted} : null;


        private int PartySize => PacketProcessor.Instance.PlayerTracker.PartyList().Count;
        private PartyType PartyType => 
            PartySize <= 1 ? PartyType.Solo :
            IsRaid ? PartyType.Raid : PartyType.Party;
        
        private string PartyStatus => 
            PartyType == PartyType.Solo ? "Solo" :
            PartyType == PartyType.Party ? "In party" :
            PartyType == PartyType.Raid ? "In raid": null;

        private string AdditionalStatus => _matchingStarted == null ? null :
            _matchingType == MatchingType.Dungeon ? " | IMS" : " | BG Queue";
        
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
            State = $"{PartyStatus}{AdditionalStatus}",
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
            if (_matchingStarted != null) return State.Matching;
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

        private float GetFightHpPercent()
        {
            long totalMax = 1;
            long totalNow = 0;
            
            foreach (var boss in _bosses)
            {
                var max = boss.Info.HP;
                _bossHps.TryGetValue(boss.Id, out var now);

                if (now < max && now > 0)
                {
                    totalMax += max;
                    totalNow += now;
                }
            }
            
            return (float) totalNow / totalMax;
        }

        private string FightHp()
        {
            float hp = GetFightHpPercent();
            if (hp < 0) return null;

            return $"| {hp*100:n0}%";
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
            _client = null;
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
        
        // TODO: add LFG link packet processing for more precision (?)
        public void HandleLfg(S_SHOW_PARTY_MATCH_INFO sShowPartyMatchInfo)
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

        public void HandleLfg(C_REGISTER_PARTY_INFO cRegisterPartyInfo)
        {
            _lfgMessage = cRegisterPartyInfo.Message;
            _lfgStarted = _lfgStarted ?? DateTime.UtcNow;
            UpdatePresence();
        }

        public void HandleLfg(S_MY_PARTY_MATCH_INFO sShowPartyMatchInfo)
        {
            _lfgMessage = sShowPartyMatchInfo.Message;
            _lfgStarted = _lfgStarted ?? DateTime.UtcNow;
            UpdatePresence();
        }
        
       
        public void HandleBossHp(S_BOSS_GAGE_INFO message)
        {
            var bossHp = 0L;

            if (message.HpRemaining < message.TotalHp)
            {
                bossHp = message.HpRemaining;
            }

            _bossHps[message.EntityId] = bossHp;
            UpdatePresence();
        }

        public void HandleBossHp(SCreatureChangeHp message)
        {
            var bossHp = 0L;

            if (message.HpRemaining < message.TotalHp)
            {
                bossHp = message.HpRemaining;
            }

            _bossHps[message.TargetId] = bossHp;
            UpdatePresence();
        }

        public void HandleIms(S_CHANGE_EVENT_MATCHING_STATE message)
        {
            if (message.Searching)
            {
                _matchingStarted = _matchingStarted ?? DateTime.UtcNow;
                _matchingType = message.Type;
            }
            else { _matchingStarted = null; }
            
            UpdatePresence();
        }
    }
}