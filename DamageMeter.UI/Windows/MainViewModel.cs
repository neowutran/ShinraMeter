using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DamageMeter.AutoUpdate;
using DamageMeter.UI.Windows;
using Data;
using Nostrum;
using System.Windows.Input;
using System.Windows.Media;
using Lang;
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
        private bool _enableChatAfterOverload;
        private bool _bossGageVisible;
        private string _timerText;
        private string _totalDpsText;
        private string _totalDamageText;
        private bool _isGraphVisible;
        private bool _blurPlayerNames;
        private int _queuedPackets;
        private ImageSource _windowIcon;
        private NpcEntity _selectedEncounter; // TODO: replace NpcEntity with an EncounterViewModel
        /// <summary>
        /// This will be used for "TOTAL" encounter
        /// </summary>
        public static readonly NpcEntity TotalEncounter = new NpcEntity(EntityId.Empty, EntityId.Empty, null, new NpcInfo(0, 0, false, 0, LP.TotalEncounter, ""), new Vector3f(), new Angle()); // TODO: replace NpcEntity with an EncounterViewModel

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
        public bool ShowTimeLeft
        {
            get => BasicTeraData.Instance.WindowData.ShowTimeLeft;
            set
            {
                if (BasicTeraData.Instance.WindowData.ShowTimeLeft == value) return;
                BasicTeraData.Instance.WindowData.ShowTimeLeft = value;
                NotifyPropertyChanged();
            }
        }
        public double WindowOpacity => BasicTeraData.Instance.WindowData.MainWindowOpacity;
        public int NumberOfPlayersDisplayed => BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed;
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
        public bool HideGeneralData
        {
            get => _hideGeneralData;
            set
            {
                if (_hideGeneralData == value) return;
                _hideGeneralData = value;
                NotifyPropertyChanged();
            }
        }
        public bool BossGageVisible
        {
            get => _bossGageVisible;
            set
            {
                if (_bossGageVisible == value) return;
                _bossGageVisible = value;
                NotifyPropertyChanged();
            }
        }
        public string TimerText
        {
            get => _timerText;
            set
            {
                if (_timerText == value) return;
                _timerText = value;
                NotifyPropertyChanged();
            }
        }
        public string TotalDpsText
        {
            get => _totalDpsText;
            set
            {
                if (_totalDpsText == value) return;
                _totalDpsText = value;
                NotifyPropertyChanged();
            }
        }
        public string TotalDamageText
        {
            get => _totalDamageText;
            set
            {
                if (_totalDamageText == value) return;
                _totalDamageText = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsGraphVisible
        {
            get => _isGraphVisible;
            set
            {
                if (_isGraphVisible == value) return;
                _isGraphVisible = value;
                NotifyPropertyChanged();
            }
        }
        public bool BlurPlayerNames
        {
            get => _blurPlayerNames;
            set
            {
                if (_blurPlayerNames == value) return;
                _blurPlayerNames = value;
                NotifyPropertyChanged();
            }
        }
        public int QueuedPackets
        {
            get => _queuedPackets;
            set
            {
                if (_queuedPackets == value) return;
                _queuedPackets = value;
                NotifyPropertyChanged();
            }
        }
        public ImageSource WindowIcon
        {
            get => _windowIcon;
            set
            {
                if (_windowIcon == value) return;
                _windowIcon = value;
                NotifyPropertyChanged();
            }
        }
        
        public SynchronizedObservableCollection<NpcEntity> Encounters { get; } // TODO: replace NpcEntity with an EncounterViewModel
        public NpcEntity SelectedEncounter // TODO: replace NpcEntity with an EncounterViewModel
        {
            get => _selectedEncounter;
            set
            {
                if (_selectedEncounter == value) return;
                _selectedEncounter = value;
                NotifyPropertyChanged();
            }
        } 
        public GraphViewModel GraphData { get; }

        public ICommand TogglePauseCommand { get; }
        public ICommand ToggleAddsCommand { get; }
        public ICommand SetBossGageVisibilityCommand { get; }
        public ICommand ShowEntityStatsCommand { get; }
        public ICommand ShowUploadHistoryCommand { get; }
        public ICommand ShowBossHPBarCommand { get; }
        public ICommand VerifyCloseCommand { get; }
        public ICommand ChangeTimeLeftCommand { get; }

        public MainViewModel()
        {
            Instance = this;

            App.Setup();

            WindowTitle = "Shinra Meter v" + UpdateManager.Version;

            GraphData = new GraphViewModel();
            Encounters = new SynchronizedObservableCollection<NpcEntity>();


            PacketProcessor.Instance.Connected += OnConnected;
            PacketProcessor.Instance.PauseAction += OnPaused;
            PacketProcessor.Instance.MapChangedAction += OnMapChanged;
            PacketProcessor.Instance.DisplayGeneralDataChanged += OnDisplayGeneralDataChanged;
            PacketProcessor.Instance.OverloadedChanged += OnOverloadedChanged;
            PacketProcessor.Instance.TickUpdated += OnUpdate;

            SettingsWindowViewModel.NumberOfPlayersDisplayedChanged += OnNumberOfPlayersDisplayedChanged;

            TogglePauseCommand = new RelayCommand(_ => TogglePause());
            ToggleAddsCommand = new RelayCommand(_ => ToggleAdds());
            SetBossGageVisibilityCommand = new RelayCommand(visibility => SetBossGageVisibility((bool.Parse(visibility.ToString()))));
            ShowBossHPBarCommand = new RelayCommand(_ => App.HudContainer.BossGage.ShowWindow());
            ShowEntityStatsCommand = new RelayCommand(_ => App.HudContainer.EntityStats.ShowWindow());
            ShowUploadHistoryCommand = new RelayCommand(_ => App.HudContainer.UploadHistory.ShowWindow());
            VerifyCloseCommand = new RelayCommand(_ => App.VerifyClose(Keyboard.IsKeyDown(Key.LeftShift)));
            ChangeTimeLeftCommand = new RelayCommand(_ => ShowTimeLeft = !ShowTimeLeft);
        }

        private void OnNumberOfPlayersDisplayedChanged(int v)
        {
            NotifyPropertyChanged(nameof(NumberOfPlayersDisplayed));
        }

        private void OnUpdate(UiUpdateMessage message)
        {
            QueuedPackets = message.QueuedPackets;
            var timeValue = BasicTeraData.Instance.WindowData.ShowTimeLeft && message.StatsSummary.EntityInformation.TimeLeft > 0
                ? message.StatsSummary.EntityInformation.TimeLeft
                : message.StatsSummary.EntityInformation.Interval;

            TimerText = TimeSpan.FromSeconds(timeValue / TimeSpan.TicksPerSecond).ToString(@"mm\:ss");

            TotalDpsText = FormatHelpers.Instance.FormatValue(message.StatsSummary.EntityInformation.Interval == 0
                               ? message.StatsSummary.EntityInformation.TotalDamage
                               : message.StatsSummary.EntityInformation.TotalDamage * TimeSpan.TicksPerSecond / message.StatsSummary.EntityInformation.Interval) + LP.PerSecond;

            TotalDamageText = FormatHelpers.Instance.FormatValue(message.StatsSummary.EntityInformation.TotalDamage);

            if (BasicTeraData.Instance.WindowData.RealtimeGraphEnabled)
            {
                GraphData.Update(message);
                IsGraphVisible = true;
            }
            else
            {
                IsGraphVisible = false;
                GraphData.Reset();
            }

            UpdateEncounters(message);
        }
        private void UpdateEncounters(UiUpdateMessage message)
        {
            var currentBoss = message.StatsSummary.EntityInformation.Entity;
            var entities = message.Entities;

            if (!NeedUpdateEncounter(entities))
            {
                SelectEncounter(currentBoss);
                return;
            }

            NpcEntity selectedEntity = null;
            if (SelectedEncounter != null && SelectedEncounter != TotalEncounter)
            {
                selectedEntity = SelectedEncounter;
            }

            //todo: sync the list without resetting it
            Encounters.Clear();
            Encounters.Add(TotalEncounter);

            var selected = false;
            foreach (var entity in entities)
            {
                Encounters.Add(entity);
                if (entity != selectedEntity) continue;
                SelectedEncounter = entity;
                selected = true;
            }

            if (SelectEncounter(currentBoss)) return;
            if (selected) return;
            SelectedEncounter = Encounters.First();
        }

        private bool SelectEncounter(NpcEntity entity)
        {
            if (entity == null) { return false; }
            for (var i = 1; i < Encounters.Count; i++)
            {
                if (Encounters[i] != entity) continue;
                SelectedEncounter = Encounters[i];
                return true;
            }
            return false;
        }

        private bool NeedUpdateEncounter(IReadOnlyList<NpcEntity> entities)
        {
            if (entities.Count != Encounters.Count - 1) return true;
            for (var i = 1; i < Encounters.Count - 1; i++)
            {
                if (Encounters[i] != entities[i - 1]) return true;
            }
            return false;
        }

        private void SetBossGageVisibility(bool visibility)
        {
            BossGageVisible = visibility;
        }

        private void TogglePause()
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
            BossGageVisible = true;
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
                SetBossGageVisibility(false);
            }
            else if (_hideEid == eid)
            {
                HideGeneralData = false;
                SetBossGageVisibility(true);
            }
        }
        private void OnConnected(string servername)
        {
            WindowTitle = servername;
        }
        private void OnOverloadedChanged()
        {
            if (PacketProcessor.Instance.Overloaded)
            {
                if (BasicTeraData.Instance.WindowData.EnableChat)
                {
                    BasicTeraData.Instance.WindowData.EnableChat = false;
                    _enableChatAfterOverload = true;
                }
            }
            else
            {
                if (_enableChatAfterOverload)
                    BasicTeraData.Instance.WindowData.EnableChat = true;
            }
        }

    }

    public class PlayersDisplayedCountToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int val)) val = 1;
            return val * 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}