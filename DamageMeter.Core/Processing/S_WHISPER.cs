using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_WHISPER
    {
        internal S_WHISPER(Tera.Game.Messages.S_WHISPER message)
        {
            Chat.Instance.Add(message);
        }
    }
}
