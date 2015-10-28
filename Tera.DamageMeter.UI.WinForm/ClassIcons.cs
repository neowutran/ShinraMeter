using System;
using System.Collections.Generic;
using System.Drawing;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class ClassIcons
    {
        private readonly Dictionary<PlayerClass, Image> _images = new Dictionary<PlayerClass, Image>();

        public ClassIcons(string directory, int size)
        {
            Size = size;
            foreach (var playerClass in (PlayerClass[]) Enum.GetValues(typeof (PlayerClass)))
            {
                var filename = directory + playerClass.ToString().ToLowerInvariant() + ".png";
                using (var image = Image.FromFile(filename))
                {
                    var resizedImage = new Bitmap(image, size, size);
                    _images.Add(playerClass, resizedImage);
                }
            }
        }

        public int Size { get; private set; }

        public Image GetImage(PlayerClass race)
        {
            return _images[race];
        }
    }
}