using System.Windows;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            lDps.Content = "112k/s";
        }

        private void button_Menu_click(object sender, RoutedEventArgs e)
        {
        }

        private void button_Reset_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}