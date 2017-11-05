using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class S_CREATURE_CHANGE_HP
    {
        internal S_CREATURE_CHANGE_HP(SCreatureChangeHp message)
        {
            HudManager.Instance.UpdateBoss(message);
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
        }
    }
}
