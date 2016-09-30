using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_PLAYER_CHANGE_MP
    {
        internal S_PLAYER_CHANGE_MP(Tera.Game.Messages.SPlayerChangeMp message)
        {
            if (message.SourceId != NetworkController.Instance.EntityTracker.MeterUser.Id &&
                      message.TargetId != NetworkController.Instance.EntityTracker.MeterUser.Id &&
                      NetworkController.Instance.EntityTracker.GetOrPlaceholder(message.TargetId).RootOwner != NetworkController.Instance.EntityTracker.MeterUser)
            {
                var source = NetworkController.Instance.EntityTracker.GetOrPlaceholder(message.SourceId);
                BasicTeraData.LogError("SPlayerChangeMp need rootowner update2:" + (source as NpcEntity)?.Info.Name ?? source.GetType() + ": " + source, false, true);
            }
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
    }
}
