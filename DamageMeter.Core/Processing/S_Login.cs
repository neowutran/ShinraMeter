using Data;
using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_LOGIN
    {
        internal S_LOGIN(Tera.Game.Messages.LoginServerMessage message)
        {
            if (NetworkController.Instance.NeedInit)
            {
                NetworkController.Instance.RaiseConnected(BasicTeraData.Instance.Servers.GetServerName(message.ServerId, NetworkController.Instance.Server));
                bool trackerreset = true;
                if (NetworkController.Instance.EntityTracker != null)
                {
                    try
                    {
                        var oldregion = BasicTeraData.Instance.Servers.GetServer(NetworkController.Instance.EntityTracker.MeterUser.ServerId).Region;
                        trackerreset = NetworkController.Instance.Server.Region != oldregion;
                    }
                    catch (Exception e)
                    {
                        BasicTeraData.LogError("New server:" + NetworkController.Instance.Server + ";Old server Id:" + NetworkController.Instance.EntityTracker.MeterUser.ServerId, false, true);
                        throw;
                    }
                }
                NetworkController.Instance.Server = BasicTeraData.Instance.Servers.GetServer(message.ServerId, NetworkController.Instance.Server);
                NetworkController.Instance.MessageFactory.Version = NetworkController.Instance.Server.Region;
                if (trackerreset)
                {
                    NetworkController.Instance.TeraData = BasicTeraData.Instance.DataForRegion(NetworkController.Instance.Server.Region);
                    BasicTeraData.Instance.HotDotDatabase.Get(8888888).Name = LP.Enrage;
                    BasicTeraData.Instance.HotDotDatabase.Get(8888889).Name = LP.Slaying;
                    BasicTeraData.Instance.HotDotDatabase.Get(8888889).Tooltip = LP.SlayingTooltip;
                    NetworkController.Instance.EntityTracker = new EntityTracker(BasicTeraData.Instance.MonsterDatabase);
                    NetworkController.Instance.PlayerTracker = new PlayerTracker(NetworkController.Instance.EntityTracker, BasicTeraData.Instance.Servers);
                    Database.Database.Instance.DeleteAll();
                    NetworkController.Instance.PacketProcessing.UpdateEntityTracker();
                    NetworkController.Instance.PacketProcessing.UpdatePlayerTracker();
                }
                NetworkController.Instance.NeedInit = false;
            }
            NetworkController.Instance.AbnormalityStorage.EndAll(message.Time.Ticks);
            NetworkController.Instance.AbnormalityTracker = new AbnormalityTracker(NetworkController.Instance.EntityTracker, NetworkController.Instance.PlayerTracker,
                BasicTeraData.Instance.HotDotDatabase, NetworkController.Instance.AbnormalityStorage, DamageTracker.Instance.Update);
            NetworkController.Instance.OnGuildIconAction(NetworkController.Instance.UserLogoTracker.GetLogo(message.PlayerId));
            NetworkController.Instance.EntityTracker.Update(message);
        }
    }
}
