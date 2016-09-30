using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_ADD_CHARM_STATUS
    {
        internal S_ADD_CHARM_STATUS(Tera.Game.Messages.SAddCharmStatus message)
        {
            NetworkController.Instance.CharmTracker.CharmAdd(message.TargetId, message.CharmId, message.Status, message.Time.Ticks);
        }
    }
}
