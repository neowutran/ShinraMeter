using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_PARTY_MEMBER_CHANGE_HP
    {
        internal S_PARTY_MEMBER_CHANGE_HP(Tera.Game.Messages.SPartyMemberChangeHp message)
        {
            var user = NetworkController.Instance.PlayerTracker.GetOrNull(message.ServerId, message.PlayerId);
            if (user == null) return;
            NetworkController.Instance.AbnormalityTracker.RegisterSlaying(user.User, message.Slaying, message.Time.Ticks);
        }
    }
}
