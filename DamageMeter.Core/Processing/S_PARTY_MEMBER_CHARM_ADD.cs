using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_PARTY_MEMBER_CHARM_ADD
    {
        internal S_PARTY_MEMBER_CHARM_ADD(Tera.Game.Messages.SPartyMemberCharmAdd message)
        {
            var player = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (player == null) return;
            NetworkController.Instance.CharmTracker.CharmAdd(player.User.Id, message.CharmId, message.Status, message.Time.Ticks);
        }
    }
}
