using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Tera.Data;
using Tera.Sniffing;
using Application = System.Windows.Forms.Application;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            TeraSniffer.Instance.Enabled = true;
            UiModel.Instance.Connected += HandleConnected;
            UiModel.Instance.DataUpdated += Update;
            KeyboardHook.Instance.RegisterKeyboardHook();
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Update;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            ListEncounter.Items.Add(new Entity(""));
        }

        public Dictionary<PlayerInfo, PlayerStats> Controls { get; set; } = new Dictionary<PlayerInfo, PlayerStats>();


        public void Update(object sender, EventArgs e)
        {
            KeyboardHook.Instance.SetHotkeys(TeraWindow.IsTeraActive());
            Repaint();
        }

        private void Repaint()
        {
            lock (Controls)
            {
                TotalDamage.Content = FormatHelpers.Instance.FormatValue(DamageTracker.Instance.TotalDamage);
                var interval = TimeSpan.FromSeconds(DamageTracker.Instance.Interval);
                Timer.Content = interval.ToString(@"mm\:ss");

                foreach (var player in Controls)
                {
                    player.Value.Repaint();
                }

                Players.Items.Clear();
                var sortedDict = from entry in Controls
                    orderby entry.Value.PlayerInfo.Dealt.DamageFraction descending
                    select entry;
                foreach (var item in sortedDict)
                {
                    Players.Items.Add(item.Value);
                }
                Height = (Controls.Count)*29 + CloseMeter.ActualHeight;
            }
        }

        public void HandleConnected(string serverName)
        {
            ChangeTitle changeTitle = delegate(string newServerName) { Title = newServerName; };
            Dispatcher.Invoke(changeTitle, serverName);
            UiModel.Instance.Dispatcher = Dispatcher;
        }

        private void EmptyEncounter()
        {
            ListEncounter.Items.Clear();
            ListEncounter.Items.Add(new Entity(""));
            ListEncounter.SelectedIndex = 0;
        }

        private void AddEncounter(IReadOnlyCollection<Entity> entities)
        {
            if ((ListEncounter.Items.Count - 1) > entities.Count) return;
            foreach (
                var entity in
                    entities.Where(entity => !ListEncounter.Items.Contains(entity))
                        .Where(entity => !entity.Name.Equals("")))
            {
                ListEncounter.Items.Add(entity);
            }
            ListEncounter.UpdateLayout();
        }

        private void StayTopMost()

        {
            if (!Topmost) return;
            Topmost = false;
            Topmost = true;
        }

        public void Update(IEnumerable<PlayerInfo> playerStatsSequence, ObservableCollection<Entity> newentities)
        {
            UpdateData changeData = delegate(IEnumerable<PlayerInfo> stats, ObservableCollection<Entity> entities)
            {
                if (entities.Count == 0)
                {
                    EmptyEncounter();
                }
                else
                {
                    AddEncounter(entities);
                }
                StayTopMost();
                stats = stats.OrderByDescending(playerStats => playerStats.Dealt.Damage);
                var visiblePlayerStats = new HashSet<PlayerInfo>();
                foreach (var playerStats in stats)
                {
                    visiblePlayerStats.Add(playerStats);
                    PlayerStats playerStatsControl;
                    Controls.TryGetValue(playerStats, out playerStatsControl);
                    if (playerStatsControl != null) continue;
                    if (playerStats.Dealt.Damage == 0)
                    {
                        continue;
                    }
                    playerStatsControl = new PlayerStats(playerStats);
                    Controls.Add(playerStats, playerStatsControl);
                }

                var invisibleControls = Controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
                foreach (var invisibleControl in invisibleControls)
                {
                    Controls[invisibleControl.Key].CloseSkills();
                    Controls.Remove(invisibleControl.Key);
                }

                if (entities.Count == 0)
                {
                    Repaint();
                }
            };
            Dispatcher.Invoke(changeData, playerStatsSequence, newentities);
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
                ToggleTopMost.Content = "P";
                return;
            }
            Topmost = true;
            ToggleTopMost.Content = "U";
        }


        private void ListEncounter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            var encounter = (Entity) e.AddedItems[0];
            if (encounter.Name.Equals(""))
            {
                encounter = null;
            }
            UiModel.Instance.Encounter = encounter;
            Repaint();
        }

        private delegate void ChangeTitle(string servername);

        private delegate void UpdateData(IEnumerable<PlayerInfo> stats, ObservableCollection<Entity> entities);
    }
}