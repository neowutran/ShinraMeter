using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class Abnormality
    {
        internal Abnormality(Tera.Game.Messages.SAbnormalityBegin message)
        {
            NetworkController.Instance.AbnormalityTracker.AddAbnormality(message);
        }

        internal Abnormality(Tera.Game.Messages.SAbnormalityRefresh message)
        {
            NetworkController.Instance.AbnormalityTracker.RefreshAbnormality(message);

        }

        internal Abnormality(Tera.Game.Messages.SAbnormalityEnd message)
        {
            NetworkController.Instance.AbnormalityTracker.DeleteAbnormality(message);
        }
    }
}
