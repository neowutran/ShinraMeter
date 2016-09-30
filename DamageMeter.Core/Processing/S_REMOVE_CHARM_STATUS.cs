using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_REMOVE_CHARM_STATUS
    {
        internal S_REMOVE_CHARM_STATUS(Tera.Game.Messages.SRemoveCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmDel(NetworkController.Instance.EntityTracker.MeterUser.Id, message.CharmId, message.Time.Ticks);
        }

    }
}
