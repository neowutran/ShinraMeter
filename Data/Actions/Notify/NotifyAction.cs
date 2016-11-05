using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify
{
    public class NotifyAction : Action
    {

        public Sound Sound { get; set; }
        public Balloon Balloon { get; set; }

        public NotifyAction(Sound sound, Balloon balloon)
        {
            Sound = sound;
            Balloon = balloon;
        }
    }
}
