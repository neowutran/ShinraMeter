using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_CREATURE_CHANGE_HP
    {

        internal S_CREATURE_CHANGE_HP(Tera.Game.Messages.SCreatureChangeHp message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
    }
}
