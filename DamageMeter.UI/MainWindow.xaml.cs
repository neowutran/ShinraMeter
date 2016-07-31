﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using DamageMeter.AutoUpdate;
using DamageMeter.Database.Structures;
using DamageMeter.Sniffing;
using DamageMeter.UI.EntityStats;
using Data;
using log4net;
using Tera.Game;
using Tera.Game.Abnormality;
using Application = System.Windows.Forms.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Chatbox _chatbox;
        private readonly EntityStatsMain _entityStats;

        private readonly TeradpsHistory _windowHistory;
        internal bool ForceWindowVisibilityHidden;
        private bool _keyboardInitialized;
        private bool _topMost = true;
        private readonly SystemTray _systemTray;

        public MainWindow()
        {
            InitializeComponent();
            var currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            // Handler for exceptions in threads behind forms.
            Application.ThreadException += GlobalThreadExceptionHandler;
            if (BasicTeraData.Instance.WindowData.InvisibleUi)
            {
                Visibility = Visibility.Hidden;
            }
            System.Windows.Application.Current.Resources["Scale"] = BasicTeraData.Instance.WindowData.Scale;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            TeraSniffer.Instance.Enabled = true;
            NetworkController.Instance.Connected += HandleConnected;
            NetworkController.Instance.TickUpdated += Update;
            NetworkController.Instance.SetClickThrouAction += SetClickThrou;
            NetworkController.Instance.UnsetClickThrouAction += UnsetClickThrou;
            NetworkController.Instance.GuildIconAction += InstanceOnGuildIconAction;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += UpdateKeyboard;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
            Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
            Chrono.ToolTip = "Only boss";
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            History.Source = BasicTeraData.Instance.ImageDatabase.History.Source;
            Config.Source = BasicTeraData.Instance.ImageDatabase.Config.Source;
            Chatbox.Source = BasicTeraData.Instance.ImageDatabase.Chat.Source;
            ListEncounter.PreviewKeyDown += ListEncounterOnPreviewKeyDown;
            UpdateComboboxEncounter(new List<NpcEntity>(), null);
            Title = "Shinra Meter V" + UpdateManager.Version;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.MainWindowOpacity;
            Closing -= ClickThrouWindow_Closing;
            Topmost = BasicTeraData.Instance.WindowData.Topmost;
            _entityStats = new EntityStatsMain(this);
            _chatbox = new Chatbox(new List<ChatMessage>());
            _windowHistory = new TeradpsHistory(new ConcurrentDictionary<string, NpcEntity>());
            Console.WriteLine(
                "SuperUniqueStringEasilyDetectableToBeAbleToRandomizeTheProgramAndBypassSignatureBasedBlock");
            _systemTray = new SystemTray(this);
            KeyboardHook.Instance.SwitchTopMost += delegate { _systemTray.SwitchStayTop(); };
        }

        private void InstanceOnGuildIconAction(Bitmap icon)
        {

            NetworkController.GuildIconEvent changeUi =
              delegate (Bitmap bitmap)
              {
                  Icon = bitmap?.ToImageSource() ?? BasicTeraData.Instance.ImageDatabase.Icon;
                  _systemTray.TrayIcon.Icon = bitmap?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;

              };
            Dispatcher.Invoke(changeUi, icon);
            
        }

        public Dictionary<Player, PlayerStats> Controls { get; set; } = new Dictionary<Player, PlayerStats>();

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Exit();
        }

        public void SetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
            foreach (var players in Controls)
            {
                players.Value.SetClickThrou();
            }
            _entityStats.SetClickThrou();
            _systemTray.ClickThrou.Checked = true;
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStatsClickThrou.Source;
        }

        public void UnsetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExVisible(hwnd);
            foreach (var players in Controls)
            {
                players.Value.UnsetClickThrou();
            }
            _entityStats.UnsetClickThrou();
            _systemTray.ClickThrou.Checked = false;
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
        }


        public void VerifyClose()
        {
            SetForegroundWindow(new WindowInteropHelper(this).Handle);
            if (MessageBox.Show("Do you want to close the application?", "Close Shinra Meter V" + UpdateManager.Version,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        public void Exit()
        {
            _systemTray.TrayIcon.Visible = false;
            _systemTray.TrayIcon.Icon = null;
            _systemTray.TrayIcon.Dispose();
            BasicTeraData.Instance.WindowData.Location = new Point(Left, Top);
            NetworkController.Instance.Exit();
        }

        private void ListEncounterOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = true;
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            LogError("##### CRASH (version=" + UpdateManager.Version + "): #####\r\n" + ex.Message + "\r\n" +
                     ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException +
                     "\r\n" + ex.TargetSite);
            MessageBox.Show("An fatal error has be found, please see the error.log file for more informations.");
        }

        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            LogError("##### FORM EXCEPTION (version=" + UpdateManager.Version + "): #####\r\n" + ex.Message + "\r\n" +
                     ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException +
                     "\r\n" + ex.TargetSite);
            MessageBox.Show("An fatal error has be found, please see the error.log file for more informations.");
        }

        private static void LogError(string error)
        {
            try
            {
                var log = LogManager.GetLogger(typeof(Program));
                log.Error(error);
                if (!BasicTeraData.Instance.WindowData.Debug)
                {
                    return;
                }

                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("error", error)
                    });

                    var response = client.PostAsync("http://diclah.com/~yukikoo/debug/debug.php", formContent);
                    var responseString = response.Result.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString.Result);
                }
            }
            catch
            {
                // Ignore
            }
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
            else
            {
                if (KeyboardHook.Instance.SetHotkeys(teraWindowActive))
                {
                    StayTopMost();
                }
            }

            if (!BasicTeraData.Instance.WindowData.AlwaysVisible)
            {
                if (!teraWindowActive && !meterWindowActive)
                {
                    Visibility = Visibility.Hidden;
                    ForceWindowVisibilityHidden = true;
                }

                if ((meterWindowActive || teraWindowActive) &&
                    ((BasicTeraData.Instance.WindowData.InvisibleUi && Controls.Count > 0) ||
                     !BasicTeraData.Instance.WindowData.InvisibleUi))
                {
                    ForceWindowVisibilityHidden = false;
                    Visibility = Visibility.Visible;
                }
            }
            else
            {
                ForceWindowVisibilityHidden = false;
            }
        }

        public void Update(StatsSummary nstatsSummary, Database.Structures.Skills nskills, List<NpcEntity> nentities,
            bool ntimedEncounter, AbnormalityStorage nabnormals,
            ConcurrentDictionary<string, NpcEntity> nbossHistory, List<ChatMessage> nchatbox, int npacketWaiting)
        {
            NetworkController.UpdateUiHandler changeUi =
                delegate(StatsSummary statsSummary, Database.Structures.Skills skills, List<NpcEntity> entities,
                    bool timedEncounter,
                    AbnormalityStorage abnormals, ConcurrentDictionary<string, NpcEntity> bossHistory,
                    List<ChatMessage> chatbox, int packetWaiting)
                {
                    UpdateComboboxEncounter(entities, statsSummary.EntityInformation.Entity);
                    _entityStats.Update(statsSummary.EntityInformation, abnormals);
                    _windowHistory.Update(bossHistory);
                    _chatbox.Update(chatbox);
                    _systemTray.UpdatePacketWaiting(packetWaiting);

                    PartyDps.Content =
                        FormatHelpers.Instance.FormatValue(statsSummary.EntityInformation.Interval == 0
                            ? statsSummary.EntityInformation.TotalDamage
                            : statsSummary.EntityInformation.TotalDamage*TimeSpan.TicksPerSecond/
                              statsSummary.EntityInformation.Interval) + "/s";
                    var visiblePlayerStats = new HashSet<Player>();
                    var statsDamage = statsSummary.PlayerDamageDealt;
                    var statsHeal = statsSummary.PlayerHealDealt;
                    var counter = 0;
                    foreach (var playerStats in statsDamage)
                    {
                        PlayerStats playerStatsControl;
                        Controls.TryGetValue(playerStats.Source, out playerStatsControl);
                        if (playerStats.Amount == 0)
                        {
                            continue;
                        }

                        if (counter == 9)
                        {
                            break;
                        }
                        counter++;

                        visiblePlayerStats.Add(playerStats.Source);
                        if (playerStatsControl != null) continue;
                        playerStatsControl = new PlayerStats(playerStats,
                            statsHeal.FirstOrDefault(x => x.Source == playerStats.Source),
                            statsSummary.EntityInformation, skills, abnormals.Get(playerStats.Source));
                        Controls.Add(playerStats.Source, playerStatsControl);                     
                    }

                    var invisibleControls = Controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
                    foreach (var invisibleControl in invisibleControls)
                    {
                        Controls[invisibleControl.Key].CloseSkills();
                        Controls.Remove(invisibleControl.Key);
                    }

                    TotalDamage.Content = FormatHelpers.Instance.FormatValue(statsSummary.EntityInformation.TotalDamage);
                    var interval = TimeSpan.FromSeconds(statsSummary.EntityInformation.Interval/TimeSpan.TicksPerSecond);
                    Timer.Content = interval.ToString(@"mm\:ss");

                    Players.Items.Clear();

                    foreach (var item in statsDamage)
                    {
                        if (!Controls.ContainsKey(item.Source)) continue;
                        Players.Items.Add(Controls[item.Source]);
                        Controls[item.Source].Repaint(item,
                            statsHeal.FirstOrDefault(x => x.Source == item.Source),
                        statsSummary.EntityInformation, skills, abnormals.Get(item.Source), timedEncounter);
                    }

                    if (BasicTeraData.Instance.WindowData.InvisibleUi)
                    {
                        if (Controls.Count > 0 && !ForceWindowVisibilityHidden)
                        {
                            Visibility = Visibility.Visible;
                        }
                        if (Controls.Count == 0)
                        {
                            Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        if (!ForceWindowVisibilityHidden)
                        {
                            Visibility = Visibility.Visible;
                        }
                    }
                   
                };
            Dispatcher.Invoke(changeUi, nstatsSummary, nskills, nentities, ntimedEncounter, nabnormals, nbossHistory,
                nchatbox, npacketWaiting);
        }

        private void ShowHistory(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _windowHistory.Show();
        }

        private void ShowChat(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _chatbox.Show();
        }

        public void HandleConnected(string serverName)
        {
            ChangeTitle changeTitle = delegate(string newServerName)
            {
                Title = newServerName;
                _systemTray.TrayIcon.Text = "Shinra Meter V" + UpdateManager.Version + ": " + newServerName;
            };
            Dispatcher.Invoke(changeTitle, serverName);
        }

        internal void StayTopMost()
        {
            if (!Topmost || !_topMost) return;
            Topmost = false;
            Topmost = true;
        }

        private bool NeedUpdateEncounter(IReadOnlyList<NpcEntity> entities)
        {
            if (entities.Count != ListEncounter.Items.Count - 1)
            {
                return true;
            }
            for (var i = 1; i < ListEncounter.Items.Count - 1; i++)
            {
                if ((NpcEntity) ((ComboBoxItem) ListEncounter.Items[i]).Content != entities[i - 1])
                {
                    return true;
                }
            }
            return false;
        }

        private bool ChangeEncounterSelection(NpcEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            for (var i = 1; i < ListEncounter.Items.Count; i++)
            {
                if ((NpcEntity) ((ComboBoxItem) ListEncounter.Items[i]).Content != entity) continue;
                ListEncounter.SelectedItem = ListEncounter.Items[i];
                return true;
            }
            return false;
        }


        private void UpdateComboboxEncounter(IReadOnlyList<NpcEntity> entities, NpcEntity currentBoss)
        {
            //http://stackoverflow.com/questions/12164488/system-reflection-targetinvocationexception-occurred-in-presentationframework
            if (ListEncounter == null || !ListEncounter.IsLoaded)
            {
                return;
            }

            if (!NeedUpdateEncounter(entities))
            {
                ChangeEncounterSelection(currentBoss);
                return;
            }

            NpcEntity selectedEntity = null;
            if ((ComboBoxItem) ListEncounter.SelectedItem != null &&
                !(((ComboBoxItem) ListEncounter.SelectedItem).Content is string))
            {
                selectedEntity = (NpcEntity) ((ComboBoxItem) ListEncounter.SelectedItem).Content;
            }

            ListEncounter.Items.Clear();
            ListEncounter.Items.Add(new ComboBoxItem {Content = "TOTAL"});
            var selected = false;
            foreach (var entity in entities)
            {
                var item = new ComboBoxItem {Content = entity};
                ListEncounter.Items.Add(item);
                if (entity != selectedEntity) continue;
                ListEncounter.SelectedItem = item;
                selected = true;
            }
            if (ChangeEncounterSelection(currentBoss))
            {
                return;
            }

            if (selected) return;
            ListEncounter.SelectedItem = ListEncounter.Items[0];
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (BasicTeraData.Instance.WindowData.RememberPosition)
            {
                Top = BasicTeraData.Instance.WindowData.Location.Y;
                Left = BasicTeraData.Instance.WindowData.Location.X;
                return;
            }
            Top = 0;
            Left = 0;
        }

        private void ListEncounter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;

            NpcEntity encounter = null;
            if (((ComboBoxItem) e.AddedItems[0]).Content is NpcEntity)
            {
                encounter = (NpcEntity) ((ComboBoxItem) e.AddedItems[0]).Content;
            }

            if (encounter != NetworkController.Instance.Encounter)
            {
                NetworkController.Instance.NewEncounter = encounter;
            }
        }

        public void CloseEntityStats()
        {
            _entityStats.Hide();
            _entityStats.Topmost = false;
        }

        private void EntityStatsImage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _entityStats.Show();
            _entityStats.Topmost = false;
            _entityStats.Topmost = true;
        }

        private void Chrono_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NetworkController.Instance.TimedEncounter)
            {
                NetworkController.Instance.TimedEncounter = false;
                Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
                Chrono.ToolTip = "Only boss";
            }
            else
            {
                NetworkController.Instance.TimedEncounter = true;
                Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chrono.Source;
                Chrono.ToolTip = "Boss + Adds";
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
                ListEncounter.GetType()
                    .GetField("_highlightedInfo", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(ListEncounter, null);
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


        private delegate void ChangeTitle(string servername);
    }
}