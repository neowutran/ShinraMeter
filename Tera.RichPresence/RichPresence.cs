using System;
using System.Diagnostics;
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


        private string DefaultImage => _me.FullName == "Killian : Roukanken" ? "roukanken_default" : "tera_default";

        private DiscordRPC.RichPresence InGamePresence => new DiscordRPC.RichPresence
        {
            Details = "TODO",
            State = "TDDO",
            Party =
            {
                Size = 1,
                Max = 1,
            },
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
            // Deinitialize();
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


        public void Invoke()
        {
            Client.Invoke();
        }

        public void Initialize()
        {
            _location = null;
            _me = null;
            _isIngame = false;
            // InitClient();
        }
        
        public void Deinitialize()
        {
            Client.Dispose();
        }
    }
}