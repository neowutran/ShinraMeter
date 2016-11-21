using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify.SoundElements
{
    public class TextToSpeech : SoundInterface
    {

        public string Text { get; set; }
        public VoiceGender VoiceGender { get; set; }
        public VoiceAge VoiceAge { get; set; }
        public int VoicePosition { get; set; }

        public int Volume { get; set; }
        public int Rate { get; set; }
        public string CultureInfo { get; set; }

        public TextToSpeech(string text, VoiceGender voiceGender, VoiceAge voiceAge, int voicePosition, string culture, int volume, int rate )
        {
            Text = text;
            VoiceGender = voiceGender;
            VoiceAge = voiceAge;
            VoicePosition = voicePosition;
            CultureInfo = culture;
            Volume = volume;
            Rate = rate;
        }

        public void Play()
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {               
                synth.SelectVoiceByHints(VoiceGender, VoiceAge, VoicePosition, new CultureInfo(CultureInfo));
                synth.SetOutputToDefaultAudioDevice();
                synth.Volume = Volume;
                synth.Rate = Rate;
                synth.Speak(Text);
            }

        }
    }
}
