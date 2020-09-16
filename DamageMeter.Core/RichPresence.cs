using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter;
using Data;
using DiscordRPC;
using DiscordRPC.Logging;
using Lang;
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
        
        private MatchingType? _matchingType;
        private DateTime? _matchingStarted;

        // ;)
        private string DefaultImage => "tera_default";
        
        private State State => getState();

        private bool ShowCharacter => BasicTeraData.Instance.WindowData.RichPresenceShowCharacter;
        private bool ShowLocation => BasicTeraData.Instance.WindowData.RichPresenceShowLocation;
        private bool ShowStatus => BasicTeraData.Instance.WindowData.RichPresenceShowStatus;
        private bool ShowParty => BasicTeraData.Instance.WindowData.RichPresenceShowParty;
        
        
        private string Details => 
            !ShowStatus ? LP.RpStatusPlaying:
            State == State.Idle ? LP.RpStatusIdle :
            State == State.Fight ? $"{LP.RpStatusFight} | {FightHp()}{GetFightName()} " :
            State == State.Lfg ? $"{LP.RpStatusLfg} | {_lfgMessage}" :
            State == State.Matching ? 
                (_matchingType == MatchingType.Dungeon ? LP.RpStatusDungeonMatch: 
                _matchingType == MatchingType.Battleground ? LP.RpStatusBattlegroundMatch : null)
            : null;

        private Timestamps Timestamps =>
            !ShowStatus ? null :
            State == State.Fight ? new Timestamps {Start = _fightStarted} : 
            State == State.Lfg ? new Timestamps {Start = _lfgStarted} : 
            State == State.Matching ? new Timestamps {Start = _matchingStarted} : null;


        private int PartySize => PacketProcessor.Instance.PlayerTracker.PartySize;
        private PartyType PartyType => 
            PartySize <= 1 ? PartyType.Solo :
            IsRaid ? PartyType.Raid : PartyType.Party;
        
        private string PartyStatus => 
            !ShowParty ? null :
            PartyType == PartyType.Solo ? LP.RpPartySolo :
            PartyType == PartyType.Party ? LP.RpPartyParty :
            PartyType == PartyType.Raid ? LP.RpPartyRaid : null;

        private string AdditionalStatus => 
            !ShowStatus ? null : 
            _matchingStarted == null ? null :
            _matchingType == MatchingType.Dungeon ? $" | {LP.RpPartyQueue}" : $" | {LP.RpPartyBg}";
        
        private bool IsRaid => PacketProcessor.Instance.PlayerTracker.IsRaid;
        private Party Party => 
            !ShowParty ? null : 
            PartySize <= 1 ? null: new Party
            {
                ID = "none",        // crashes otherwise
                Size = PartySize,
                Max = IsRaid ? 30 : 5
            };
        
        private DiscordRPC.RichPresence InGamePresence => new DiscordRPC.RichPresence
        {
            Details = Details.LimitUtf8ByteCount(128),
            State = $"{PartyStatus}{AdditionalStatus}",
            Timestamps = Timestamps,
            Party = Party,
            Assets = new Assets
            {
                LargeImageKey = (ShowLocation ? BasicTeraData.Instance.MapData.GetImageName(_location) : null) ?? DefaultImage,
                LargeImageText = _location == null || !ShowLocation ? null : BasicTeraData.Instance.MapData.GetFullName(_location) + " (" + PacketProcessor.Instance.Server.Region + ")",
                SmallImageKey = ShowCharacter  && _me != null ? $"class_{_me.RaceGenderClass.Class.ToString().ToLower()}" : null, 
                SmallImageText = ShowCharacter && _me != null ? $"{LP.RpLevel} {_me.Level} {_me.Name} ({_me.Server})" : null,
            }
        
        };

        private DiscordRPC.RichPresence CharacterSelectPresence => new DiscordRPC.RichPresence
        {
            Details = ShowStatus ? LP.RpStatusCharSelect : LP.RpStatusPlaying,
            State = ShowCharacter ? $"{_server.Name}" : null,
            Assets = new Assets
            {
                LargeImageKey = DefaultImage,
            }
        };
        
        private DiscordRPC.RichPresence Presence => 
            _isIngame ? InGamePresence : 
            _server != null ? CharacterSelectPresence : null;

        
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
            if (_bosses.All(boss => boss.Info.Name == firstName)) return firstName;
            
            return LP.RpMultipleEnemies;
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

            return $"{hp*100:n0}% | ";
        }
        
        private DiscordRpcClient InitClient()
        {
            try {
                var logger = new ShinraLogger { Level = LogLevel.Error };
                _client = new DiscordRpcClient(ClientId, -1, logger);
                _client.Initialize();
            }
            catch (Exception e){ BasicTeraData.LogError("Discord RPC Init fail: "+e.Message, false, true);}

            return _client;
        }
        
        private void UpdatePresence(DiscordRPC.RichPresence presence = null)
        {
            if (!BasicTeraData.Instance.WindowData.EnableRichPresence || !BasicTeraData.Instance.WindowData.EnableChat) 
                return;
            
            presence = presence ?? Presence;
            try { Client?.SetPresence(presence); }
            catch (Exception e) { BasicTeraData.LogError("Discord RPC set presence fail: " + e.Message, false, true); }
        }

        public void Invoke()
        {
            try { _client?.Invoke(); }
            catch (Exception e) { BasicTeraData.LogError("Discord RPC invoke error: " + e.Message, false, true); }
        }

        public void Initialize()
        {
            _location = null;
            
            _bosses = new List<NpcEntity>();
            _bossHps = new Dictionary<EntityId, long>();
            _fightStarted = null;

            _lfgMessage = null;
            _lfgStarted = null;

            _matchingStarted = null;
            _matchingType = null;
        }
        
        public void Deinitialize()
        {
            if (_client == null) { return; }

            _client.Dispose();
            _client = null;
        }

        public void Login(Player me)
        {
            _me = me;
            _isIngame = true;
            _server = PacketProcessor.Instance.Server;
            PacketProcessor.Instance.PlayerTracker.PartyChangedEvent += HandlePartyChanged;
            UpdatePresence();
        }
        
        public void HandleConnected(Server server)
        {
            Initialize();
            _me = null;
            _isIngame = false;
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
            _me = null;
            _isIngame = false;
            UpdatePresence();
        }


        public void DespawnNpc(SDespawnNpc message)
        {
            NpcEntity boss = _bosses.Find(entity => entity.Id == message.NPC);
            if (boss == null) return;
            
            _bosses.Remove(boss);
            _bossHps.Remove(boss.Id);
            if (_bosses.Count == 0) _fightStarted = null;
            
            UpdatePresence();
        }
        
        public void S_LOAD_TOPO(S_LOAD_TOPO message)
        {
            Initialize();
            UpdatePresence();
        }

        public void HadleNpcOccupierInfo(SNpcOccupierInfo message)
        {
            if (!message.HasReset) { return; }

            _bosses.RemoveAll(boss => boss.Id == message.NPC);
            _bossHps.Remove(message.NPC);

            if (_bosses.Count == 0) { _fightStarted = null; }
            
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

        private void HandlePartyChanged()
        {
            // if guy left pt or pt is now full
            if ((_lastParty != null && Party == null) || (Party != null && Party.Size == Party.Max))
            {
                _lfgMessage = null;
                _lfgStarted = null;
            }
            
            _lastParty = Party;
            UpdatePresence();
        }
        
        // TODO: add LFG link packet processing for more precision
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

        public void Update()
        {
            UpdatePresence();
        }
    }
    public class ShinraLogger : DiscordRPC.Logging.ILogger
    {
        public LogLevel Level { get; set; }
        public void Trace(string message, params object[] args)
        {
            //Null Logger, so no messages are acutally sent
        }
        public void Info(string message, params object[] args)
        {
            //Null Logger, so no messages are acutally sent
        }
        public void Warning(string message, params object[] args)
        {
            //Null Logger, so no messages are acutally sent 
        }
        public void Error(string message, params object[] args)
        {
            if (message != "Failed connection to {0}. {1}" && message != "Failed to connect for some reason.")
                BasicTeraData.LogError("DiscordRPC logged error:\r\n"+ message, true, true);
        }
    }
}