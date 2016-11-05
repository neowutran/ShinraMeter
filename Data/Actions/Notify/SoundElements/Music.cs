using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify.SoundElements
{
    public class Music
    {

        public string SoundFile { get; set; }
        public int Volume { get; set; }
        public int Duration { get; set; }

        public Music(string soundFile, int volume, int duration)
        {
            SoundFile = soundFile;
            Volume = volume;
            Duration = duration;
        
        }
    }
}
