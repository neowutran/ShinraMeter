using DamageMeter.AutoUpdate;
using DamageMeter.UI.Windows;
using Data;
using Lang;
using Nostrum.Factories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Nostrum;
using Tera.Game;

namespace DamageMeter.UI
{
    public readonly struct ToastInfo
    {
        public enum Severity
        {
            Info,
            Success,
            Warning,
            Error
        }
        public string Text { get; }
        public Severity SeverityLevel { get; }

        public ToastInfo(string txt, Severity lvl)
        {
            Text = txt;
            SeverityLevel = lvl;
        }
    }

    public class ToastViewModel : TSPropertyChanged
    {
        private readonly Queue<ToastInfo> _queue;
        private bool _visible;
        private string _text;
        private ToastInfo.Severity _severity;

        public bool Visible
        {
            get => _visible;
            private set
            {
                if (_visible == value) return;
                _visible = value;
                NotifyPropertyChanged();
            }
        }
        public string Text
        {
            get => _text;
            private set
            {
                if (_text == value) return;
                _text = value;
                NotifyPropertyChanged();
            }
        }
        public ToastInfo.Severity Severity
        {
            get => _severity;
            private set
            {
                if (_severity == value) return;
                _severity = value;
                NotifyPropertyChanged();
            }
        }

        public ToastViewModel()
        {
            _queue = new Queue<ToastInfo>();
        }

        public void Show(string txt, ToastInfo.Severity lvl)
        {
            if (Visible)
            {
                Enqueue(new ToastInfo(txt, lvl));
                return;
            }

            Text = txt;
            Severity = lvl;
            Visible = true;

            Task.Delay(3000).ContinueWith(t =>
            {
                Visible = false;
                if (_queue.Count == 0) return;
                Task.Delay(500).ContinueWith(tt =>
                {
                    var next = _queue.Dequeue();
                    Show(next.Text, next.SeverityLevel);
                });
            });
        }

