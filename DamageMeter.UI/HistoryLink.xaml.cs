using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.TeraDpsApi;
using Data;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour HistoryLink.xaml
    /// </summary>
    public partial class HistoryLink
    {
        public HistoryLink(UploadData link, NpcEntity boss)
        {
            InitializeComponent();
            Boss.Content = boss.Info.Name;
            Boss.Tag = link.Url;
            var tt = new UploadTooltip(link);
            var t = new ToolTip { Background = Brushes.Transparent, Padding = new Thickness(10, 10, 20, 20), BorderThickness = new Thickness(0), Content = tt };
            Boss.ToolTip = t;
            if (!link.Success)
            {
                Boss.Foreground = Brushes.Red;
                return;
            }
            Link.Source = BasicTeraData.Instance.ImageDatabase.Link.Source;
        }

        private void Click_Link(object sender, MouseButtonEventArgs e)
        {
            if (Boss.Tag.ToString().StartsWith("http://") || Boss.Tag.ToString().StartsWith("https://")) { Process.Start("explorer.exe", "\"" + Boss.Tag + "\""); }
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}