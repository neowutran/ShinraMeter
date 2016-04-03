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
            Pin = new Image {Source = new BitmapImage(new Uri(folder + "pin.png"))};
            UnPin = new Image {Source = new BitmapImage(new Uri(folder + "unpin.png"))};
            EntityStats = new Image {Source = new BitmapImage(new Uri(folder + "stats.png"))};
            EntityStatsClickThrou = new Image { Source = new BitmapImage(new Uri(folder + "stats_click_throu.png")) };
            Chrono = new Image { Source = new BitmapImage(new Uri(folder + "chrono.png")) };
            Chronobar = new Image { Source = new BitmapImage(new Uri(folder + "chronobar.png")) };
            Close = new Image { Source = new BitmapImage(new Uri(folder + "close.png")) };
            History = new Image { Source = new BitmapImage(new Uri(folder + "historic.png")) };

            Tray = new Icon(folder + "shinra.ico");

        }

        public Image Chrono { get; private set; }

        public Image History { get; private set; }
        public Image Close { get; private set; }

        public Image Chronobar { get; private set; }

        public Image Pin { get; private set; }
        public Image UnPin { get; private set; }
        public Image EntityStats { get; private set; }

        public Image EntityStatsClickThrou { get; private set; }

        public Icon Tray { get; private set; }
    }
}