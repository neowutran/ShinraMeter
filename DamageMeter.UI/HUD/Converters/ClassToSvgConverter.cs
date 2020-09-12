using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Tera.Game;

namespace DamageMeter.UI.HUD.Converters
{
    public class ClassToSvgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value switch
            {
                PlayerClass cl => cl,
                string s => (PlayerClass)Enum.Parse(typeof(PlayerClass), s),
                _ => PlayerClass.Common
            };

            var ret = c switch
            {
                PlayerClass.Warrior =>   Application.Current.FindResource("SvgClassWarrior"),
                PlayerClass.Lancer =>    Application.Current.FindResource("SvgClassLancer"),
                PlayerClass.Slayer =>    Application.Current.FindResource("SvgClassSlayer"),
                PlayerClass.Berserker => Application.Current.FindResource("SvgClassBerserker"),
                PlayerClass.Sorcerer =>  Application.Current.FindResource("SvgClassSorcerer"),
                PlayerClass.Archer =>    Application.Current.FindResource("SvgClassArcher"),
                PlayerClass.Priest =>    Application.Current.FindResource("SvgClassPriest"),
                PlayerClass.Mystic =>    Application.Current.FindResource("SvgClassMystic"),
                PlayerClass.Reaper =>    Application.Current.FindResource("SvgClassReaper"),
                PlayerClass.Gunner =>    Application.Current.FindResource("SvgClassGunner"),
                PlayerClass.Brawler =>   Application.Current.FindResource("SvgClassBrawler"),
                PlayerClass.Ninja =>     Application.Current.FindResource("SvgClassNinja"),
                PlayerClass.Valkyrie =>  Application.Current.FindResource("SvgClassValkyrie"),
                _ =>                     Application.Current.FindResource("SvgClassCommon")
            };

            return (Geometry) ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}