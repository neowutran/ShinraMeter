using System;
using System.Windows.Input;
using Nostrum;

namespace DamageMeter.UI
{
    public class BeepVM : TSPropertyChanged
    {
        public static event Action<BeepVM> DeleteBeepEvent;

        private int _frequency;
        private int _duration;

        public int Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency == value) return;
                _frequency = value;
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

        public ICommand DeleteBeepCommand { get; }

        public BeepVM(int freq, int duration)
        {
            _frequency = freq;
            _duration = duration;

            DeleteBeepCommand = new RelayCommand(_ => DeleteBeepEvent?.Invoke(this));
        }
    }
}