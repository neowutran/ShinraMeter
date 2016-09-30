using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_ABNORMALITY_END
    {
        internal S_ABNORMALITY_END(Tera.Game.Messages.SAbnormalityEnd message)
        {
            NetworkController.Instance.AbnormalityTracker.DeleteAbnormality(message);
        }
    }
}
