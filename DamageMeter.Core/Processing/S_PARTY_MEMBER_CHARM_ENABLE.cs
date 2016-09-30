using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_PARTY_MEMBER_CHARM_ENABLE
    {
        internal S_PARTY_MEMBER_CHARM_ENABLE(Tera.Game.Messages.SPartyMemberCharmEnable message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmEnable(player.User.Id, message.CharmId, message.Time.Ticks);
        }
    }
}
