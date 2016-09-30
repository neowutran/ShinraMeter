using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class Charm
    {
        internal Charm(Tera.Game.Messages.SPartyMemberCharmAdd message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmAdd(player.User.Id, message.CharmId, message.Status, message.Time.Ticks);
        }
        internal Charm(Tera.Game.Messages.SResetCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmReset(message.TargetId, message.Charms, message.Time.Ticks);

        }

        internal Charm(Tera.Game.Messages.SAddCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmAdd(message.TargetId, message.CharmId, message.Status, message.Time.Ticks);
        }

        internal Charm(Tera.Game.Messages.SEnableCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmEnable(NetworkController.Instance.EntityTracker.MeterUser.Id, message.CharmId, message.Time.Ticks);

        }

        internal Charm(Tera.Game.Messages.SPartyMemberCharmDel message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmDel(player.User.Id, message.CharmId, message.Time.Ticks);
        }

        internal Charm(Tera.Game.Messages.SPartyMemberCharmEnable message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmEnable(player.User.Id, message.CharmId, message.Time.Ticks);
        }

        internal Charm(Tera.Game.Messages.SPartyMemberCharmReset message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmReset(player.User.Id, message.Charms, message.Time.Ticks);
        }

    }
}
