using System;
using System.Collections.Generic;
using System.Threading;

namespace Data.Actions.Notify.SoundElements
{
    public class Beeps : SoundInterface
    {
        public Beeps(List<Beep> beeps)
        {
            BeepList = beeps;
        }

        public List<Beep> BeepList { get; set; }

        public void Play()
        {
            foreach (var beep in BeepList)
                if (beep.Frequency == 0) Thread.Sleep(beep.Duration);
                else Console.Beep(beep.Frequency, beep.Duration);
        }
    }
}