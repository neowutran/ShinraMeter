using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_GET_USER_GUILD_LOGO
    {
        internal S_GET_USER_GUILD_LOGO(Tera.Game.Messages.S_GET_USER_GUILD_LOGO message)
        {
            NetworkController.Instance.UserLogoTracker.AddLogo(message);
        }
    }
}
