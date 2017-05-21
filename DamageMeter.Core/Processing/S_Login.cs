using System.Drawing;
using System.Drawing.Text;
using Data;
using Lang;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class S_LOGIN
    {
        internal S_LOGIN(LoginServerMessage message)
        {
            if (NetworkController.Instance.NeedInit)
            {
                NetworkController.Instance.RaiseConnected(BasicTeraData.Instance.Servers.GetServerName(message.ServerId, NetworkController.Instance.Server));
                var trackerreset = true;
                if (NetworkController.Instance.EntityTracker != null)
                {
                    try
                    {
                        var oldregion = BasicTeraData.Instance.Servers.GetServer(NetworkController.Instance.EntityTracker.MeterUser.ServerId).Region;
                        trackerreset = NetworkController.Instance.Server.Region != oldregion;
                    }
                    catch
                    {
                        BasicTeraData.LogError(
                            "New server:" + NetworkController.Instance.Server + ";Old server Id:" + NetworkController.Instance.EntityTracker.MeterUser?.ServerId,
                            false, true);
                        throw;
                    }
                }
                NetworkController.Instance.Server = BasicTeraData.Instance.Servers.GetServer(message.ServerId, NetworkController.Instance.Server);
                NetworkController.Instance.MessageFactory.Region = NetworkController.Instance.Server.Region;
                if (trackerreset)
                {
                    NetworkController.Instance.TeraData = BasicTeraData.Instance.DataForRegion(NetworkController.Instance.Server.Region);
                    BasicTeraData.Instance.HotDotDatabase.Enraged.Name = LP.Enrage;
                    BasicTeraData.Instance.HotDotDatabase.Slaying.Name = LP.Slaying;
                    BasicTeraData.Instance.HotDotDatabase.Slaying.Tooltip = LP.SlayingTooltip;
                    NetworkController.Instance.EntityTracker = new EntityTracker(BasicTeraData.Instance.MonsterDatabase, NetworkController.Instance.UserLogoTracker);
                    NetworkController.Instance.PlayerTracker = new PlayerTracker(NetworkController.Instance.EntityTracker, BasicTeraData.Instance.Servers);
                    NetworkController.Instance.MeterPlayers.Clear();
                    Database.Database.Instance.DeleteAll();
                    SelectFont(NetworkController.Instance.Server.Region);
                }
                NetworkController.Instance.NeedInit = false;
            }
            NetworkController.Instance.AbnormalityStorage.EndAll(message.Time.Ticks);
            NetworkController.Instance.AbnormalityTracker = new AbnormalityTracker(NetworkController.Instance.EntityTracker,
                NetworkController.Instance.PlayerTracker, BasicTeraData.Instance.HotDotDatabase, NetworkController.Instance.AbnormalityStorage,
                DamageTracker.Instance.Update);
            if (NetworkController.Instance.MessageFactory.ChatEnabled)
            {
                NetworkController.Instance.AbnormalityTracker.AbnormalityAdded += NotifyProcessor.Instance.AbnormalityNotifierAdded;
                NetworkController.Instance.AbnormalityTracker.AbnormalityRemoved += NotifyProcessor.Instance.AbnormalityNotifierRemoved;
            }
            NetworkController.Instance.OnGuildIconAction(NetworkController.Instance.UserLogoTracker.GetLogo(message.PlayerId));
            NetworkController.Instance.EntityTracker.Update(message);
            BasicTeraData.Instance.EventsData.Load(NetworkController.Instance.EntityTracker.MeterUser.RaceGenderClass.Class);
            NetworkController.Instance.PacketProcessing.Update();
            NetworkController.Instance.RaisePause(false);
            var me = NetworkController.Instance.PlayerTracker.Me();
            if (!NetworkController.Instance.MeterPlayers.Contains(me)) { NetworkController.Instance.MeterPlayers.Add(me); }
        }

        internal static void SelectFont(string region)
        {
            CopyPaste.PFC = new PrivateFontCollection();
            if (region == "RU") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Fira Sans.ttf"); }
            else if (region == "KR") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\2002L_chat.ttf"); }
            else if (region == "TW") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\DFPHeiMedium-B5.ttf"); }
            else if (region == "JP") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\TT-UDShinGo-Medium.ttf"); }
            else { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Frutiger LT Pro 55 Roman.ttf"); }
            CopyPaste.Font = new Font(CopyPaste.PFC.Families[0], 12, FontStyle.Regular, GraphicsUnit.Pixel);
        }
    }
}