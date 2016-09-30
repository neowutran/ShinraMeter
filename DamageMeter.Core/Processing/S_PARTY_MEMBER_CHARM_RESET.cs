using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_PARTY_MEMBER_CHARM_RESET
    {
        internal S_PARTY_MEMBER_CHARM_RESET(Tera.Game.Messages.SPartyMemberCharmReset message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmReset(player.User.Id, message.Charms, message.Time.Ticks);
        }
    }
}
