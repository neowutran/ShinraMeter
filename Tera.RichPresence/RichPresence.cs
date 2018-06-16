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

        public static RichPresence Instance => _instance ?? (_instance = new RichPresence());

        private Player _me;
        private Location _location;

        // private string LargeImageKey => _location == null ? "tera_default" : BasicTeraData.Instance.MapData.GetImageName(_location);
        private string LargeImageKey =>  _me.FullName == "Killian : Roukanken" ? "roukanken_default" : "tera_default";
        private string LargeImageText => _location == null ? null : BasicTeraData.Instance.MapData.GetFullName(_location);
        
        
        private string SmallImageKey => "class_" + _me.RaceGenderClass.Class.ToString().ToLower();
        private string SmallImageText => $"Lvl {_me.Level} {_me.Name} ({_me.Server})";
        
        public void Login(Player me)
        {
            _me = me;
            UpdatePresence();
        }

        private DiscordRPC.RichPresence Presence => new DiscordRPC.RichPresence
        {
            State = LargeImageText,
            Assets = new Assets {LargeImageKey = LargeImageKey, LargeImageText = LargeImageText, SmallImageKey = SmallImageKey, SmallImageText = SmallImageText}
        };

        private void UpdatePresence(DiscordRPC.RichPresence presence = null)
        {
            presence = presence ?? Presence;
            var request = DiscordRPC.Web.WebRPC.PrepareRequest(presence, ClientId);

            using (var web = new System.Net.WebClient())
            {
                foreach (var header in request.Headers) { web.Headers.Add(header.Key, header.Value); }
                var json = web.UploadString(request.URL, request.Data);
            }
        }

        public void VisitNewSection(S_VISIT_NEW_SECTION message)
        {
            _location = new Location(message.MapId, message.GuardId, message.SectionId, 0, 0);
            UpdatePresence();
        }

        public void ReturnToLobby()
        {
            _location = null;
            _me = null;
            UpdatePresence(new DiscordRPC.RichPresence());
        }
    }
}