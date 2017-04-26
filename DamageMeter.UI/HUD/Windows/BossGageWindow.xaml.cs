using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DamageMeter.UI.HUD.Windows
{
    /// <summary>
    /// Logica di interazione per BossGageWindow.xaml
    /// </summary>
    public partial class BossGageWindow : ClickThrouWindow
    {
        public BossGageWindow()
        {
            InitializeComponent();

            Bosses.DataContext = HudManager.Instance.CurrentBosses;
            Bosses.ItemsSource = HudManager.Instance.CurrentBosses;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu = new ContextMenu();
            var HideButton = new MenuItem() { Header = "Hide" };
            HideButton.Click += (s, ev) =>
            {
                HideWindow();
            };
            ContextMenu.Items.Add(HideButton);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move(sender, e);
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }
    }
}

