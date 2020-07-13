using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamageMeter.UI.Windows
{
    public partial class SliderButton : Button
    {
        public Geometry SvgIcon
        {
            get => (Geometry)GetValue(SvgIconProperty);
            set => SetValue(SvgIconProperty, value);
        }
        public static readonly DependencyProperty SvgIconProperty = DependencyProperty.Register("SvgIcon", typeof(Geometry), typeof(SliderButton));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(SliderButton), new PropertyMetadata(""));




        public SliderButton()
        {
            InitializeComponent();
        }
    }
}
