using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using Tera.Game;
using Image = System.Windows.Controls.Image;

namespace Data
{
    public class ClassIcons
    {
        private static ClassIcons _instance;
        private readonly Dictionary<PlayerClass, Bitmap> _drawings = new Dictionary<PlayerClass, Bitmap>();
        private readonly Dictionary<PlayerClass, Image> _images = new Dictionary<PlayerClass, Image>();

        private ClassIcons()
        {
            var directory = BasicTeraData.Instance.ResourceDirectory + @"\data\class-icons\";
            foreach (var playerClass in (PlayerClass[]) Enum.GetValues(typeof(PlayerClass)))
            {
                var filename = directory + playerClass.ToString().ToLowerInvariant() + ".png";
                var image = new Image {Source = new BitmapImage(new Uri(filename))};
                _images.Add(playerClass, image);
                var drawing = new Bitmap(filename);
                for (var i = 0; i < drawing.Width; i++)
                {
                    for (var j = 0; j < drawing.Height; j++)
                    {
                        var col = drawing.GetPixel(i, j);
                        drawing.SetPixel(i, j,
                            Color.FromArgb(col.A, 255 - (col.R + col.G + col.B)/3, 255 - (col.R + col.G + col.B)/3,
                                255 - (col.R + col.G + col.B)/3));
                    }
                }
                _drawings.Add(playerClass, drawing);
            }
        }


        public static ClassIcons Instance => _instance ?? (_instance = new ClassIcons());

        public Image GetImage(PlayerClass pclass)
        {
            return _images[pclass];
        }

        public Bitmap GetBitmap(PlayerClass pclass)
        {
            return _drawings[pclass];
        }
    }
}