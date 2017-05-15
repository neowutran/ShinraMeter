using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Data
{
    public class ImageDatabase
    {
        public ImageDatabase(string folder)
        {
            EntityStats = new Image {Source = new BitmapImage(new Uri(folder + "stats2.png"))};
            EntityStatsClickThrou = new Image {Source = new BitmapImage(new Uri(folder + "stats_click_throu2.png"))};
            Chrono = new Image {Source = new BitmapImage(new Uri(folder + "chrono2.png"))};
            Chronobar = new Image {Source = new BitmapImage(new Uri(folder + "chronobar2.png"))};
            Close = new Image {Source = new BitmapImage(new Uri(folder + "close2.png"))};
            History = new Image {Source = new BitmapImage(new Uri(folder + "historic2.png"))};
            Copy = new Image {Source = new BitmapImage(new Uri(folder + "copy2.png"))};
            Config = new Image {Source = new BitmapImage(new Uri(folder + "config2.png"))};
            Chat = new Image {Source = new BitmapImage(new Uri(folder + "chat2.png"))};
            Link = new Image {Source = new BitmapImage(new Uri(folder + "link2.png"))};
            Enraged = new Image {Source = new BitmapImage(new Uri(folder + "enraged.png"))};
            BossGage = new Image {Source = new BitmapImage(new Uri(folder + "eye2.png"))};

            Icon = new BitmapImage(new Uri(folder + "shinra.ico"));
            Tray = new Icon(folder + "shinra.ico");
        }

        public BitmapImage Icon { get; }
        public Image Chrono { get; }
        public Image Link { get; }

        public Image Copy { get; }
        public Image Config { get; }
        public Image Chat { get; }
        public Image BossGage { get; }

        public Image History { get; }
        public Image Close { get; }

        public Image Chronobar { get; }

        public Image EntityStats { get; }
        public Image Enraged { get; }
        public Image EntityStatsClickThrou { get; }

        public Icon Tray { get; }
    }
}