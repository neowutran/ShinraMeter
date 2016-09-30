using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_RESET_CHARM_STATUS
    {
        internal S_RESET_CHARM_STATUS(Tera.Game.Messages.SResetCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmReset(message.TargetId, message.Charms, message.Time.Ticks);

        }
    }
}
