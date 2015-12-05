using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Tera.Data
{
    public class PinData
    {
        public PinData(string folder)
        {
            Pin = new Image {Source = new BitmapImage(new Uri(folder + "pin.png"))};
            UnPin = new Image {Source = new BitmapImage(new Uri(folder + "unpin.png"))};
        }

        public Image Pin { get; private set; }
        public Image UnPin { get; private set; }
    }
}