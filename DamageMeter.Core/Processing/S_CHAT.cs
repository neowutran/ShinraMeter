using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_CHAT
    {
        internal S_CHAT(Tera.Game.Messages.S_CHAT message)
        {
            Chat.Instance.Add(message);
        }
    }
}
