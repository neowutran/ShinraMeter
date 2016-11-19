using Data.Actions.Notify.SoundElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify
{
    public class NotifyAction : Action
    {

        public SoundInterface Sound { get; set; }
        public Balloon Balloon { get; set; }

      
        public NotifyAction(SoundInterface sound, Balloon balloon)
        {
            Sound = sound;
            Balloon = balloon;
        }
    }
}
