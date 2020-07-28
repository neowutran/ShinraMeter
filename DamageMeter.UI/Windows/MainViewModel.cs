using System.Windows.Input;
using DamageMeter.UI.Windows;
using Data;
using Nostrum;
using Tera.Game;

namespace DamageMeter.UI
{
    public class MainViewModel : TSPropertyChanged
    {
        public static MainViewModel Instance { get; private set; }
        private string _windowTitle = "Shinra Meter";

        private bool _paused;
        private bool _waitingMapChangeTBVisible;
        private bool _mapChanged;
        private bool _hideGeneralData;
        private EntityId _hideEid;

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (_windowTitle == value) return;
                _windowTitle = value;
                NotifyPropertyChanged();
            }
        }
        public bool Paused
        {
            get => _paused;
            set
            {
                if (_paused == value) return;
                _paused = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(UserPaused));

                if (_paused)
                {
                    _mapChanged = false;
                    WaitingMapChangeTBVisibile = !UserPaused;
                }
                else
                {
                    WaitingMapChangeTBVisibile = !_mapChanged;
                }

            }
        }
        public bool UserPaused => BasicTeraData.Instance.WindowData.UserPaused;
        public double WindowOpacity => BasicTeraData.Instance.WindowData.MainWindowOpacity;
        public bool ShowAdds => PacketProcessor.Instance.TimedEncounter;
        public bool WaitingMapChangeTBVisibile
        {
            get => _waitingMapChangeTBVisible;
            set
            {
                if (_waitingMapChangeTBVisible == value) return;
                _waitingMapChangeTBVisible = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand TogglePauseCommand { get; }
        public ICommand ToggleAddsCommand { get; }
        public ICommand ShowEntityStatsCommand { get; }

        public bool HideGeneralData
        {
            get => _hideGeneralData;
            set
            {
                if(_hideGeneralData == value) return;
                _hideGeneralData = value;
                NotifyPropertyChanged();
            }
        }


        public MainViewModel()
        {
            Instance = this;

            App.Setup();
            PacketProcessor.Instance.Connected += OnConnected;
            PacketProcessor.Instance.PauseAction += OnPaused;
            PacketProcessor.Instance.MapChangedAction += OnMapChanged;
            PacketProcessor.Instance.DisplayGeneralDataChanged += OnDisplayGeneralDataChanged;

            TogglePauseCommand = new RelayCommand(_ => TogglePause());
            ToggleAddsCommand = new RelayCommand(_ => ToggleAdds());
        }

        public void TogglePause()
        {
            BasicTeraData.Instance.WindowData.UserPaused = !BasicTeraData.Instance.WindowData.UserPaused;
            if (BasicTeraData.Instance.WindowData.UserPaused)
            {
                PacketProcessor.Instance.NeedPause = true;
            }

            Paused = BasicTeraData.Instance.WindowData.UserPaused;
            
            SettingsWindowViewModel.NotifyPausedChanged();
        }
        private void ToggleAdds()
        {
            PacketProcessor.Instance.TimedEncounter = !PacketProcessor.Instance.TimedEncounter;
            NotifyPropertyChanged(nameof(ShowAdds));
        }

        private void OnMapChanged()
        {
            if (!Paused)
            {
                _mapChanged = true;
                WaitingMapChangeTBVisibile = false;
            }

            HideGeneralData = false;
        }

        private void OnPaused(bool paused)
        {
            Paused = paused;

        }
        private void OnDisplayGeneralDataChanged(bool hide, EntityId eid)
        {
            if (hide)
            {
                _hideEid = eid;
                HideGeneralData = true;
            }
            else if (_hideEid == eid)
            {
                HideGeneralData = false;
            }
        }
        private void OnConnected(string servername)
        {
            WindowTitle = servername;

        }
    }
}