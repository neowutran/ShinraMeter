using System;
using System.Collections.Generic;
using System.Globalization;
using Data;
using Data.Actions.Notify;
using Data.Actions.Notify.SoundElements;

namespace DamageMeter.UI
{
    public class ActionVM : TSPropertyChanged
    {
        private string _balloonTitle;
        private string _balloonBody;
        private int _balloonDisplayTime;
        private EventType _eventType;
        private bool _hasBalloon;
        private SoundType _soundType;

        public SoundType SoundType
        {
            get => _soundType;
            set
            {
                if (_soundType == value) return;
                _soundType = value;
                SoundData = _soundType switch
                {
                    SoundType.Music => new MusicDataVM(new Music("", 100, 1000)),
                    SoundType.Beeps => new BeepsDataVM(new Beeps(new List<Beep> { new(250, 500), new(0, 250), new(250, 500) })),
                    SoundType.TTS => new TtsDataVM(new TextToSpeech("", VoiceGender.Neutral, VoiceAge.Adult, 0, BasicTeraData.Instance.WindowData.UILanguage, 100, 0)),
                    _ => null
                };
                NotifyPropertyChanged();
            }
        }

        public bool HasBalloon
        {
            get => _hasBalloon;
            set
            {
                if (_hasBalloon == value) return;
                _hasBalloon = value;
                NotifyPropertyChanged();
            }
        }

        public string BalloonTitle
        {
            get => _balloonTitle;
            set
            {
                if (_balloonTitle == value) return;
                _balloonTitle = value;
                NotifyPropertyChanged();
            }
        }

        public string BalloonText
        {
            get => _balloonBody;
            set
            {
                if (_balloonBody == value) return;
                _balloonBody = value;
                NotifyPropertyChanged();
            }
        }

        public int BalloonDisplayTime
        {
            get => _balloonDisplayTime;
            set
            {
                if (_balloonDisplayTime == value) return;
                _balloonDisplayTime = value;
                NotifyPropertyChanged();
            }
        }

        public EventType EventType
        {
            get => _eventType;
            set
            {
                if (_eventType == value) return;
                _eventType = value;
                NotifyPropertyChanged();
            }
        }

        private BaseSoundVM _soundData;

        public BaseSoundVM SoundData
        {
            get => _soundData;
            set
            {
                if (_soundData == value) return;
                _soundData = value;
                NotifyPropertyChanged();
            }
        }

        public ActionVM(Balloon balloon, SoundInterface s)
        {
            if (balloon != null)
            {
                _hasBalloon = true;

                _balloonTitle = balloon.TitleText;
                _balloonBody = balloon.BodyText;

                _balloonDisplayTime = balloon.DisplayTime;

                _eventType = balloon.EventType;
            }

            if (s != null)
            {
                _soundType = s switch
                {
                    Music => SoundType.Music,
                    Beeps => SoundType.Beeps,
                    TextToSpeech => SoundType.TTS,
                    _ => SoundType.None
                };

                SoundData = s switch
                {
                    Music m => new MusicDataVM(m),
                    Beeps b => new BeepsDataVM(b),
                    TextToSpeech tts => new TtsDataVM(tts),
                    _ => null
                };
            }
        }
    }
}