using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamageMeter.UI.Windows
{
    public partial class FooterButton 
    {
        public Geometry SvgIcon
        {
            get => (Geometry)GetValue(SvgIconProperty);
            set => SetValue(SvgIconProperty, value);
        }
        public static readonly DependencyProperty SvgIconProperty = DependencyProperty.Register("SvgIcon", typeof(Geometry), typeof(FooterButton));

        public FooterButton()
        {
            InitializeComponent();
        }
    }
}
