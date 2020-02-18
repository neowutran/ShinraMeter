using System.Globalization;

namespace Data.Actions.Notify.SoundElements
{
    
    
    public class TextToSpeech : SoundInterface
    {
        public TextToSpeech(string text, VoiceGender voiceGender, VoiceAge voiceAge, int voicePosition, string culture, int volume, int rate)
        {
            Text = text;
            VoiceGender = voiceGender;
            VoiceAge = voiceAge;
            VoicePosition = voicePosition;
            CultureInfo = culture;
            Volume = volume;
            Rate = rate;
        }

        public string Text { get; set; }
        public VoiceGender VoiceGender { get; set; }
        public VoiceAge VoiceAge { get; set; }
        public int VoicePosition { get; set; }

        public int Volume { get; set; }
        public int Rate { get; set; }
        public string CultureInfo { get; set; }

#if NETFULL
        public void Play()
        {
            using (var synth = new System.Speech.Synthesis.SpeechSynthesizer())
            {
                synth.SelectVoiceByHints((System.Speech.Synthesis.VoiceGender)VoiceGender, (System.Speech.Synthesis.VoiceAge)VoiceAge, VoicePosition, new CultureInfo(CultureInfo));
                synth.SetOutputToDefaultAudioDevice();
                synth.Volume = Volume;
                synth.Rate = Rate;
                synth.Speak(Text);
            }
        }
#endif

#if NETCORE
        public void Play() {
            var synth = new SpeechLib.SpVoiceClass();
            var voices = synth.GetVoices("", $"Gender={VoiceGender};Age={VoiceAge};Language={new CultureInfo(CultureInfo).LCID:X}");
            synth.Voice = voices.Item(0);
            synth.Volume = Volume;
            synth.Rate = Rate;
            synth.Speak(Text);
        }
#endif
    }
    //
    // Summary:
    //     Defines the values for the gender of a synthesized voice.
    public enum VoiceGender
    {
        //
        // Summary:
        //     Indicates no voice gender specification.
        NotSet = 0,
        //
        // Summary:
        //     Indicates a male voice.
        Male = 1,
        //
        // Summary:
        //     Indicates a female voice.
        Female = 2,
        //
        // Summary:
        //     Indicates a gender-neutral voice.
        Neutral = 3
    }
    //
    // Summary:
    //     Defines the values for the age of a synthesized voice.
    public enum VoiceAge
    {
        //
        // Summary:
        //     Indicates that no voice age is specified.
        NotSet = 0,
        //
        // Summary:
        //     Indicates a child voice (age 10).
        Child = 10,
        //
        // Summary:
        //     Indicates a teenage voice (age 15).
        Teen = 15,
        //
        // Summary:
        //     Indicates an adult voice (age 30).
        Adult = 30,
        //
        // Summary:
        //     Indicates a senior voice (age 65).
        Senior = 65
    }
}