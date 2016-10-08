using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class NotifyMessage
    {
        public bool HaveSound { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }

        public NotifyMessage(string title, string content, bool haveSound = true)
        {
            HaveSound = haveSound;
            Title = title;
            Content = content;
        }


    }
}
