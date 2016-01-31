using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Data
{
    public class PinData
    {
        public PinData(string folder)
        {
            Pin = new Image {Source = new BitmapImage(new Uri(folder + "pin.png"))};
            UnPin = new Image {Source = new BitmapImage(new Uri(folder + "unpin.png"))};
            EntityStats = new Image {Source = new BitmapImage(new Uri(folder + "stats.png"))};
        }

        public Image Pin { get; private set; }
        public Image UnPin { get; private set; }
        public Image EntityStats { get; private set; }
    }
}