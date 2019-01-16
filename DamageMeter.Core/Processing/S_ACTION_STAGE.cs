using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_ACTION_STAGE
    {
        internal S_ACTION_STAGE(Tera.Game.Messages.S_ACTION_STAGE message) {
            PacketProcessor.Instance.EntityTracker.Update(message);
            if (message.Stage!=0) return;
            var entitySource = PacketProcessor.Instance.EntityTracker.GetOrPlaceholder(message.Entity);
            var entityTarget = PacketProcessor.Instance.EntityTracker.GetOrPlaceholder(message.Target) ?? PacketProcessor.Instance.PlayerTracker.GetUnknownPlayer();
            if (entitySource is UserEntity)
                Database.Database.Instance.Insert(0, Database.Database.Type.Counter, entityTarget, entitySource, message.SkillId, false , false,
                                                  message.Time.Ticks, null, HitDirection.Dot);
        }
    }
}
