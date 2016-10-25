using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class NotifyMessage
    {
        public SoundEnum Sound { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }

        public enum SoundEnum
        {
            NoSound = 0,
            Type1 = 1,
            Type2 = 2,
            Type3 = 3,
            Type4 = 4
        }

        public NotifyMessage(string title, string content, SoundEnum sound = SoundEnum.NoSound)
        {
            Sound = sound;
            Title = title;
            Content = content;
        }


    }
}
