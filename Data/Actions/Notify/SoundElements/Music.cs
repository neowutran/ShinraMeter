using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify.SoundElements
{
    public class Music
    {

        public string File { get; set; }
        public float Volume { get; set; }
        public int Duration { get; set; }

        public Music(string soundFile, float volume, int duration)
        {
            File = soundFile;
            Volume = volume;
            Duration = duration;
        
        }
    }
}
