using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DamageMeter.AutoUpdate;
using DamageMeter.Database.Structures;
using DamageMeter.Sniffing;
using DamageMeter.UI.EntityStats;
using DamageMeter.UI.HUD.Windows;
using Data;
using Data.Actions.Notify;
using Lang;
using DamageMeter.D3D9Render.TeraData;
using Tera.Game;
using Tera.Game.Abnormality;
using Application = System.Windows.Forms.Application;
using Brushes = System.Windows.Media.Brushes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using Microsoft.Win32;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly BossGageWindow _bossGageBar;
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly EntityStatsMain _entityStats;
        private readonly PopupNotification _popupNotification;

        public D3D9Render.Renderer DXrender;

        private readonly TeradpsHistory _windowHistory;
        internal Chatbox _chatbox;
        private bool _keyboardInitialized;
        private bool _topMost = true;
        private bool _paused = false;

        internal bool ForceWindowVisibilityHidden;
        //private readonly SystemTray _systemTray;

        public MainWindow()
        {
            InitializeComponent();
            // Handler for exceptions in threads behind forms.
            Application.ThreadException += GlobalThreadExceptionHandler;
            if (BasicTeraData.Instance.WindowData.InvisibleUi) { Visibility = Visibility.Hidden; }
            System.Windows.Application.Current.Resources["Scale"] = BasicTeraData.Instance.WindowData.Scale;
            if (BasicTeraData.Instance.WindowData.LowPriority) { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle; }


            TeraSniffer.Instance.Enabled = true;
            TeraSniffer.Instance.Warning += PcapWarning;
            NetworkController.Instance.Connected += HandleConnected;
            NetworkController.Instance.TickUpdated += Update;
            NetworkController.Instance.SetClickThrouAction += SetClickThrou;
            NetworkController.Instance.UnsetClickThrouAction += UnsetClickThrou;
            NetworkController.Instance.GuildIconAction += InstanceOnGuildIconAction;
            NetworkController.Instance.PauseAction += PauseState;
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += UpdateKeyboard;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
            if (BasicTeraData.Instance.WindowData.EnableOverlay) DXrender = new D3D9Render.Renderer();
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
            Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
            Chrono.ToolTip = LP.MainWindow_Only_boss;
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            HideNames.Source = BasicTeraData.Instance.ImageDatabase.HideNicknames.Source;
            ShowHotkeysIcon.Source = BasicTeraData.Instance.ImageDatabase.Hotkeys.Source;
            History.Source = BasicTeraData.Instance.ImageDatabase.History.Source;
            Config.Source = BasicTeraData.Instance.ImageDatabase.Config.Source;
            Chatbox.Source = BasicTeraData.Instance.ImageDatabase.Chat.Source;
            BossGageImg.Source = BasicTeraData.Instance.ImageDatabase.BossGage.Source;
            ListEncounter.PreviewKeyDown += ListEncounterOnPreviewKeyDown;
            UpdateComboboxEncounter(new List<NpcEntity>(), null);
            Title = "Shinra Meter V" + UpdateManager.Version;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.MainWindowOpacity;
            Topmost = BasicTeraData.Instance.WindowData.Topmost;
            ShowInTaskbar = !BasicTeraData.Instance.WindowData.Topmost;
            Scroller.MaxHeight = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed * 30;
            _entityStats = new EntityStatsMain(){ Scale = BasicTeraData.Instance.WindowData.DebuffsStatus.Scale, DontClose = true};
            _bossGageBar = new BossGageWindow() { Scale = BasicTeraData.Instance.WindowData.BossGageStatus.Scale, DontClose = true };
            _popupNotification = new PopupNotification() { DontClose = true };
            _windowHistory = new TeradpsHistory(new ConcurrentDictionary<string, NpcEntity>()) { Scale = BasicTeraData.Instance.WindowData.BossGageStatus.Scale, DontClose = true };

            Console.WriteLine("SuperUniqueStringEasilyDetectableToBeAbleToRandomizeTheProgramAndBypassSignatureBasedBlock");
            KeyboardHook.Instance.SwitchTopMost += delegate { NotifyIcon.SwitchStayTop(); };
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            NotifyIcon.Initialize(this);
            NotifyIcon.InitializeServerList(NetworkController.Instance.Initialize());
        }

        public Dictionary<Player, PlayerStats> Controls { get; set; } = new Dictionary<Player, PlayerStats>();
       
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            Exit();
        }


        private void InstanceOnGuildIconAction(Bitmap icon)
        {
            void ChangeUi(Bitmap bitmap)
            {
                Icon = bitmap?.ToImageSource() ?? BasicTeraData.Instance.ImageDatabase.Icon;
                NotifyIcon.Tray.Icon = bitmap?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;
            }

            Dispatcher.Invoke((NetworkController.GuildIconEvent) ChangeUi, icon);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Exit();
        }

        public new void SetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
            foreach (var players in Controls) { players.Value.SetClickThrou(); }
            _entityStats.SetClickThrou();
            _popupNotification.SetClickThrou();
            _bossGageBar.SetClickThrou();
            //NotifyIcon.ClickThrou.IsChecked = true;
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStatsClickThrou.Source;
        }

        public new void UnsetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExVisible(hwnd);
            foreach (var players in Controls) { players.Value.UnsetClickThrou(); }
            _entityStats.UnsetClickThrou();
            _bossGageBar.UnsetClickThrou();
            _popupNotification.UnsetClickThrou();
            //NotifyIcon.ClickThrou.IsChecked = false;
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
        }


        public void VerifyClose()
        {
            SetForegroundWindow(new WindowInteropHelper(this).Handle);
            if (MessageBox.Show(LP.MainWindow_Do_you_want_to_close_the_application, LP.MainWindow_Close_Shinra_Meter_V + UpdateManager.Version,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) { return; }
            BasicTeraData.Instance.WindowData.BossGageStatus =
                new WindowStatus(_bossGageBar.LastSnappedPoint ?? new Point(_bossGageBar.Left, _bossGageBar.Top), _bossGageBar.Visible, _bossGageBar.Scale);
            BasicTeraData.Instance.WindowData.HistoryStatus = new WindowStatus(_windowHistory.LastSnappedPoint ?? new Point(_windowHistory.Left, _windowHistory.Top),
                _windowHistory.Visible, _windowHistory.Scale);
            BasicTeraData.Instance.WindowData.DebuffsStatus = new WindowStatus(_entityStats.LastSnappedPoint ?? new Point(_entityStats.Left, _entityStats.Top),
                _entityStats.Visible, _entityStats.Scale);
            BasicTeraData.Instance.WindowData.PopupNotificationLocation = _popupNotification.LastSnappedPoint ??
                                                                          new Point(_popupNotification.Left, _popupNotification.Top);
            Close();
        }



        public void Exit()
        {
            BasicTeraData.Instance.WindowData.Location = LastSnappedPoint ?? new Point(Left, Top);
            ForceWindowVisibilityHidden = true;
            NetworkController.Instance.TickUpdated -= Update;
            _dispatcherTimer.Stop();
            NotifyIcon.Tray.Visibility = Visibility.Collapsed;
            NotifyIcon.Tray.Icon = null;
            NotifyIcon.Tray.IconSource = null;
            NotifyIcon.Tray.Dispose();
            NotifyIcon.Tray = null;
            _topMost = false;
            NetworkController.Instance.Exit();
            DXrender?.Dispose();
        }

        private void ListEncounterOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = true;
        }

        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            BasicTeraData.LogError("##### FORM EXCEPTION #####\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data +
                                   "\r\n" + ex.InnerException + "\r\n" + ex.TargetSite);
            MessageBox.Show(LP.MainWindow_Fatal_error);
        }

        public void UpdateKeyboard(object o, EventArgs args)
        {
            var teraWindowActive = TeraWindow.IsTeraActive();
            var meterWindowActive = TeraWindow.IsMeterActive();
            if (!_keyboardInitialized)
            {
                KeyboardHook.Instance.RegisterKeyboardHook();
                _keyboardInitialized = true;
            }
            
            else { if (KeyboardHook.Instance.SetHotkeys(teraWindowActive)) { StayTopMost(); } }

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
                Scroller.MaxHeight = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed * 30;
                UpdateComboboxEncounter(message.Entities, message.StatsSummary.EntityInformation.Entity);
                _entityStats.Update(message.StatsSummary.EntityInformation, message.Abnormals);
                _windowHistory.Update(message.BossHistory);
                _chatbox?.Update(message.Chatbox);
                _popupNotification.AddNotification(message.Flash);

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
                        BasicTeraData.LogError(
                            "duplicate playerinfo: \r\n" + string.Join("\r\n ", statsDamage.Select(x => x.Source.ToString() + " ->  " + x.Amount)), false, true);
                        continue;
                    }
                    Players.Items.Add(Controls[item.Source]);
                    Controls[item.Source].Repaint(item, statsHeal.FirstOrDefault(x => x.Source == item.Source), message.StatsSummary.EntityInformation, message.Skills,
                        message.Abnormals.Get(item.Source), message.TimedEncounter);
                }

                if (BasicTeraData.Instance.WindowData.InvisibleUi && !_paused)
                {
                    if (Controls.Count > 0 && !ForceWindowVisibilityHidden && Visibility != Visibility.Visible) { ShowWindow(); } //Visibility = Visibility.Visible; }
                    if (Controls.Count == 0 && Visibility != Visibility.Hidden) { HideWindow(); } //Visibility = Visibility.Hidden; }
                }
                else if (!ForceWindowVisibilityHidden && Visibility != Visibility.Visible) { ShowWindow(); } //Visibility = Visibility.Visible; } 
                if (TeraWindow.IsTeraActive() && BasicTeraData.Instance.WindowData.Topmost)
                {
                    StayTopMost();
                }
            }

            Dispatcher.Invoke((NetworkController.UpdateUiHandler) ChangeUi, nmessage);
        }

        private void ShowHistory(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _windowHistory.ShowWindow();
        }

        private void ShowChat(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _chatbox = new Chatbox {Owner = this};
            _chatbox.ShowWindow();
        }

        public void HandleConnected(string serverName)
        {
            void ChangeTitle(string newServerName)
            {
                Title = newServerName;
                NotifyIcon.Tray.ToolTipText = "Shinra Meter V" + UpdateManager.Version + ": " + newServerName;
                SnapToScreen();
            }

            Dispatcher.Invoke((ChangeTitle) ChangeTitle, serverName);
        }

        internal void StayTopMost()
        {
            if (!_topMost || !Topmost) {
                Debug.WriteLine("Not topmost");
                return;
            }
            foreach (Window window in System.Windows.Application.Current.Windows)
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
                if ((NpcEntity) ((ComboBoxItem) ListEncounter.Items[i]).Content != entities[i - 1]) { return true; }
            }
            return false;
        }

        private bool ChangeEncounterSelection(NpcEntity entity)
        {
            if (entity == null) { return false; }

            for (var i = 1; i < ListEncounter.Items.Count; i++)
            {
                if ((NpcEntity) ((ComboBoxItem) ListEncounter.Items[i]).Content != entity) { continue; }
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
            if ((ComboBoxItem) ListEncounter.SelectedItem != null && !(((ComboBoxItem) ListEncounter.SelectedItem).Content is string))
            {
                selectedEntity = (NpcEntity) ((ComboBoxItem) ListEncounter.SelectedItem).Content;
            }

            ListEncounter.Items.Clear();
            ListEncounter.Items.Add(new ComboBoxItem {Content = LP.TotalEncounter});
            var selected = false;
            foreach (var entity in entities)
            {
                var item = new ComboBoxItem {Content = entity};
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
            _entityStats.Owner = this;
            _bossGageBar.Owner = this;
            _windowHistory.Owner = this;
            if (BasicTeraData.Instance.WindowData.RememberPosition)
            {
                LastSnappedPoint=BasicTeraData.Instance.WindowData.Location;
                Left = LastSnappedPoint?.X ?? 0;
                Top = LastSnappedPoint?.Y ?? 0;
                _dragged = true;
                SnapToScreen();
                _popupNotification.LastSnappedPoint = BasicTeraData.Instance.WindowData.PopupNotificationLocation;
                _popupNotification.Left = _popupNotification.LastSnappedPoint?.X ?? 0;
                _popupNotification.Top = _popupNotification.LastSnappedPoint?.Y ?? 0;
                _popupNotification.Show();
                _popupNotification.Hide();
                _entityStats.LastSnappedPoint = BasicTeraData.Instance.WindowData.DebuffsStatus.Location;
                _entityStats.Left = _entityStats.LastSnappedPoint?.X ?? 0;
                _entityStats.Top = _entityStats.LastSnappedPoint?.Y ?? 0;
                _entityStats.Show();
                _entityStats.Hide();
                if (BasicTeraData.Instance.WindowData.DebuffsStatus.Visible) { _entityStats.ShowWindow(); }
                _bossGageBar.LastSnappedPoint = BasicTeraData.Instance.WindowData.BossGageStatus.Location;
                _bossGageBar.Left = _bossGageBar.LastSnappedPoint?.X ?? 0;
                _bossGageBar.Top = _bossGageBar.LastSnappedPoint?.Y ?? 0;
                _bossGageBar.Show();
                _bossGageBar.Hide();
                if (BasicTeraData.Instance.WindowData.BossGageStatus.Visible) { _bossGageBar.ShowWindow(); }
                _windowHistory.LastSnappedPoint = BasicTeraData.Instance.WindowData.HistoryStatus.Location;
                _windowHistory.Left = _windowHistory.LastSnappedPoint?.X ?? 0;
                _windowHistory.Top = _windowHistory.LastSnappedPoint?.Y ?? 0;
                _windowHistory.Show();
                _windowHistory.Hide();
                if (BasicTeraData.Instance.WindowData.HistoryStatus.Visible) { _windowHistory.ShowWindow(); }
                return;
            }
            Top = 0;
            Left = 0;
        }

        private void ListEncounter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) { return; }

            NpcEntity encounter = null;
            if (((ComboBoxItem) e.AddedItems[0]).Content is NpcEntity) { encounter = (NpcEntity) ((ComboBoxItem) e.AddedItems[0]).Content; }

            if (encounter != NetworkController.Instance.Encounter) { NetworkController.Instance.NewEncounter = encounter; }
        }

        private void EntityStatsImage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _entityStats.ShowWindow();
        }

        private void Chrono_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NetworkController.Instance.TimedEncounter)
            {
                NetworkController.Instance.TimedEncounter = false;
                Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
                Chrono.ToolTip = LP.MainWindow_Only_boss;
            }
            else
            {
                NetworkController.Instance.TimedEncounter = true;
                Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chrono.Source;
                Chrono.ToolTip = LP.MainWindow_Boss_Adds;
            }
        }

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

        private void Close_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            VerifyClose();
        }

        private void Config_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var style = FindResource("ShinraContext") as Style;
            PopupMenu.Style = style;
            PopupMenu.IsOpen = true;
        }

        public void PcapWarning(string str)
        {
            BasicTeraData.LogError(str, false, true);
        }

        private void ChangeTimeLeft(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) { return; }
            BasicTeraData.Instance.WindowData.ShowTimeLeft = !BasicTeraData.Instance.WindowData.ShowTimeLeft;
        }

        private void ShowBossGage(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _bossGageBar.ShowWindow();
        }
        private void ShowHotkeysInfo(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _bossGageBar.ShowWindow();
        }

        public void PauseState(bool pause)
        {
            Dispatcher.Invoke(() =>
            {
                _paused = pause;
                if (pause)
                {
                    BackgroundColor.Background = Brushes.DarkRed;
                    TooSlow.Visibility = Visibility.Visible;
                }
                else
                {
                    BackgroundColor.Background = (SolidColorBrush) App.Current.FindResource("DarkBarColor");
                    TooSlow.Visibility = Visibility.Collapsed;
                }
            });
        }

        private delegate void ChangeTitle(string servername);

        private void HideNames_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Controls.Count == 0) return;
            foreach (var it in Controls)
            {
                it.Value.SwitchHiddenMode();
            }
        }
    }

    public static class Extensions
    {
        public static List<ClassInfo> ToClassInfo(this IEnumerable<PlayerDamageDealt> data, long sum, long interval)
        {
            // return linq expression method
            return data.Select(dealt => new ClassInfo
            {
                PName = $"{dealt.Source.Name}",
                PDmg = $"{FormatHelpers.Instance.FormatPercent((double)dealt.Amount/sum)}",
                PDsp =
                    $"{FormatHelpers.Instance.FormatValue(interval == 0 ? dealt.Amount : dealt.Amount * TimeSpan.TicksPerSecond / interval)}{LP.PerSecond}",
                PCrit = $"{Math.Round(dealt.CritRate)}%",
                PId = dealt.Source.PlayerId
            }).ToList();
        }
    }
}