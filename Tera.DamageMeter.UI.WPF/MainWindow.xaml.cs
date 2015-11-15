using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tera.DamageMeter.UI.Handler;
using Tera.Data;
using Tera.Sniffing;
using Application = System.Windows.Forms.Application;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDpsWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            TeraSniffer.Instance.Enabled = true;
            UiModel.Instance.Connected += HandleConnected;
            UiModel.Instance.DataUpdated += Update;
            KeyboardHook.Instance.RegisterKeyboardHook(this);
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += Update;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        public Dictionary<PlayerInfo, PlayerStats> Controls { get; set; } = new Dictionary<PlayerInfo, PlayerStats>();

        public void Update(object sender, EventArgs e)
        {
            lock (Controls)
            {
                foreach (var player in Controls)
                {
                    player.Value.Repaint();
                }

                Players.Items.Clear();
                var sortedDict = from entry in Controls orderby entry.Value.PlayerData.DamageFraction descending select entry;
                foreach (var item in sortedDict)
                {
                    Players.Items.Add(item.Value);
                }
            }
         
        }

        public List<PlayerData> PlayerData()
        {
            return Controls.Select(keyValue => keyValue.Value.PlayerData).ToList();
        }


        public void HandleConnected(string serverName)
        {
            ChangeTitle changeTitle = delegate(string _serverName) { Title = _serverName; };
            Dispatcher.Invoke(changeTitle, serverName);
        }


        public void Update(IEnumerable<PlayerInfo> playerStatsSequence)
        {
            UpdateData changeData = delegate(IEnumerable<PlayerInfo> stats)
            {
                stats = stats.OrderByDescending(playerStats => playerStats.Dealt.Damage);
                var totalDamage = stats.Sum(playerStats => playerStats.Dealt.Damage);

                var visiblePlayerStats = new HashSet<PlayerInfo>();
                foreach (var playerStats in stats)
                {
                    visiblePlayerStats.Add(playerStats);
                    PlayerStats playerStatsControl;
                    Controls.TryGetValue(playerStats, out playerStatsControl);
                    if (playerStatsControl == null)
                    {
                        playerStatsControl = new PlayerStats(playerStats);
                        Controls.Add(playerStats, playerStatsControl);
                    }

                    playerStatsControl.PlayerData.TotalDamage = totalDamage;
                }

                var invisibleControls = Controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
                foreach (var invisibleControl in invisibleControls)
                {
                    Controls.Remove(invisibleControl.Key);
                }
            };
            Dispatcher.Invoke(changeData, playerStatsSequence);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            BasicTeraData.Instance.WindowData.Location = SystemParameters.WorkArea.Location;
            BasicTeraData.Instance.WindowData.Height = Convert.ToInt32(Height);
            BasicTeraData.Instance.WindowData.Save();
            TeraSniffer.Instance.Enabled = false;
            Application.Exit();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Height = BasicTeraData.Instance.WindowData.Height;
            Top = BasicTeraData.Instance.WindowData.Location.Y;
            Left = BasicTeraData.Instance.WindowData.Location.X;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private delegate void ChangeTitle(string servername);

        private delegate void UpdateData(IEnumerable<PlayerInfo> stats);

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}