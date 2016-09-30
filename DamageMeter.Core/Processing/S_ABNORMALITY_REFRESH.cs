using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_ABNORMALITY_REFRESH
    {
        internal S_ABNORMALITY_REFRESH(Tera.Game.Messages.SAbnormalityRefresh message)
        {
            NetworkController.Instance.AbnormalityTracker.RefreshAbnormality(message);

        }
    }
}
