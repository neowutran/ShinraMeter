using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI.HUD.Windows
{
    public partial class BossGageWindow
    {
        public BossGageWindow()
        {
            Loaded += OnLoaded;

            // DataContext is MainViewModel, set from MainWindow
            InitializeComponent();

            //Bosses.DataContext = HudManager.Instance.CurrentBosses;
            Bosses.ItemsSource = DamageMeter.HudManager.Instance.CurrentBosses;

            HudManager.Instance.CurrentBosses.CollectionChanged += OnBossesChanged;
        }

        private void OnBossesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!BasicTeraData.Instance.WindowData.BossGageStatus.Visible) return;
            if (HudManager.Instance.CurrentBosses.Count == 0)
            {
                HideWindow();
            }
            else
            {
                    ShowWindow();
                //if (!IsVisible)
                //{
                //}
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.LastSnappedPoint = BasicTeraData.Instance.WindowData.BossGageStatus.Location;
            this.Left = this.LastSnappedPoint?.X ?? 0;
            this.Top = this.LastSnappedPoint?.Y ?? 0;
            this.Show();
            this.Hide();
            if (BasicTeraData.Instance.WindowData.BossGageStatus.Visible)
            {
                this.ShowWindow();
            }


            //ContextMenu = new ContextMenu();
            //var HideButton = new MenuItem {Header = "Hide"};
            //HideButton.Click += (s, ev) => { HideWindow(); };
            //ContextMenu.Items.Add(HideButton);
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        protected override bool Empty => HudManager.Instance.CurrentBosses.Count == 0;

        public override void SaveWindowPos()
        {
            if (double.IsNaN(Left) || double.IsNaN(Top)) return;

            BasicTeraData.Instance.WindowData.BossGageStatus = new WindowStatus(LastSnappedPoint ?? new Point(Left, Top), Visible, Scale);
        }
    }
}