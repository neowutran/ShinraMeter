using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Tera.Game;

namespace Data
{
    public class ClassIcons
    {
        private static ClassIcons _instance;
        private readonly Dictionary<PlayerClass, Image> _images = new Dictionary<PlayerClass, Image>();

        private ClassIcons()
        {
            var directory = BasicTeraData.Instance.ResourceDirectory + @"\data\class-icons\";
            foreach (var playerClass in (PlayerClass[]) Enum.GetValues(typeof (PlayerClass)))
            {
                var filename = directory + playerClass.ToString().ToLowerInvariant() + ".png";
                var image = new Image {Source = new BitmapImage(new Uri(filename))};
                _images.Add(playerClass, image);
            }
        }


        public static ClassIcons Instance => _instance ?? (_instance = new ClassIcons());

        public Image GetImage(PlayerClass race)
        {
            return _images[race];
        }
    }
}