using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DamageMeter.AutoUpdate;
using DamageMeter.Sniffing;
using DamageMeter.UI.EntityStats;
using Data;
using log4net;
using Application = System.Windows.Forms.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly EntityStatsMain _entityStats;
        private bool _keyboardInitialized;

        private NotifyIcon _trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            var currentDomain = default(AppDomain);
            currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            // Handler for exceptions in threads behind forms.
            Application.ThreadException += GlobalThreadExceptionHandler;
            if (BasicTeraData.Instance.WindowData.InvisibleUI)
            {
                Visibility = Visibility.Hidden;
            }
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            TeraSniffer.Instance.Enabled = true;
            NetworkController.Instance.Connected += HandleConnected;
            NetworkController.Instance.TickUpdated += Update;
            NetworkController.Instance.SetClickThrouAction += SetClickThrou;
            NetworkController.Instance.UnsetClickThrouAction += UnsetClickThrou;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += UpdateKeyboard;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            PinImage.Source = BasicTeraData.Instance.ImageDatabase.UnPin.Source;
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
            Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chrono.Source;
            ListEncounter.PreviewKeyDown += ListEncounterOnPreviewKeyDown;
            UpdateComboboxEncounter(new LinkedList<Entity>(), null);
            Title = "Shinra Meter V" + UpdateManager.Version;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.MainWindowOpacity;
            _entityStats = new EntityStatsMain(this);
            TrayConfiguration();
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
            _clickThrou.Text = "Desactivate click throu";
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
            _clickThrou.Text = "Activate click throu";
            EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
        }

        public Dictionary<PlayerInfo, PlayerStats> Controls { get; set; } = new Dictionary<PlayerInfo, PlayerStats>();


        private void TrayConfiguration()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = BasicTeraData.Instance.ImageDatabase.Tray,
                Visible = true,
                Text = "Shinra Meter V" + UpdateManager.Version + ": No server"
            };
            _trayIcon.Click += TrayIconOnClick;


            var reset = new MenuItem {Text = "Reset"};
            reset.Click += ResetOnClick;
            var exit = new MenuItem {Text = "Close"};
            exit.Click += ExitOnClick;
            var wiki = new MenuItem {Text = "Wiki"};
            wiki.Click += WikiOnClick;
            var patch = new MenuItem {Text = "Patch note"};
            patch.Click += PatchOnClick;
            var issues = new MenuItem {Text = "Report issue"};

            issues.Click += IssuesOnClick;
            var forum = new MenuItem {Text = "Forum"};

            forum.Click += ForumOnClick;
            _clickThrou = new MenuItem {Text = "Activate click throu"};
            _clickThrou.Click += ClickThrouOnClick;


            var context = new ContextMenu();
            context.MenuItems.Add(_clickThrou);
            context.MenuItems.Add(reset);
            context.MenuItems.Add(exit);
            context.MenuItems.Add(wiki);
            context.MenuItems.Add(patch);
            context.MenuItems.Add(issues);
            context.MenuItems.Add(forum);


            _trayIcon.ContextMenu = context;
        }

        private MenuItem _clickThrou;

        private static void ClickThrouOnClick(object sender, EventArgs eventArgs)
        {
            NetworkController.Instance.SwitchClickThrou();
        }

        private void PatchOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki/Patch-note");
        }

        private void ExitOnClick(object sender, EventArgs eventArgs)
        {
            VerifyClose();
        }

        private void ResetOnClick(object sender, EventArgs eventArgs)
        {
            NetworkController.Instance.Reset();
        }

        private void ForumOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe",
                "http://forum.teratoday.com/topic/1316-shinrameter-open-source-tera-dps-meter/");
        }

        private void IssuesOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/issues");
        }

        private void WikiOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki");
        }

        private void TrayIconOnClick(object sender, EventArgs eventArgs)
        {
            var e = (MouseEventArgs) eventArgs;
            if (e.Button.ToString() == "Right")
            {
                return;
            }
            var invisibleUi = BasicTeraData.Instance.WindowData.InvisibleUI;
            BasicTeraData.Instance.WindowData.InvisibleUI = !invisibleUi;

            if (invisibleUi)
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                Visibility = Controls.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public void VerifyClose()
        {
            if (MessageBox.Show("Do you want to close the application?", "Close Shinra Meter V" + UpdateManager.Version,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        public void Exit()
        {
            _trayIcon.Visible = false;
            _trayIcon.Icon = null;
            _trayIcon.Dispose();
            BasicTeraData.Instance.WindowData.Location = new Point(Left, Top);
            NetworkController.Instance.Exit();
        }

        private void ListEncounterOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = true;
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An fatal error has be found, please see the error.log file for more informations.");
            var ex = default(Exception);
            ex = (Exception) e.ExceptionObject;
            var log = LogManager.GetLogger(typeof (Program));
            log.Error("##### CRASH (version=" + UpdateManager.Version + "): #####\r\n" + ex.Message + "\r\n" +
                      ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException +
                      "\r\n" + ex.TargetSite);
        }

        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("An fatal error has be found, please see the error.log file for more informations.");
            var ex = default(Exception);
            ex = e.Exception;
            var log = LogManager.GetLogger(typeof (Program)); //Log4NET
            log.Error("##### FORM EXCEPTION (version=" + UpdateManager.Version + "): #####\r\n" + ex.Message + "\r\n" +
                      ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException +
                      "\r\n" + ex.TargetSite);
        }

        public void UpdateKeyboard(object o, EventArgs args)
        {
            if (!_keyboardInitialized)
            {
                KeyboardHook.Instance.RegisterKeyboardHook();
                _keyboardInitialized = true;
            }
            else
            {
                KeyboardHook.Instance.SetHotkeys(TeraWindow.IsTeraActive());
            }
        }

        public void Update(long nfirstHit, long nlastHit, long ntotalDamage, Dictionary<Entity, EntityInfo> nentities,
            List<PlayerInfo> nstats, Entity ncurrentBoss)
        {
            UpdateUi changeUi =
                delegate(long firstHit, long lastHit, long totalDamage, Dictionary<Entity, EntityInfo> entities,
                    List<PlayerInfo> stats, Entity currentBoss)
                {
                    StayTopMost();
                    var entitiesStats = entities.ToList().OrderByDescending(e => e.Value.LastHit).ToList();
                    var encounterList = new LinkedList<Entity>();
                    foreach (var entityStats in entitiesStats)
                    {
                        encounterList.AddLast(entityStats.Key);
                    }
                    UpdateComboboxEncounter(encounterList, currentBoss);
                    _entityStats.Update(entities);
                    var visiblePlayerStats = new HashSet<PlayerInfo>();
                    var counter = 0;
                    foreach (var playerStats in stats)
                    {
                        PlayerStats playerStatsControl;
                        Controls.TryGetValue(playerStats, out playerStatsControl);
                        if (playerStats.Dealt.Damage == 0 && playerStats.Received.Hits == 0)
                        {
                            continue;
                        }
                        visiblePlayerStats.Add(playerStats);
                        if (playerStatsControl != null) continue;
                        playerStatsControl = new PlayerStats(playerStats);
                        Controls.Add(playerStats, playerStatsControl);

                        if (counter == 9)
                        {
                            break;
                        }
                        counter++;
                    }

                    var invisibleControls = Controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
                    foreach (var invisibleControl in invisibleControls)
                    {
                        Controls[invisibleControl.Key].CloseSkills();
                        Controls.Remove(invisibleControl.Key);
                    }

                    TotalDamage.Content = FormatHelpers.Instance.FormatValue(totalDamage);
                    var intervalvalue = lastHit - firstHit;
                    var interval = TimeSpan.FromSeconds(intervalvalue);
                    Timer.Content = interval.ToString(@"mm\:ss");

                    Players.Items.Clear();
                    var sortedDict = from entry in Controls
                        orderby
                            stats[stats.IndexOf(entry.Value.PlayerInfo)].Dealt.DamageFraction(totalDamage) descending
                        select entry;
                    foreach (var item in sortedDict)
                    {
                        Players.Items.Add(item.Value);
                        var data = stats.IndexOf(item.Value.PlayerInfo);
                        
                        item.Value.Repaint(stats[data], totalDamage, firstHit, lastHit);
                    }

                    Height = Controls.Count*29 + CloseMeter.ActualHeight;
                       
                    if (BasicTeraData.Instance.WindowData.InvisibleUI)
                    {
                        Visibility = Controls.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                    }
                };
            Dispatcher.Invoke(changeUi, nfirstHit, nlastHit, ntotalDamage, nentities, nstats, ncurrentBoss);
        }


        public void HandleConnected(string serverName)
        {
            ChangeTitle changeTitle = delegate(string newServerName)
            {
                Title = newServerName;
                _trayIcon.Text = "Shinra Meter V" + UpdateManager.Version + ": " + newServerName;
            };
            Dispatcher.Invoke(changeTitle, serverName);
        }

        private void StayTopMost()

        {
            
            if (!Topmost || !_topMost) return;
            Topmost = false;
            Topmost = true;
            
        }

        private bool NeedUpdateEncounter(IReadOnlyList<Entity> entities)
        {
            if (entities.Count != ListEncounter.Items.Count)
            {
                return true;
            }
            for (var i = 1; i < ListEncounter.Items.Count - 1; i++)
            {
                if (((Entity)((ComboBoxItem)ListEncounter.Items[i]).Content) != entities[i - 1])
                {
                    return true;
                }
            }
            return false;
        }

        private bool ChangeEncounterSelection(Entity entity)
        {
            if (entity == null)
            {
                return false;
            }
            foreach (var item in ListEncounter.Items)
            {
                if (((Entity) ((ComboBoxItem) item).Content) != entity) continue;
                ListEncounter.SelectedItem = item;
                return true;
            }
            return false;
        }

        private void UpdateComboboxEncounter(IEnumerable<Entity> entities, Entity currentBoss)
        {
            var entityList = entities.ToList();
            if (!NeedUpdateEncounter(entityList))
            {
                ChangeEncounterSelection(currentBoss);
                return;
            }

            Entity selectedEntity = null;
            if ((ComboBoxItem) ListEncounter.SelectedItem != null)
            {
                selectedEntity = (Entity) ((ComboBoxItem) ListEncounter.SelectedItem).Content;
            }

            ListEncounter.Items.Clear();
            ListEncounter.Items.Add(new ComboBoxItem {Content = new Entity("TOTAL")});
            var selected = false;
            foreach (var entity in entityList)
            {
                var item = new ComboBoxItem {Content = entity};
                if (entity.IsBoss())
                {
                    item.Foreground = Brushes.Red;
                }
                ListEncounter.Items.Add(item);
                if (entity != selectedEntity) continue;
                ListEncounter.SelectedItem = ListEncounter.Items[ListEncounter.Items.Count - 1];
                selected = true;
            }
            if (ChangeEncounterSelection(currentBoss))
            {
                return;
            }

            if (selected) return;
            ListEncounter.SelectedItem = ListEncounter.Items[0];

        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Exit();
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

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception Move");
            }
        }



        private void ListEncounter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            var encounter = (Entity) ((ComboBoxItem) e.AddedItems[0]).Content;
            if (encounter.Name.Equals("TOTAL"))
            {
                encounter = null;
            }
            if (encounter != NetworkController.Instance.Encounter)
            {
                NetworkController.Instance.Encounter = encounter;
                NetworkController.Instance.ForceUpdate = true;
            }
        }

        public void CloseEntityStats()
        {
            _entityStats.Hide();
            _entityStats.Topmost = false;
        }

        private delegate void UpdateUi(
            long firstHit, long lastHit, long totalDamage, Dictionary<Entity, EntityInfo> entities,
            List<PlayerInfo> stats, Entity currentBoss);

        private delegate void ChangeTitle(string servername);

        private void PinImage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Topmost)
            {
                Topmost = false;
                PinImage.Source = BasicTeraData.Instance.ImageDatabase.Pin.Source;
                return;
            }
            Topmost = true;
            PinImage.Source = BasicTeraData.Instance.ImageDatabase.UnPin.Source;
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
                Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chrono.Source;
            }
            else
            {
                NetworkController.Instance.TimedEncounter = true;
                Chrono.Source = BasicTeraData.Instance.ImageDatabase.Chronobar.Source;
            }
        }

        private bool _topMost = true;

        private void ListEncounter_OnDropDownOpened(object sender, EventArgs e)
        {
            _topMost = false;
        }

        private void ListEncounter_OnDropDownClosed(object sender, EventArgs e)
        {
            _topMost = true;
        }
    }
}