using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Actions.Notify.SoundElements
{
    public class Music : SoundInterface
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

        public void Play()
        {
            var file = Path.Combine(BasicTeraData.Instance.ResourceDirectory, "sound/", File);
            try
            {
                var outputStream = new MediaFoundationReader(file);
                var volumeStream = new WaveChannel32(outputStream);
                volumeStream.Volume = Volume;
                //Create WaveOutEvent since it works in Background and UI Threads
                var player = new DirectSoundOut();
                //Init Player with Configured Volume Stream
                player.Init(volumeStream);
                player.Play();

                var timer = new Timer((obj) =>
                {
                    player.Stop();
                    player.Dispose();
                    volumeStream.Dispose();
                    outputStream.Dispose();
                    outputStream = null;
                    player = null;
                    volumeStream = null;
                }, null, Duration, Timeout.Infinite);
            }
            catch (Exception e)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                BasicTeraData.LogError("Sound ERROR test" + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine + e.InnerException + Environment.NewLine + e + Environment.NewLine + "filename:" + file + Environment.NewLine + "line:" + line, false, true);
            }
        }
    }
}
