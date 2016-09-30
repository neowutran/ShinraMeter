using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_ABNORMALITY_BEGIN
    {
        internal S_ABNORMALITY_BEGIN(Tera.Game.Messages.SAbnormalityBegin message)
        {
            NetworkController.Instance.AbnormalityTracker.AddAbnormality(message);
        }
    }
}
