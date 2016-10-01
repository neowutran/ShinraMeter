using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class Charm
    {
        internal Charm(Tera.Game.Messages.SPartyMemberCharmAdd message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
        internal Charm(Tera.Game.Messages.SResetCharmStatus message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        internal Charm(Tera.Game.Messages.SAddCharmStatus message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        internal Charm(Tera.Game.Messages.SEnableCharmStatus message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        internal Charm(Tera.Game.Messages.SPartyMemberCharmDel message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        internal Charm(Tera.Game.Messages.SPartyMemberCharmEnable message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        internal Charm(Tera.Game.Messages.SPartyMemberCharmReset message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
        internal Charm(Tera.Game.Messages.SRemoveCharmStatus message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

    }
}
