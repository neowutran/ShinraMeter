using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_REQUEST_CONTRACT
    {
        internal S_REQUEST_CONTRACT(Tera.Game.Messages.S_REQUEST_CONTRACT message)
        {
            if (!TeraWindow.IsTeraActive())
            {
                if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.PartyInvite)
                {
                    NetworkController.Instance.FlashMessage = new Tuple<string, string>(
                        LP.PartyInvite + ": " + message.Sender,
                        message.Sender
                        );
                }
                else if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.TradeRequest)
                {
                    NetworkController.Instance.FlashMessage = new Tuple<string, string>(
                        LP.Trading + ": " + message.Sender,
                        message.Sender
                        );
                }
                else if (message.Type != Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.Craft)
                {
                    NetworkController.Instance.FlashMessage = new Tuple<string, string>(
                        LP.ContactTry,
                        LP.ContactTry
                        );
                }
            }
        }
    }
}
