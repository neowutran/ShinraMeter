using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour Balloon.xaml
    /// </summary>
    public partial class Balloon : UserControl
    {
        public Balloon()
        {
            InitializeComponent();
        }

        public void Value(string title, string text)
        {
            Title.Content = title;
            TextBlock.Text = text;
        }
    }
}
