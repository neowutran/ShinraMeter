using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Processing
{
    internal class S_TRADE_BROKER_DEAL_SUGGESTED
    {
        internal S_TRADE_BROKER_DEAL_SUGGESTED(Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED message)
        {
            if (!TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = new Tuple<string, string>(
                    LP.Trading + ": " + message.PlayerName,
                    LP.SellerPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.SellerPrice) +
                    Environment.NewLine +
                    LP.OfferedPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.OfferedPrice)
                    );
            }
        }
    }
}
