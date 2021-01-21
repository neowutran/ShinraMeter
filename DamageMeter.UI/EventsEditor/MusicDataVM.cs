using Data.Actions.Notify.SoundElements;
using System;

namespace DamageMeter.UI
{
    public class MusicDataVM : BaseSoundVM
    {
        private string _file;
        private int _volume; //0-100
        private int _duration;

        public string File
        {
            get => _file;
            set
            {
                if (_file == value) return;
                _file = value;
                NotifyPropertyChanged();
            }
        }

        public int Volume
        {
            get => _volume;
            set
            {
                if (_volume == value) return;
                _volume = value;
                NotifyPropertyChanged();
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                if (_duration == value) return;
                _duration = value;
                NotifyPropertyChanged();
            }
        }

        public MusicDataVM(Music music)
        {
            _file = music.File;
            _volume = Convert.ToInt32(music.Volume);
            _duration = music.Duration;
        }
    }
}