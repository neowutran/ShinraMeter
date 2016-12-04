using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    public static class S_SPAWN_ME
    {
        public static void Process(Tera.Game.Messages.SpawnMeServerMessage message)
        { 
            NotifyProcessor.SpawnMe(message);
            NetworkController.Instance.PlayerTracker.ResetOutOfRange();
        }
    }
}