        public void Enqueue(ToastInfo info)
        {
            _queue.Enqueue(info);
        }
    }

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
        private bool _connected;
        private string _timerText;
        private string _totalDpsText;
        private string _totalDamageText;
        private bool _isGraphVisible;
        private bool _blurPlayerNames;
        private int _queuedPackets;
        private bool _isEmpty;
        private bool _isMouseOver;

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
                NotifyPropertyChanged(nameof(MainVersionText));
                NotifyPropertyChanged(nameof(DbVersionText));
            }
        }

        public string MainVersionText
        {
            get
            {
                var split = WindowTitle.Replace("Shinra Meter v", "").Split('.');
                if (split.Length == 1) return "";
                return "v" + split[0] + "." + split[1];
            }
        }
        public string DbVersionText
        {
            get
            {
                var split = WindowTitle.Replace("Shinra Meter v", "").Split('.');
                if (split.Length == 1) return "";
                return "." + split[2];
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
        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected == value) return;
                _connected = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ConnectionStatusText));
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
                NotifyPropertyChanged(nameof(LoadWarning));
                NotifyPropertyChanged(nameof(LoadCritical));
            }
        }
        public bool LoadWarning => QueuedPackets > 3000;
        public bool LoadCritical => QueuedPackets > 5000;
        public bool ClickThru => BasicTeraData.Instance.WindowData.ClickThrou;

        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                if (_isEmpty == value) return;
                _isEmpty = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsMouseOver
        {
            get => _isMouseOver;
            set
            {
                if (_isMouseOver == value) return;
                _isMouseOver = value;
                NotifyPropertyChanged();
            }
        }



        public string ConnectionStatusText => Connected ? PacketProcessor.Instance.Server?.Name : LP.SystemTray_No_server;

        public Color BackgroundColor => BasicTeraData.Instance.WindowData.BackgroundColor;
        public Color BorderColor => BasicTeraData.Instance.WindowData.BorderColor;

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
        public SynchronizedObservableCollection<PlayerDamageViewModel> Players { get; }
        public ICollectionViewLiveShaping PlayersView { get; set; }
        public GraphViewModel GraphData { get; }
        public ToastViewModel ToastData { get; }

        public ICommand TogglePauseCommand { get; }
        public ICommand ToggleAddsCommand { get; }
        public ICommand SetBossGageVisibilityCommand { get; }
        public ICommand ShowEntityStatsCommand { get; }
        public ICommand ShowUploadHistoryCommand { get; }
        public ICommand ShowBossHPBarCommand { get; }
        public ICommand VerifyCloseCommand { get; }
        public ICommand ChangeTimeLeftCommand { get; }
        public ICommand OpenSettingsCommand { get; }


        public MainViewModel()
        {
            Instance = this;

            App.Setup();

            WindowTitle = "Shinra Meter v" + UpdateManager.Version;

            GraphData = new GraphViewModel();
            ToastData = new ToastViewModel();
            Encounters = new SynchronizedObservableCollection<NpcEntity>();
            Players = new SynchronizedObservableCollection<PlayerDamageViewModel>();
            PlayersView = CollectionViewFactory.CreateLiveCollectionView(Players,
                sortFilters: new[]
                {
                    new SortDescription(nameof(PlayerDamageViewModel.ShowFirst), ListSortDirection.Descending),
                    new SortDescription(nameof(PlayerDamageViewModel.Amount), ListSortDirection.Descending)
                });

            PacketProcessor.Instance.Connected += OnConnected;
            PacketProcessor.Instance.PauseAction += OnPaused;
            PacketProcessor.Instance.MapChangedAction += OnMapChanged;
            PacketProcessor.Instance.DisplayGeneralDataChanged += OnDisplayGeneralDataChanged;
            PacketProcessor.Instance.OverloadedChanged += OnOverloadedChanged;
            PacketProcessor.Instance.TickUpdated += OnUpdate;

            SettingsWindowViewModel.NumberOfPlayersDisplayedChanged += OnNumberOfPlayersDisplayedChanged;
            SettingsWindowViewModel.WindowColorsChanged += OnWindowColorsChanged;

            BasicTeraData.Instance.WindowData.ClickThruChanged += OnClickThruChanged;
            DataExporter.FightSendStatusUpdated += OnFightSendStatusUpdated;

            TogglePauseCommand = new RelayCommand(_ => TogglePause());
            ToggleAddsCommand = new RelayCommand(_ => ToggleAdds());
            SetBossGageVisibilityCommand = new RelayCommand(visibility => SetBossGageVisibility(bool.Parse(visibility.ToString() ?? "False")));
            ShowBossHPBarCommand = new RelayCommand(_ => App.HudContainer.BossGage.ShowWindow());
            ShowEntityStatsCommand = new RelayCommand(_ => App.HudContainer.EntityStats.ShowWindow());
            ShowUploadHistoryCommand = new RelayCommand(_ => App.HudContainer.UploadHistory.ShowWindow());
            VerifyCloseCommand = new RelayCommand(_ => App.VerifyClose(Keyboard.IsKeyDown(Key.LeftShift)));
            ChangeTimeLeftCommand = new RelayCommand(_ => ShowTimeLeft = !ShowTimeLeft);
            OpenSettingsCommand = new RelayCommand(_ => SettingsWindow.ShowWindow());

        }

        private void OnFightSendStatusUpdated(DataExporter.FightSendStatus status, string txt)
        {
            var lvl = status switch
            {
                DataExporter.FightSendStatus.Success => ToastInfo.Severity.Success,
                DataExporter.FightSendStatus.InProgress => ToastInfo.Severity.Info,
                DataExporter.FightSendStatus.Failed => ToastInfo.Severity.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
            ToastData.Show(txt, lvl);
        }

        private void OnWindowColorsChanged()
        {
            NotifyPropertyChanged(nameof(BackgroundColor));
            NotifyPropertyChanged(nameof(BorderColor));
        }

        private void OnClickThruChanged()
        {
            NotifyPropertyChanged(nameof(ClickThru));
            ToastData.Show($"Clickthru {(ClickThru ? "en" : "dis")}abled", ToastInfo.Severity.Info);
        }

        private void OnNumberOfPlayersDisplayedChanged(int v)
        {
            NotifyPropertyChanged(nameof(NumberOfPlayersDisplayed));
        }

        private void OnUpdate(UiUpdateMessage message)
        {
            App.MainDispatcher.InvokeAsync(() =>
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
                    Task.Run(() =>
                    {
                        App.MainDispatcher.InvokeAsync(() => GraphData.Update(message), DispatcherPriority.Background);
                    });
                    IsGraphVisible = true;
                }
                else
                {
                    IsGraphVisible = false;
                    GraphData.Reset();
                }

                UpdateEncounters(message);

                var statsDamage = message.StatsSummary.PlayerDamageDealt;
                var statsHeal = message.StatsSummary.PlayerHealDealt;
                foreach (var damageDealt in statsDamage)
                {
                    //if (!Controls.ContainsKey(item.Source)) { continue; }
                    //if (Players.Contains(Controls[item.Source]))
                    //{
                    //    BasicTeraData.LogError("duplicate playerinfo: \r\n" + string.Join("\r\n ", statsDamage.Select(x => x.Source.ToString() + " ->  " + x.Amount)), false, true);
                    //    continue;
                    //}
                    var healDealt = statsHeal.FirstOrDefault(x => x.Source == damageDealt.Source);

                    var existing = Players.FirstOrDefault(p => p.Name == damageDealt.Source.Name);
                    if (existing != null)
                    {
                        existing.Update(damageDealt, healDealt, message.StatsSummary.EntityInformation, message.Skills, message.Abnormals,
                            message.TimedEncounter);
                    }
                    else
                    {
                        Players.Add(new PlayerDamageViewModel(damageDealt, healDealt, message.StatsSummary.EntityInformation, message.Skills, message.Abnormals, message.TimedEncounter));
                    }

                    //Controls[item.Source].Repaint(item, statsHeal.FirstOrDefault(x => x.Source == item.Source), 
                    //    message.StatsSummary.EntityInformation, message.Skills,
                    //    message.Abnormals.Get(item.Source), message.TimedEncounter);
                }


                Players.Where(x => statsDamage.All(sd => sd.Source.Name != x.Name)).ToList().ForEach(p =>
                {
                    //todo: close details window associated to this player first
                    Players.Remove(p);
                });


                IsEmpty = Players.Count == 0 && Encounters.Count <= 1;
            });
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
            var alreadyConnected = Connected;
            var sameServer = servername == WindowTitle;
            if (alreadyConnected && sameServer) return;
            WindowTitle = servername;
            Connected = servername != LP.SystemTray_No_server;
            ToastData.Show(Connected ?"Connected to " + servername : "Disconnected", Connected ? ToastInfo.Severity.Success : ToastInfo.Severity.Info);
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
}