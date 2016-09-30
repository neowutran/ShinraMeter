using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class Chat
    {
        internal Chat(Tera.Game.Messages.S_PRIVATE_CHAT message)
        {
            DamageMeter.Chat.Instance.Add(message);
        }

        internal Chat(Tera.Game.Messages.S_CHAT message)
        {
            DamageMeter.Chat.Instance.Add(message);
        }

        internal Chat(Tera.Game.Messages.S_WHISPER message)
        {
            DamageMeter.Chat.Instance.Add(message);
        }
    }
}
