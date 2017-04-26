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
            EntityStats = new Image {Source = new BitmapImage(new Uri(folder + "stats.png"))};
            EntityStatsClickThrou = new Image {Source = new BitmapImage(new Uri(folder + "stats_click_throu.png"))};
            Chrono = new Image {Source = new BitmapImage(new Uri(folder + "chrono.png"))};
            Chronobar = new Image {Source = new BitmapImage(new Uri(folder + "chronobar.png"))};
            Close = new Image {Source = new BitmapImage(new Uri(folder + "close.png"))};
            History = new Image {Source = new BitmapImage(new Uri(folder + "historic.png"))};
            Copy = new Image {Source = new BitmapImage(new Uri(folder + "copy.png"))};
            Config = new Image {Source = new BitmapImage(new Uri(folder + "config.png"))};
            Chat = new Image {Source = new BitmapImage(new Uri(folder + "chat.png"))};
            Link = new Image {Source = new BitmapImage(new Uri(folder + "link.png"))};
            Enraged = new Image { Source = new BitmapImage(new Uri(folder + "enraged.png")) };

            Icon = new BitmapImage(new Uri(folder + "shinra.ico"));
            Tray = new Icon(folder + "shinra.ico");
        }

        public BitmapImage Icon { get; private set; }
        public Image Chrono { get; private set; }
        public Image Link { get; private set; }

        public Image Copy { get; private set; }
        public Image Config { get; private set; }
        public Image Chat { get; private set; }

        public Image History { get; private set; }
        public Image Close { get; private set; }

        public Image Chronobar { get; private set; }

        public Image EntityStats { get; private set; }
        public Image Enraged { get; private set; }
        public Image EntityStatsClickThrou { get; private set; }

        public Icon Tray { get; private set; }
    }
}