using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamageMeter.Processing
{
    public static class S_SPAWN_NPC
    {
        public static void Process(Tera.Game.Messages.SpawnNpcServerMessage message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            DamageTracker.Instance.UpdateEntities(message);
        }
    }
}
