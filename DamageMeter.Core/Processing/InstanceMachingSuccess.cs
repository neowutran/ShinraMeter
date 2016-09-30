using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class InstanceMatchingSuccess
    {
        internal InstanceMatchingSuccess(Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH message)
        {
            Process();
        }

        internal InstanceMatchingSuccess(Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO message)
        {
            Process();
        }

        private void Process()
        {
            if (!TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = new Tuple<string, string>(
                    LP.PartyMatchingSuccess,
                    LP.PartyMatchingSuccess
                    );
            }
        }
    }
}
