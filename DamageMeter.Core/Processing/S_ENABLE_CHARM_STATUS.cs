using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_ENABLE_CHARM_STATUS
    {
        internal S_ENABLE_CHARM_STATUS(Tera.Game.Messages.SEnableCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmEnable(NetworkController.Instance.EntityTracker.MeterUser.Id, message.CharmId, message.Time.Ticks);

        }
    }
}
