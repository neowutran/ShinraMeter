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
            Tray = new Icon(folder + "shinra.ico");
        }

        public Image Pin { get; private set; }
        public Image UnPin { get; private set; }
        public Image EntityStats { get; private set; }

        public Icon Tray { get; private set; }
    }
}