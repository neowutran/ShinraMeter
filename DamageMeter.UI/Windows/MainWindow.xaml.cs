using DamageMeter.AutoUpdate;
using DamageMeter.D3D9Render.TeraData;
using DamageMeter.Database.Structures;
using DamageMeter.Sniffing;
using DamageMeter.TeraDpsApi;
using DamageMeter.UI.EntityStats;
using DamageMeter.UI.HUD.Windows;
using DamageMeter.UI.Windows;
using Data;
using Lang;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tera.Game;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace DamageMeter.UI
{
    public partial class MainWindow
    {
        // ========================================================================
        public MainViewModel DC { get; set; } // Temporary reference, remove after.
        // ========================================================================

        private readonly DispatcherTimer _dispatcherTimer;
        //private readonly BossGageWindow _bossGageBar;
        //private readonly EntityStatsMain _entityStats;
        //private readonly PopupNotification _popupNotification;
        //private readonly TeradpsHistory _windowHistory;
        ////private Chatbox _chatbox;
        public GraphViewModel GraphViewModel { get; }
        public D3D9Render.Renderer DXrender;

        private bool _topMost = true;
        //private bool _paused = false;
        //private bool _mapChanged = true;
        //private EntityId _hideEid;

        //private bool _hideGeneralData = false;
        internal bool ForceWindowVisibilityHidden;

        public static MainWindow Instance { get; private set; }


        public MainWindow()
        {
            App.SplashScreen?.SetText("Initializing main window...");

            // ========================================================================
            DC = new MainViewModel(); // todo: Temporary reference, remove after (assign to DataContext directly).
            DataContext = DC;
            // ========================================================================
            App.WindowManager.MainWindow = this;

            InitializeComponent();

            if (BasicTeraData.Instance.WindowData.InvisibleUi) { Visibility = Visibility.Hidden; }
            Console.WriteLine("SuperUniqueStringEasilyDetectableToBeAbleToRandomizeTheProgramAndBypassSignatureBasedBlock");

            PacketProcessor.Instance.Connected += OnConnected;
            PacketProcessor.Instance.TickUpdated += Update;
            PacketProcessor.Instance.SetClickThrouAction += SetClickThrou;
            PacketProcessor.Instance.UnsetClickThrouAction += UnsetClickThrou;
            PacketProcessor.Instance.GuildIconAction += InstanceOnGuildIconAction;
            //PacketProcessor.Instance.PauseAction += PauseState;
            //PacketProcessor.Instance.MapChangedAction += MapChanged;
            //PacketProcessor.Instance.DisplayGeneralDataChanged += OnDisplayGeneralDataChanged;
            //PacketProcessor.Instance.OverloadedChanged += OnOverloadedChanged;

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += UpdateKeyboard;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
            //if (BasicTeraData.Instance.WindowData.EnableOverlay) DXrender = new D3D9Render.Renderer(); ///*** fix me
            //EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
            //Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
            //Chrono.ToolTip = LP.MainWindow_Only_boss;
            //History.Source = BasicTeraData.Instance.ImageDatabase.History.Source;
            //Config.Source = BasicTeraData.Instance.ImageDatabase.Config.Source;
            //Chatbox.Source = BasicTeraData.Instance.ImageDatabase.Chat.Source;
            //BossGageImg.Source = BasicTeraData.Instance.ImageDatabase.BossGage.Source;
            //HideNamesImage.Source = BasicTeraData.Instance.ImageDatabase.HideNicknames.Source;
            //UserPauseBtn.Source = BasicTeraData.Instance.ImageDatabase.Pause.Source;
            //HideNames.ToolTip = LP.Blur_player_names;
            ListEncounter.PreviewKeyDown += ListEncounterOnPreviewKeyDown;
            UpdateComboboxEncounter(new List<NpcEntity>(), null);
            //Title = "Shinra Meter V" + UpdateManager.Version;
            Topmost = BasicTeraData.Instance.WindowData.Topmost;
            ShowInTaskbar = !BasicTeraData.Instance.WindowData.Topmost;
            Scroller.MaxHeight = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed * 30;
            //_entityStats = new EntityStatsMain() { Scale = BasicTeraData.Instance.WindowData.DebuffsStatus.Scale, DontClose = true };
            //_bossGageBar = new BossGageWindow() { Scale = BasicTeraData.Instance.WindowData.BossGageStatus.Scale, DontClose = true, DataContext = DataContext };
            //_popupNotification = new PopupNotification() { DontClose = true };
            //_windowHistory = new TeradpsHistory(new ConcurrentDictionary<UploadData, NpcEntity>()) { Scale = BasicTeraData.Instance.WindowData.HistoryStatus.Scale, DontClose = true };
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            if (BasicTeraData.Instance.WindowData.ClickThrou) { SetClickThrou(); }
            GraphViewModel = new GraphViewModel();
            //power swith handle
            //SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged; // moved to App

            SettingsWindowViewModel.WindowScaleChanged += OnScaleChanged;

            App.SplashScreen?.CloseWindowSafe();

        }

        //private void OnMainWindowOpacityChanged(double v)
        //{
        //    InvokePropertyChanged(nameof(WindowOpacity));
        //}

        private void OnScaleChanged(double val)
        {
            Scale = val;
        }

        //private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        //{
        //    if (e.Mode != PowerModes.StatusChange)
        //        TeraSniffer.Instance.CleanupForcefully();
        //}

        //private void OnDisplayGeneralDataChanged(bool hide, EntityId eid)
        //{
        //    //if (hide)
        //    //{
        //    //    //_hideEid = eid;
        //    //    //_hideGeneralData = true;
        //    //    _bossGageBar.TempToggle(false);
        //    //}
        //    //else if (_hideEid == eid)
        //    //{
        //    //    //_hideGeneralData = false;
        //    //    _bossGageBar.TempToggle(true);
        //    //}
        //}

        //private void MapChanged()
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        //if (!_paused)
        //        //{
        //        //    _mapChanged = true;
        //        //    WaitingMapChange.Visibility = Visibility.Collapsed;
        //        //}
        //        //_hideGeneralData = false;

        //        //_bossGageBar.TempToggle(true);
        //    });
        //}

        public Dictionary<Player, PlayerStats> Controls { get; set; } = new Dictionary<Player, PlayerStats>();

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            App.WindowManager.SaveWindowsPos();
            Exit();
        }


        private void InstanceOnGuildIconAction(Bitmap icon)
        {
            //void ChangeUi(Bitmap bitmap)
            //{
                Dispatcher.Invoke(() =>
                {
                    Icon = icon?.ToImageSource() ?? BasicTeraData.Instance.ImageDatabase.Icon;
                    //_trayIcon.Icon = icon?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;
                });
            //}

            //Dispatcher.Invoke((PacketProcessor.GuildIconEvent)ChangeUi, icon);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            foreach (Window win in App.Current.Windows)
            {
                if (!(win is ClickThrouWindow ctw)) continue;

                ctw.DontClose = false;
                ctw.Close();
            }
            Exit();
            //must be removed at exit
            //SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private bool _needRefreshClickThrou = false;

        private void RefreshClickThrou()
        {
            if (!_needRefreshClickThrou) { return; }
            if (BasicTeraData.Instance.WindowData.ClickThrou)
            {
                SetClickThrou();
            }
            else
            {
                UnsetClickThrou();
            }
        }



        //public void VerifyClose(bool noConfirm = false)
        //{
        //    //SetForegroundWindow(new WindowInteropHelper(this).Handle); //todo: needed?
        //    Close();
        //}

        //internal void SaveWindowsPos()
        //{
        //    //BasicTeraData.Instance.WindowData.BossGageStatus = new WindowStatus(_bossGageBar.LastSnappedPoint ?? new Point(_bossGageBar.Left, _bossGageBar.Top), _bossGageBar.Visible, _bossGageBar.Scale);
        //    //BasicTeraData.Instance.WindowData.HistoryStatus = new WindowStatus(_windowHistory.LastSnappedPoint ?? new Point(_windowHistory.Left, _windowHistory.Top), _windowHistory.Visible, _windowHistory.Scale);
        //    //BasicTeraData.Instance.WindowData.DebuffsStatus = new WindowStatus(_entityStats.LastSnappedPoint ?? new Point(_entityStats.Left, _entityStats.Top), _entityStats.Visible, _entityStats.Scale);
        //    //BasicTeraData.Instance.WindowData.PopupNotificationLocation = _popupNotification.LastSnappedPoint ?? new Point(_popupNotification.Left, _popupNotification.Top);
        //}

        public void Exit()
        {
            BasicTeraData.Instance.WindowData.Location = LastSnappedPoint ?? new Point(Left, Top);
            ForceWindowVisibilityHidden = true;
            PacketProcessor.Instance.TickUpdated -= Update;
            _dispatcherTimer.Stop();
            //if (NotifyIcon.Tray != null) {
            //    NotifyIcon.Tray.Visibility = Visibility.Collapsed;
            //    NotifyIcon.Tray.Icon = null;
            //    NotifyIcon.Tray.IconSource = null;
            //    NotifyIcon.Tray.Dispose();
            //    NotifyIcon.Tray = null;
            //}
            //_trayIcon.Dispose();
            _topMost = false;
            PacketProcessor.Instance.Exit();
            DXrender?.Dispose();
            App.Dispose();
        }

        private void ListEncounterOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = true;
        }


        public void UpdateKeyboard(object o, EventArgs args)
        {
            var teraWindowActive = TeraWindow.IsTeraActive();
            var meterWindowActive = TeraWindow.IsMeterActive();
            if (KeyboardHook.Instance.SetHotkeys(teraWindowActive)) { StayTopMost(); }

            if (!BasicTeraData.Instance.WindowData.AlwaysVisible)
            {
                if (!teraWindowActive && !meterWindowActive)
                {
                    HideWindow(); //Visibility = Visibility.Hidden;
                    ForceWindowVisibilityHidden = true;
                }

                if ((meterWindowActive || teraWindowActive) && (BasicTeraData.Instance.WindowData.InvisibleUi && Controls.Count > 0 ||
                                                                !BasicTeraData.Instance.WindowData.InvisibleUi))
                {
                    ForceWindowVisibilityHidden = false;
                    ShowWindow(); //Visibility = Visibility.Visible;
                }
            }
            else { ForceWindowVisibilityHidden = false; }
        }

        public void Update(UiUpdateMessage nmessage)
        {
            void ChangeUi(UiUpdateMessage message)
            {
                QueuedPacketsLed.Fill = message.QueuedPackets > 1000 && message.QueuedPackets < 5000
                    ? Brushes.DarkOrange
                    : message.QueuedPackets >= 5000
                        ? new SolidColorBrush(Color.FromRgb(0xff, 30, 0x40))
                        : Brushes.Transparent;

                RefreshClickThrou();
                Scroller.MaxHeight = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed * 30;
                UpdateComboboxEncounter(message.Entities, message.StatsSummary.EntityInformation.Entity);
                //_entityStats.Update(message.StatsSummary.EntityInformation, message.Abnormals);
                //_windowHistory.Update(message.BossHistory);
                //_chatbox?.Update(message.Chatbox);
                //_popupNotification.AddNotification(message.Flash);

                PartyDps.Content = FormatHelpers.Instance.FormatValue(message.StatsSummary.EntityInformation.Interval == 0
                                       ? message.StatsSummary.EntityInformation.TotalDamage
                                       : message.StatsSummary.EntityInformation.TotalDamage * TimeSpan.TicksPerSecond / message.StatsSummary.EntityInformation.Interval) +
                                   LP.PerSecond;
                var visiblePlayerStats = new HashSet<Player>();
                var statsDamage = message.StatsSummary.PlayerDamageDealt;
                var statsHeal = message.StatsSummary.PlayerHealDealt;
                foreach (var playerStats in statsDamage)
                {
                    PlayerStats playerStatsControl;
                    Controls.TryGetValue(playerStats.Source, out playerStatsControl);
                    if (playerStats.Amount == 0) { continue; }

                    visiblePlayerStats.Add(playerStats.Source);
                    if (playerStatsControl != null) { continue; }
                    playerStatsControl = new PlayerStats(playerStats, statsHeal.FirstOrDefault(x => x.Source == playerStats.Source), message.StatsSummary.EntityInformation,
                        message.Skills, message.Abnormals.Get(playerStats.Source));
                    Controls.Add(playerStats.Source, playerStatsControl);
                }
                DXrender?.Draw(statsDamage.ToClassInfo(message.StatsSummary.EntityInformation.TotalDamage, message.StatsSummary.EntityInformation.Interval));

                var invisibleControls = Controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
                foreach (var invisibleControl in invisibleControls)
                {
                    Controls[invisibleControl.Key].CloseSkills();
                    Controls.Remove(invisibleControl.Key);
                }


                //SGrid.Visibility = !_hideGeneralData ? Visibility.Visible : Visibility.Collapsed;
                TotalDamage.Content = FormatHelpers.Instance.FormatValue(message.StatsSummary.EntityInformation.TotalDamage);

                if (BasicTeraData.Instance.WindowData.ShowTimeLeft && message.StatsSummary.EntityInformation.TimeLeft > 0)
                {
                    var interval = TimeSpan.FromSeconds(message.StatsSummary.EntityInformation.TimeLeft / TimeSpan.TicksPerSecond);
                    Timer.Content = interval.ToString(@"mm\:ss");
                    Timer.Foreground = Brushes.LightCoral;
                }
                else
                {
                    var interval = TimeSpan.FromSeconds(message.StatsSummary.EntityInformation.Interval / TimeSpan.TicksPerSecond);
                    Timer.Content = interval.ToString(@"mm\:ss");
                    if (message.StatsSummary.EntityInformation.Interval == 0 && BasicTeraData.Instance.WindowData.ShowTimeLeft) { Timer.Foreground = Brushes.LightCoral; }
                    else { Timer.Foreground = Brushes.White; }
                }
                Players.Items.Clear();

                foreach (var item in statsDamage)
                {
                    if (!Controls.ContainsKey(item.Source)) { continue; }
                    if (Players.Items.Contains(Controls[item.Source]))
                    {
                        BasicTeraData.LogError("duplicate playerinfo: \r\n" + string.Join("\r\n ", statsDamage.Select(x => x.Source.ToString() + " ->  " + x.Amount)), false, true);
                        continue;
                    }
                    Players.Items.Add(Controls[item.Source]);
                    Controls[item.Source].Repaint(item, statsHeal.FirstOrDefault(x => x.Source == item.Source), message.StatsSummary.EntityInformation, message.Skills,
                        message.Abnormals.Get(item.Source), message.TimedEncounter);
                }

                if (BasicTeraData.Instance.WindowData.InvisibleUi && !DC.Paused)
                {
                    if (Controls.Count > 0 && !ForceWindowVisibilityHidden && Visibility != Visibility.Visible) { ShowWindow(); } //Visibility = Visibility.Visible; }
                    if (Controls.Count == 0 && Visibility != Visibility.Hidden) { HideWindow(); } //Visibility = Visibility.Hidden; }
                }
                else if (!ForceWindowVisibilityHidden && Visibility != Visibility.Visible) { ShowWindow(); } //Visibility = Visibility.Visible; } 
                if (TeraWindow.IsTeraActive() && BasicTeraData.Instance.WindowData.Topmost)
                {
                    StayTopMost();
                }
                if (BasicTeraData.Instance.WindowData.RealtimeGraphEnabled)
                {
                    GraphViewModel.Update(message);
                    Graph.Visibility = Visibility.Visible;
                }
                else
                {
                    Graph.Visibility = Visibility.Collapsed;
                    GraphViewModel.Reset();
                }

            }

            Dispatcher.Invoke((PacketProcessor.UpdateUiHandler)ChangeUi, nmessage);
        }

        //public void ShowUploadHistory()
        //{
        //    //_windowHistory.ShowWindow();
        //}
        //private void ShowHistory(object sender, MouseButtonEventArgs e)
        //{
        //    e.Handled = true;
        //    ShowUploadHistory();
        //}

        //public void ShowChat()
        //{
        //    //e.Handled = true;
        //    if (_chatbox != null && _chatbox.IsVisible) return; // can only show one chatbox now
        //    _chatbox = new Chatbox { Owner = this };
        //    _chatbox.ShowWindow();
        //}

        public void OnConnected(string serverName)
        {
            //void ChangeTitle(string newServerName)
            //{
            //Title = newServerName;
            //Dispatcher.Invoke(() => _trayIcon.Text = $"Shinra Meter v{UpdateManager.Version}: {serverName}");
            //SnapToScreen();
            //}

            //Dispatcher.Invoke((ChangeTitle)ChangeTitle, serverName);
        }

        internal void StayTopMost()
        {
            if (!_topMost || !Topmost)
            {
                Debug.WriteLine("Not topmost");
                return;
            }
            foreach (Window window in Application.Current.Windows)
            {
                window.Topmost = false;
                window.Topmost = true;
            }
        }

        private bool NeedUpdateEncounter(IReadOnlyList<NpcEntity> entities)
        {
            if (entities.Count != ListEncounter.Items.Count - 1) { return true; }
            for (var i = 1; i < ListEncounter.Items.Count - 1; i++)
            {
                if ((NpcEntity)((ComboBoxItem)ListEncounter.Items[i]).Content != entities[i - 1]) { return true; }
            }
            return false;
        }

        private bool ChangeEncounterSelection(NpcEntity entity)
        {
            if (entity == null) { return false; }

            for (var i = 1; i < ListEncounter.Items.Count; i++)
            {
                if ((NpcEntity)((ComboBoxItem)ListEncounter.Items[i]).Content != entity) { continue; }
                ListEncounter.SelectedItem = ListEncounter.Items[i];
                return true;
            }
            return false;
        }


        private void UpdateComboboxEncounter(IReadOnlyList<NpcEntity> entities, NpcEntity currentBoss)
        {
            //http://stackoverflow.com/questions/12164488/system-reflection-targetinvocationexception-occurred-in-presentationframework
            if (ListEncounter == null || !ListEncounter.IsLoaded) { return; }

            if (!NeedUpdateEncounter(entities))
            {
                ChangeEncounterSelection(currentBoss);
                return;
            }

            NpcEntity selectedEntity = null;
            if ((ComboBoxItem)ListEncounter.SelectedItem != null && !(((ComboBoxItem)ListEncounter.SelectedItem).Content is string))
            {
                selectedEntity = (NpcEntity)((ComboBoxItem)ListEncounter.SelectedItem).Content;
            }

            ListEncounter.Items.Clear();
            ListEncounter.Items.Add(new ComboBoxItem { Content = LP.TotalEncounter });
            var selected = false;
            foreach (var entity in entities)
            {
                var item = new ComboBoxItem { Content = entity };
                ListEncounter.Items.Add(item);
                if (entity != selectedEntity) { continue; }
                ListEncounter.SelectedItem = item;
                selected = true;
            }

            if (ChangeEncounterSelection(currentBoss)) { return; }
            if (selected) { return; }
            ListEncounter.SelectedItem = ListEncounter.Items[0];
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

            //_entityStats.Owner = this;
            //_bossGageBar.Owner = this;
            //_windowHistory.Owner = this;
            if (BasicTeraData.Instance.WindowData.RememberPosition)
            {
                LastSnappedPoint = BasicTeraData.Instance.WindowData.Location;
                Left = LastSnappedPoint?.X ?? 0;
                Top = LastSnappedPoint?.Y ?? 0;
                if (!BasicTeraData.Instance.WindowData.RealtimeGraphEnabled) Graph.Visibility = Visibility.Collapsed;
                _dragged = true;
                SnapToScreen();
                //_popupNotification.LastSnappedPoint = BasicTeraData.Instance.WindowData.PopupNotificationLocation;
                //_popupNotification.Left = _popupNotification.LastSnappedPoint?.X ?? 0;
                //_popupNotification.Top = _popupNotification.LastSnappedPoint?.Y ?? 0;
                //_popupNotification.Show();
                //_popupNotification.Hide();
                //_entityStats.LastSnappedPoint = BasicTeraData.Instance.WindowData.DebuffsStatus.Location;
                //_entityStats.Left = _entityStats.LastSnappedPoint?.X ?? 0;
                //_entityStats.Top = _entityStats.LastSnappedPoint?.Y ?? 0;
                //_entityStats.Show();
                //_entityStats.Hide();
                //if (BasicTeraData.Instance.WindowData.DebuffsStatus.Visible) { _entityStats.ShowWindow(); }
                //_bossGageBar.LastSnappedPoint = BasicTeraData.Instance.WindowData.BossGageStatus.Location;
                //_bossGageBar.Left = _bossGageBar.LastSnappedPoint?.X ?? 0;
                //_bossGageBar.Top = _bossGageBar.LastSnappedPoint?.Y ?? 0;
                //_bossGageBar.Show();
                //_bossGageBar.Hide();
                //if (BasicTeraData.Instance.WindowData.BossGageStatus.Visible) { _bossGageBar.ShowWindow(); }
                //_windowHistory.LastSnappedPoint = BasicTeraData.Instance.WindowData.HistoryStatus.Location;
                //_windowHistory.Left = _windowHistory.LastSnappedPoint?.X ?? 0;
                //_windowHistory.Top = _windowHistory.LastSnappedPoint?.Y ?? 0;
                //_windowHistory.Show();
                //_windowHistory.Hide();
                //if (BasicTeraData.Instance.WindowData.HistoryStatus.Visible) { _windowHistory.ShowWindow(); }
                return;
            }
            Top = 0;
            Left = 0;

        }

        private void ListEncounter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) { return; }

            NpcEntity encounter = null;
            if (((ComboBoxItem)e.AddedItems[0]).Content is NpcEntity) { encounter = (NpcEntity)((ComboBoxItem)e.AddedItems[0]).Content; }

            if (encounter != PacketProcessor.Instance.Encounter) { PacketProcessor.Instance.NewEncounter = encounter; }
        }

        //private void EntityStatsImage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    e.Handled = true;
        //    //_entityStats.ShowWindow();
        //}

        //private void Chrono_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (PacketProcessor.Instance.TimedEncounter)
        //    {
        //        PacketProcessor.Instance.TimedEncounter = false;
        //        //Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
        //        Chrono.ToolTip = LP.MainWindow_Only_boss;
        //    }
        //    else
        //    {
        //        PacketProcessor.Instance.TimedEncounter = true;
        //        //Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chrono.Source;
        //        Chrono.ToolTip = LP.MainWindow_Boss_Adds;
        //    }
        //}

        private void ListEncounter_OnDropDownOpened(object sender, EventArgs e)
        {
            _topMost = false;
        }

        private void ListEncounter_OnDropDownClosed(object sender, EventArgs e)
        {
            //.NET 4.6 bug: https://connect.microsoft.com/VisualStudio/feedback/details/1660886/system-windows-controls-combobox-coerceisselectionboxhighlighted-bug
            if (Environment.OSVersion.Version.Major >= 10)
            {
                ListEncounter.GetType().GetField("_highlightedInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ListEncounter, null);
            }
            _topMost = true;
        }

        //private void Close_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        //{
        //    VerifyClose();
        //}

        //private void Config_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var style = FindResource("ShinraContext") as Style;
        //    //PopupMenu.Style = style;
        //    //PopupMenu.IsOpen = true;
        //}

        //public void PcapWarning(string str)
        //{
        //    BasicTeraData.LogError(str, false, true);
        //}

        private void ChangeTimeLeft(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) { return; }
            BasicTeraData.Instance.WindowData.ShowTimeLeft = !BasicTeraData.Instance.WindowData.ShowTimeLeft;
        }

        //private void ShowBossGage(object sender, MouseButtonEventArgs e)
        //{
        //    e.Handled = true;
        //    //_bossGageBar.ShowWindow();
        //}

        //public void PauseState(bool pause)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        //_paused = pause;
        //        //if (pause)
        //        //{
        //        //    //_mapChanged = false;
        //        //    //if (BasicTeraData.Instance.WindowData.UserPaused)
        //        //    //{
        //        //    //    //UserPaused.Visibility = Visibility.Visible;
        //        //    //    //TooSlow.Visibility = Visibility.Collapsed;
        //        //    //    //WaitingMapChange.Visibility = Visibility.Collapsed;
        //        //    //    //UserPauseBtn.Source = BasicTeraData.Instance.ImageDatabase.Play.Source;
        //        //    //}
        //        //    //else
        //        //    //{
        //        //    //    //UserPaused.Visibility = Visibility.Collapsed;
        //        //    //    //TooSlow.Visibility = Visibility.Visible;
        //        //    //    //WaitingMapChange.Visibility = Visibility.Visible;
        //        //    //    //BackgroundBorder.Background = Brushes.DarkRed;
        //        //    //    //UserPauseBtn.Source = BasicTeraData.Instance.ImageDatabase.Pause.Source;

        //        //    //}
        //        //}
        //        //else
        //        //{
        //        //    //UserPaused.Visibility = Visibility.Collapsed;
        //        //    //TooSlow.Visibility = Visibility.Collapsed;
        //        //    //WaitingMapChange.Visibility = Visibility.Collapsed;
        //        //    //BackgroundBorder.Background = (SolidColorBrush)App.Current.FindResource("KrBgColor");
        //        //    //UserPauseBtn.Source = BasicTeraData.Instance.ImageDatabase.Pause.Source;


        //        //    //if (!_mapChanged)
        //        //    //{
        //        //    //    WaitingMapChange.Visibility = Visibility.Visible;
        //        //    //}
        //        //}
        //    });

        //}

        //private delegate void ChangeTitle(string servername);

        //public ToggleButton HideNames4Binding { get => HideNames; }


        //private void UserPauseBtnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    BasicTeraData.Instance.WindowData.UserPaused = !BasicTeraData.Instance.WindowData.UserPaused;
        //    if (BasicTeraData.Instance.WindowData.UserPaused)
        //    {
        //        PacketProcessor.Instance.NeedPause = true;
        //    }

        //    PauseState(BasicTeraData.Instance.WindowData.UserPaused);
        //}

        private void OnGraphMouseLeave(object sender, MouseEventArgs e)
        {
            _topMost = true;
        }

        private void OnGraphMouseEnter(object sender, MouseEventArgs e)
        {
            _topMost = false;
        }

        #region Done

        private readonly DoubleAnimation _expandFooterAnim = new DoubleAnimation(31, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
        private readonly DoubleAnimation _shrinkFooterAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
        private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Footer.BeginAnimation(HeightProperty, _expandFooterAnim);
        }
        private void MainWindow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Footer.BeginAnimation(HeightProperty, _shrinkFooterAnim);
        }
        public new void SetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd.ToInt64() == 0)
            {
                _needRefreshClickThrou = true;
                return;
            }
            _needRefreshClickThrou = false;
            WindowsServices.SetWindowExTransparent(hwnd);
            foreach (var players in Controls) { players.Value.SetClickThrou(); }
            App.WindowManager.SetClickThrou();

            //_entityStats.SetClickThrou();
            //_popupNotification.SetClickThrou();
            //_bossGageBar.SetClickThrou();
            //EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStatsClickThrou.Source;
        }
        public new void UnsetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd.ToInt64() == 0)
            {
                _needRefreshClickThrou = true;
                return;
            }
            _needRefreshClickThrou = false;
            WindowsServices.SetWindowExVisible(hwnd);
            foreach (var players in Controls) { players.Value.UnsetClickThrou(); }

            App.WindowManager.UnsetClickThrou();
            //_entityStats.UnsetClickThrou();
            //_bossGageBar.UnsetClickThrou();
            //_popupNotification.UnsetClickThrou();
            //EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
        }


        #endregion
    }

    public static class Extensions
    {
        public static List<ClassInfo> ToClassInfo(this IEnumerable<PlayerDamageDealt> data, long sum, long interval)
        {
            // return linq expression method
            return data.Select(dealt => new ClassInfo
            {
                PName = $"{dealt.Source.Name}",
                PDmg = $"{FormatHelpers.Instance.FormatPercent((double)dealt.Amount / sum)}",
                PDsp =
                    $"{FormatHelpers.Instance.FormatValue(interval == 0 ? dealt.Amount : dealt.Amount * TimeSpan.TicksPerSecond / interval)}{LP.PerSecond}",
                PCrit = $"{Math.Round(dealt.CritRate)}%",
                PId = dealt.Source.PlayerId
            }).ToList();
        }
    }
}