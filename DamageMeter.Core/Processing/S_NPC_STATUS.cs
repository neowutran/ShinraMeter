using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_NPC_STATUS
    {
        internal S_NPC_STATUS(Tera.Game.Messages.SNpcStatus message)
        {
            NetworkController.Instance.AbnormalityTracker.RegisterNpcStatus(message);

        }

    }
}
