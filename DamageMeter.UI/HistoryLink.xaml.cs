using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour HistoryLink.xaml
    /// </summary>
    public partial class HistoryLink : UserControl
    {
        public HistoryLink(string link, Entity boss )
        {
            InitializeComponent();
            Boss.Content = boss.Name;
            Boss.Tag = link;
            if (link.StartsWith("!"))
            {
                Boss.Foreground = Brushes.Red;
                Boss.ToolTip = link;
                return;

            }
            Link.Source = BasicTeraData.Instance.ImageDatabase.Link.Source;

        }

        private void Click_Link(object sender, MouseButtonEventArgs e)
        {
            if (!Boss.Tag.ToString().StartsWith("!"))
                Process.Start("explorer.exe", "http://teradps.io/party/rank/" + Boss.Tag);
        }

        private void Sender_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}
