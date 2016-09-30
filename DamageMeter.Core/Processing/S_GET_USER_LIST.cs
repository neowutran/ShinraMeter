using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_GET_USER_LIST
    {
        internal S_GET_USER_LIST(Tera.Game.Messages.S_GET_USER_LIST message)
        {
            NetworkController.Instance.UserLogoTracker.SetUserList(message);
        }
    }
}
