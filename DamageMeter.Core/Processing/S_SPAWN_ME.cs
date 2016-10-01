using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_SPAWN_ME
    {
        internal S_SPAWN_ME(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            NetworkController.Instance.AbnormalityStorage.EndAll(message.Time.Ticks);
            NetworkController.Instance.AbnormalityTracker = new AbnormalityTracker(NetworkController.Instance.EntityTracker, NetworkController.Instance.PlayerTracker,
                BasicTeraData.Instance.HotDotDatabase, NetworkController.Instance.AbnormalityStorage, DamageTracker.Instance.Update);
            NetworkController.Instance.AbnormalityTracker.RegisterDead(message.Id, message.Time.Ticks, message.Dead);
        }
    }
}
