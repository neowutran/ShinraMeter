using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_DESPAWN_NPC
    {
        internal S_DESPAWN_NPC(Tera.Game.Messages.SDespawnNpc message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
            NotifyProcessor.Instance.DespawnNpc(message);
            DataExporter.AutomatedExport(message, NetworkController.Instance.AbnormalityStorage);
        }
    }
}
