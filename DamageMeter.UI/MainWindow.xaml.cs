using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DamageMeter.AutoUpdate;
using DamageMeter.Sniffing;
using Data;
using Application = System.Windows.Forms.Application;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            TeraSniffer.Instance.Enabled = true;
            NetworkController.Instance.Connected += HandleConnected;
            NetworkController.Instance.TickUpdated += Update;
            DamageTracker.Instance.CurrentBossUpdated += SelectEncounter;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += UpdateKeyboard;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            PinImage.Source = BasicTeraData.Instance.PinData.UnPin.Source;
            UpdateComboboxEncounter(new LinkedList<Entity>());
            Title = "Shinra Meter V" + UpdateManager.Version;
        }

        private bool _keyboardInitialized;

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


        public Dictionary<PlayerInfo, PlayerStats> Controls { get; set; } = new Dictionary<PlayerInfo, PlayerStats>();

        public void Update(long nintervalvalue, long ntotalDamage, LinkedList<Entity> nentities, List<PlayerInfo> nstats)
        {

            UpdateUi changeUi =
                delegate(long intervalvalue, long totalDamage, LinkedList<Entity> entities, List<PlayerInfo> stats)
                {
                    UpdateComboboxEncounter(entities);
                    StayTopMost();
                    var visiblePlayerStats = new HashSet<PlayerInfo>();
                    var counter = 0;
                    foreach (var playerStats in stats)
                    {
                        PlayerStats playerStatsControl;
                        Controls.TryGetValue(playerStats, out playerStatsControl);
                        if (playerStats.Dealt.Damage == 0 && playerStats.Received.Hits == 0){continue;}
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
                    var interval = TimeSpan.FromSeconds(intervalvalue);
                    Timer.Content = interval.ToString(@"mm\:ss");
                    foreach (var player in Controls)
                    {
                        var data = stats.IndexOf(player.Value.PlayerInfo);
                        player.Value.Repaint(stats[data],totalDamage);
                    }
                    Players.Items.Clear();
                    var sortedDict = from entry in Controls
                        orderby entry.Value.PlayerInfo.Dealt.DamageFraction(totalDamage) descending
                        select entry;
                    foreach (var item in sortedDict)
                    {
                        Players.Items.Add(item.Value);
                    }
                    Height = (Controls.Count)*29 + CloseMeter.ActualHeight;
                };
            Dispatcher.Invoke(changeUi, nintervalvalue, ntotalDamage, nentities, nstats);
        }


        public void HandleConnected(string serverName)
        {
            ChangeTitle changeTitle = delegate(string newServerName) { Title = newServerName; };
            Dispatcher.Invoke(changeTitle, serverName);
        }

        public void SelectEncounter(Entity entity)
        {
            UpdateEncounter changeSelected = delegate(Entity newentity)
            {
                if (!newentity.IsBoss()) return;

                foreach (var item in ListEncounter.Items)
                {
                    var encounter = (Entity) ((ComboBoxItem) item).Content;
                    if (encounter != newentity) continue;
                    ListEncounter.SelectedItem = item;
                    NetworkController.Instance.ForceUpdate();
                    return;
                }
            };
            Dispatcher.Invoke(changeSelected, entity);
        }

        private void StayTopMost()

        {
            if (!Topmost) return;
            Topmost = false;
            Topmost = true;
        }

        private void UpdateComboboxEncounter(IEnumerable<Entity> entities)
        {
            Entity selectedEntity = null;
            if (((ComboBoxItem) ListEncounter.SelectedItem) != null)
            {
                selectedEntity = (Entity) ((ComboBoxItem) ListEncounter.SelectedItem).Content;
            }

            ListEncounter.Items.Clear();
            ListEncounter.Items.Add(new ComboBoxItem {Content = new Entity("TOTAL")});
            var selected = false;
            foreach (var entity in entities)
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
            if (selected) return;
            ListEncounter.SelectedItem = ListEncounter.Items[0];
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            BasicTeraData.Instance.WindowData.Location = new Point(Left, Top);
            BasicTeraData.Instance.WindowData.Save();
            TeraSniffer.Instance.Enabled = false;
            Application.Exit();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Top = BasicTeraData.Instance.WindowData.Location.Y;
            Left = BasicTeraData.Instance.WindowData.Location.X;
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


        private void ToggleTopMost_OnClick(object sender, RoutedEventArgs e)
        {
            if (Topmost)
            {
                Topmost = false;
                PinImage.Source = BasicTeraData.Instance.PinData.Pin.Source;
                return;
            }
            Topmost = true;
            PinImage.Source = BasicTeraData.Instance.PinData.UnPin.Source;
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
                NetworkController.Instance.ForceUpdate();
            }
        }

        private delegate void UpdateEncounter(Entity entity);

        private delegate void UpdateUi(
            long intervalvalue, long totalDamage, LinkedList<Entity> entities, List<PlayerInfo> stats);

        private delegate void ChangeTitle(string servername);
    }
}