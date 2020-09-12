using System.Windows;
using System.Windows.Media;

namespace DamageMeter.UI.Windows
{
    public partial class SvgLed
    {

        public static readonly DependencyProperty OffBrushProperty = DependencyProperty.Register("OffBrush", typeof(Brush), typeof(SvgLed), new PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty OnBrushProperty = DependencyProperty.Register("OnBrush", typeof(Brush), typeof(SvgLed), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty OffGeometryProperty = DependencyProperty.Register("OffGeometry", typeof(Geometry), typeof(SvgLed), new PropertyMetadata(null));
        public static readonly DependencyProperty OnGeometryProperty = DependencyProperty.Register("OnGeometry", typeof(Geometry), typeof(SvgLed), new PropertyMetadata(null));
        public static readonly DependencyProperty OffMarginProperty = DependencyProperty.Register("OffMargin", typeof(Thickness), typeof(SvgLed), new PropertyMetadata(new Thickness(0)));
        public static readonly DependencyProperty OnMarginProperty = DependencyProperty.Register("OnMargin", typeof(Thickness), typeof(SvgLed), new PropertyMetadata(new Thickness(0)));
        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register("IsOn", typeof(bool), typeof(SvgLed), new PropertyMetadata(false));


        public Brush OffBrush
        {
            get => (Brush)GetValue(OffBrushProperty);
            set => SetValue(OffBrushProperty, value);
        }
        public Brush OnBrush
        {
            get => (Brush)GetValue(OnBrushProperty);
            set => SetValue(OnBrushProperty, value);
        }
        public Geometry OffGeometry
        {
            get => (Geometry)GetValue(OffGeometryProperty);
            set => SetValue(OffGeometryProperty, value);
        }
        public Geometry OnGeometry
        {
            get => (Geometry)GetValue(OnGeometryProperty);
            set => SetValue(OnGeometryProperty, value);
        }
        public Thickness OnMargin
        {
            get => (Thickness)GetValue(OnMarginProperty);
            set => SetValue(OnMarginProperty, value);
        }
        public Thickness OffMargin
        {
            get => (Thickness)GetValue(OffMarginProperty);
            set => SetValue(OffMarginProperty, value);
        }
        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        public SvgLed()
        {
            InitializeComponent();
        }
    }
}
