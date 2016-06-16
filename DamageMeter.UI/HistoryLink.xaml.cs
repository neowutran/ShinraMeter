using Data;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour HistoryLink.xaml
    /// </summary>
    public partial class HistoryLink : UserControl
    {
        public HistoryLink(string link, NpcEntity boss )
        {
            InitializeComponent();
            Boss.Content = boss.Info.Name;
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
