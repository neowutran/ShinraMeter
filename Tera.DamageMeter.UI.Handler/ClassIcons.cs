using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Tera.Data;
using Tera.Game;

namespace Tera.DamageMeter.UI.Handler
{
    public class ClassIcons
    {
        public const int Size = 32;

        private static ClassIcons _instance;
        private readonly Dictionary<PlayerClass, Image> _images = new Dictionary<PlayerClass, Image>();

        private ClassIcons()
        {
            var directory = BasicTeraData.Instance.ResourceDirectory + @"class-icons\";
            foreach (var playerClass in (PlayerClass[]) Enum.GetValues(typeof (PlayerClass)))
            {
                var filename = directory + playerClass.ToString().ToLowerInvariant() + ".png";
                var image = new Image {Source = new BitmapImage(new Uri(filename))};
                _images.Add(playerClass, image);
            }
        }


        public static ClassIcons Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClassIcons();
                }
                return _instance;
            }
        }

        public Image GetImage(PlayerClass race)
        {
            return _images[race];
        }
    }
}