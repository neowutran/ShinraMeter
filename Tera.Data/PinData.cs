using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
namespace Tera.Data
{
    public class PinData
    {
        public Image Pin { get; private set; }
        public Image UnPin { get; private set; }
 
        public PinData(string folder)
        {
            Pin = new Image { Source = new BitmapImage(new Uri(folder + "pin.png")) };
            UnPin = new Image { Source = new BitmapImage(new Uri(folder + "unpin.png")) };

        }
    }
}
