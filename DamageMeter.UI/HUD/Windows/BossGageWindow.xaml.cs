using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Data;

namespace DamageMeter.UI.HUD.Windows
{
    public partial class BossGageWindow
    {
        public BossGageWindow()
        {

            // DataContext is MainViewModel, set from MainWindow
            InitializeComponent();

            //Bosses.DataContext = HudManager.Instance.CurrentBosses;
            Bosses.ItemsSource = HudManager.Instance.CurrentBosses;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LastSnappedPoint = BasicTeraData.Instance.WindowData.BossGageStatus.Location;
            this.Left = this.LastSnappedPoint?.X ?? 0;
            this.Top = this.LastSnappedPoint?.Y ?? 0;
            this.Show();
            this.Hide();
            if (BasicTeraData.Instance.WindowData.BossGageStatus.Visible) this.ShowWindow();


            //ContextMenu = new ContextMenu();
            //var HideButton = new MenuItem {Header = "Hide"};
            //HideButton.Click += (s, ev) => { HideWindow(); };
            //ContextMenu.Items.Add(HideButton);
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        protected override bool Empty => !Bosses.HasItems;

        public void TempToggle(bool visible)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                Bosses.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }, DispatcherPriority.DataBind);
        }
    }
}