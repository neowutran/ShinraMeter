using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_DESPAWN_USER
    {
        internal S_DESPAWN_USER(Tera.Game.Messages.SDespawnUser message )
        {
            NetworkController.Instance.CharmTracker.CharmReset(message.User, new List<CharmStatus>(), message.Time.Ticks);
            NetworkController.Instance.AbnormalityTracker.DeleteAbnormality(message);
        }
    }
}
