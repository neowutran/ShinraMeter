using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_DESPAWN_NPC
    {
        internal S_DESPAWN_NPC(Tera.Game.Messages.SDespawnNpc message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
            DataExporter.Export(message, NetworkController.Instance.AbnormalityStorage);
        }
    }
}
