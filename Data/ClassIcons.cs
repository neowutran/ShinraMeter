using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using Tera.Game;

namespace Data
{
    public class ClassIcons
    {
        private static ClassIcons _instance;
        private readonly Dictionary<PlayerClass, System.Windows.Controls.Image> _images = new Dictionary<PlayerClass, System.Windows.Controls.Image>();
        private readonly Dictionary<PlayerClass, Bitmap> _drawings = new Dictionary<PlayerClass, Bitmap>();

        private ClassIcons()
        {
            var directory = BasicTeraData.Instance.ResourceDirectory + @"\data\class-icons\";
            foreach (var playerClass in (PlayerClass[]) Enum.GetValues(typeof (PlayerClass)))
            {
                var filename = directory + playerClass.ToString().ToLowerInvariant() + ".png";
                var image = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri(filename))};
                _images.Add(playerClass, image);
                var drawing = new Bitmap(filename);
                for (int i = 0; i < drawing.Width; i++)
                {
                    for (int j = 0; j < drawing.Height; j++)
                    {
                        var col = drawing.GetPixel(i, j);
                        drawing.SetPixel(i,j,Color.FromArgb(col.A,255-col.R,255-col.G,255-col.B));
                    }
                }
                        _drawings.Add(playerClass, drawing);
            }
        }


        public static ClassIcons Instance => _instance ?? (_instance = new ClassIcons());

        public System.Windows.Controls.Image GetImage(PlayerClass pclass)
        {
            return _images[pclass];
        }
        public Bitmap GetBitmap(PlayerClass pclass)
        {
            return _drawings[pclass];
        }
    }
}