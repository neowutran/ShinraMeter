using Data.Actions.Notify.SoundElements;
using Data.Actions.Notify.SoundType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify
{
    public class Sound
    {
        public List<Beep> Beeps { get; set; }
        public Music Music { get; set; }
        public SoundType SoundType { get; set; }

        public Sound(List<Beep> beeps, Music music, SoundType soundType)
        {
            Beeps = beeps;
            Music = music;
            SoundType = soundType;
        }

    }
}
