using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class ChatMessage
    {

        public ChatMessage(string sender, string text)
        {
            Text = text;
            Sender = sender;
        }

        public string Sender { private set; get; }
        public string Text { private set; get; }
    }
}
