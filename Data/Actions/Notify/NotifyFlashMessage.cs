using Data.Actions.Notify.SoundElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify
{
    public class NotifyFlashMessage
    {

        public SoundInterface Sound { get; set; }
        public Balloon Balloon { get; set; }
        public int Priority { get; set; }

      
        public NotifyFlashMessage(SoundInterface sound, Balloon balloon, int priority)
        {
            Sound = sound;
            Balloon = balloon;
            Priority = priority;
        }
    }
}
