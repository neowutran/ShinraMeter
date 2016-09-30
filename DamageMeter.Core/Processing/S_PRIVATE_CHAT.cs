using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_PRIVATE_CHAT
    {
        internal S_PRIVATE_CHAT(Tera.Game.Messages.S_PRIVATE_CHAT message)
        {
            Chat.Instance.Add(message);
        }
    }
}
