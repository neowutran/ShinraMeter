using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_DESPAWN_USER
    {
        internal S_DESPAWN_USER(Tera.Game.Messages.SDespawnUser message )
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
    }
}
