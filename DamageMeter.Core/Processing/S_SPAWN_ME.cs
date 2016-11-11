using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamageMeter.Processing
{
    public static class S_SPAWN_ME
    {
        public static void Process(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            DamageTracker.Instance.ResetAllOnNewBoss = true;
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
    }
}
