using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_PLAYER_STAT_UPDATE
    {
        internal S_PLAYER_STAT_UPDATE(Tera.Game.Messages.S_PLAYER_STAT_UPDATE message)
        {
            NetworkController.Instance.AbnormalityTracker.RegisterSlaying(NetworkController.Instance.EntityTracker.MeterUser, message.Slaying, message.Time.Ticks);
        }
    }
}
