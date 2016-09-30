using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_NPC_OCCUPIER_INFO
    {
        internal S_NPC_OCCUPIER_INFO(Tera.Game.Messages.SNpcOccupierInfo message)
        {
            DamageTracker.Instance.UpdateEntities(new NpcOccupierResult(message), message.Time.Ticks);
        }
    }
}
