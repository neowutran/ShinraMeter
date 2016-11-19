using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify.SoundElements
{
    public class Beep
    {

        public int Frequency { get; set; }
        public int Duration { get; set; }
        public Beep(int frequency, int duration){
            Frequency = frequency;
            Duration = duration;
        }

   
    }
}
