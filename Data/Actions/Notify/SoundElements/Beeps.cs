using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Actions.Notify.SoundElements
{
    public class Beeps : SoundInterface
    {

        public List<Beep> BeepList { get; set; }
        public Beeps(List<Beep> beeps)
        {
            BeepList = beeps;
        }

        public void Play()
        {
            foreach (var beep in BeepList)
            {
                if (beep.Frequency==0) Thread.Sleep(beep.Duration);
                else Console.Beep(beep.Frequency, beep.Duration);
            }
        }
    }
}
