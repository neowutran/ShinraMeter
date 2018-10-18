using System.Drawing;
using System.Drawing.Text;
using Data;
using Lang;
using Tera.Game;
using Tera.Game.Messages;
using Tera.RichPresence;

namespace DamageMeter.Processing
{
    internal class S_LOGIN
    {
        internal S_LOGIN(LoginServerMessage message)
        {
            if (PacketProcessor.Instance.NeedInit)
            {
                PacketProcessor.Instance.RaiseConnected(BasicTeraData.Instance.Servers.GetServerName(message.ServerId, PacketProcessor.Instance.Server));
                PacketProcessor.Instance.Server = BasicTeraData.Instance.Servers.GetServer(message.ServerId, PacketProcessor.Instance.Server);
                PacketProcessor.Instance.MessageFactory.Region = PacketProcessor.Instance.Server.Region;
                var trackerreset = true;
                if (PacketProcessor.Instance.EntityTracker != null)
                {
                    try
                    {
                        var oldregion = BasicTeraData.Instance.Servers.GetServer(PacketProcessor.Instance.EntityTracker.MeterUser.ServerId).Region;
                        trackerreset = PacketProcessor.Instance.Server.Region != oldregion;
                    }
                    catch
                    {
                        BasicTeraData.LogError(
                            "New server:" + PacketProcessor.Instance.Server + ";Old server Id:" + PacketProcessor.Instance.EntityTracker.MeterUser?.ServerId,
                            false, true);
                        throw;
                    }
                }
                if (trackerreset)
                {
                    PacketProcessor.Instance.TeraData = BasicTeraData.Instance.DataForRegion(PacketProcessor.Instance.Server.Region);
                    BasicTeraData.Instance.HotDotDatabase.Enraged.Name = LP.Enrage;
                    BasicTeraData.Instance.HotDotDatabase.Slaying.Name = LP.Slaying;
                    BasicTeraData.Instance.HotDotDatabase.Slaying.Tooltip = LP.SlayingTooltip;
                    PacketProcessor.Instance.EntityTracker = new EntityTracker(BasicTeraData.Instance.MonsterDatabase, PacketProcessor.Instance.UserLogoTracker);
                    PacketProcessor.Instance.PlayerTracker = new PlayerTracker(PacketProcessor.Instance.EntityTracker, BasicTeraData.Instance.Servers);
                    PacketProcessor.Instance.MeterPlayers.Clear();
                    Database.Database.Instance.DeleteAll();
                    SelectFont(PacketProcessor.Instance.Server.Region);
                }
                PacketProcessor.Instance.NeedInit = false;
            }
            PacketProcessor.Instance.AbnormalityStorage.EndAll(message.Time.Ticks);
            PacketProcessor.Instance.AbnormalityTracker = new AbnormalityTracker(PacketProcessor.Instance.EntityTracker,
                PacketProcessor.Instance.PlayerTracker, BasicTeraData.Instance.HotDotDatabase, PacketProcessor.Instance.AbnormalityStorage,
                DamageTracker.Instance.Update);
            if (PacketProcessor.Instance.MessageFactory.ChatEnabled)
            {
                PacketProcessor.Instance.AbnormalityTracker.AbnormalityAdded += NotifyProcessor.Instance.AbnormalityNotifierAdded;
                PacketProcessor.Instance.AbnormalityTracker.AbnormalityRemoved += NotifyProcessor.Instance.AbnormalityNotifierRemoved;
            }
            PacketProcessor.Instance.OnGuildIconAction(PacketProcessor.Instance.UserLogoTracker.GetLogo(message.PlayerId));
            PacketProcessor.Instance.EntityTracker.Update(message);
            PacketProcessor.Instance.PlayerTracker.UpdateParty(message);
            BasicTeraData.Instance.EventsData.Load(PacketProcessor.Instance.EntityTracker.MeterUser.RaceGenderClass.Class);
            PacketProcessor.Instance.PacketProcessing.Update();
            PacketProcessor.Instance.RaisePause(false);
            var me = PacketProcessor.Instance.PlayerTracker.Me();
            if (!PacketProcessor.Instance.MeterPlayers.Contains(me)) { PacketProcessor.Instance.MeterPlayers.Add(me); }

            RichPresence.Instance.Login(me);
        }

        internal static void SelectFont(string region)
        {
            CopyPaste.PFC = new PrivateFontCollection();
            if (region == "KR"||region == "KR-PTS") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Noto Sans CJK KR Bold.ttf"); }
            else if (region == "TW") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Noto Sans CJK TC Bold.ttf"); }
            else if (region == "THA") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Arial Unicode MS.ttf"); }
            else if (region == "JP"||region == "JP-C") { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Noto Sans CJK JP Bold.ttf"); }
            else { CopyPaste.PFC.AddFontFile(BasicTeraData.Instance.ResourceDirectory + "data\\fonts\\Noto Sans.ttf"); }
            CopyPaste.Font = new Font(CopyPaste.PFC.Families[0], 12, FontStyle.Bold, GraphicsUnit.Pixel);
        }
    }
}