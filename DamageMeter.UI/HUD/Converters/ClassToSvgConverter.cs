using System;
using System.Globalization;
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
                PlayerClass.Warrior =>   App.Current.FindResource("SvgClassWarrior"),
                PlayerClass.Lancer =>    App.Current.FindResource("SvgClassLancer"),
                PlayerClass.Slayer =>    App.Current.FindResource("SvgClassSlayer"),
                PlayerClass.Berserker => App.Current.FindResource("SvgClassBerserker"),
                PlayerClass.Sorcerer =>  App.Current.FindResource("SvgClassSorcerer"),
                PlayerClass.Archer =>    App.Current.FindResource("SvgClassArcher"),
                PlayerClass.Priest =>    App.Current.FindResource("SvgClassPriest"),
                PlayerClass.Mystic =>    App.Current.FindResource("SvgClassMystic"),
                PlayerClass.Reaper =>    App.Current.FindResource("SvgClassReaper"),
                PlayerClass.Gunner =>    App.Current.FindResource("SvgClassGunner"),
                PlayerClass.Brawler =>   App.Current.FindResource("SvgClassBrawler"),
                PlayerClass.Ninja =>     App.Current.FindResource("SvgClassNinja"),
                PlayerClass.Valkyrie =>  App.Current.FindResource("SvgClassValkyrie"),
                _ =>                     App.Current.FindResource("SvgClassCommon")
            };

            return (Geometry) ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}