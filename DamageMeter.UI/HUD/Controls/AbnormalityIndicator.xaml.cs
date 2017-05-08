using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Data;
using static Tera.Game.HotDot;

namespace DamageMeter.UI.HUD.Controls
{
    public partial class AbnormalityIndicator
    {
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(AbnormalityIndicator));

        private BuffDuration _context;

        public AbnormalityIndicator()
        {
            InitializeComponent();
        }

        public double Size
        {
            get => (double) GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        private void buff_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Refresh") { return; }
            var an = new DoubleAnimation(0, 359.9, TimeSpan.FromMilliseconds(((BuffDuration) sender).Duration));
            var fps = ((BuffDuration) sender).Duration > 80000 ? 1 : 30;
            Timeline.SetDesiredFrameRate(an, fps);
            arc.BeginAnimation(Arc.EndAngleProperty, an);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _context = (BuffDuration) DataContext;
            _context.PropertyChanged += buff_PropertyChanged;
            RenderTransform = new ScaleTransform(0, 0, .5, .5);
            RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) {EasingFunction = new QuadraticEase()});
            RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) {EasingFunction = new QuadraticEase()});
            abnormalityIcon.Width = Size * .9;
            abnormalityIcon.Height = Size * .9;
            bgEll.Width = Size;
            bgEll.Height = Size;
            arc.Width = Size * .9;
            arc.Height = Size * .9;

            if (((BuffDuration) DataContext).Duration <= 0) { return; }
            var an = new DoubleAnimation(0, 359.9, TimeSpan.FromMilliseconds(((BuffDuration) DataContext).Duration));
            var fps = ((BuffDuration) DataContext).Duration > 80000 ? 1 : 30;
            Timeline.SetDesiredFrameRate(an, fps);
            arc.BeginAnimation(Arc.EndAngleProperty, an);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _context.PropertyChanged -= buff_PropertyChanged;
            _context = null;
        }
    }
}

namespace DamageMeter.UI.HUD.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BasicTeraData.Instance.Icons.GetImage((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DurationLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var seconds = (int) value / 1000;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            var days = hours / 24;

            if (minutes < 3) { return seconds.ToString(); }
            if (hours < 3) { return minutes + "m"; }
            if (days < 1) { return hours + "h"; }
            return days + "d";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AbnormalityStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (AbnormalityType) value;
            switch (val)
            {
                case AbnormalityType.Stun: return new SolidColorBrush(Colors.Red);
                case AbnormalityType.DOT: return new SolidColorBrush(Color.FromRgb(0x98, 0x42, 0xf4));
                case AbnormalityType.Debuff: return new SolidColorBrush(Color.FromRgb(0x8f, 0xf4, 0x42));
                case AbnormalityType.Buff: return new SolidColorBrush(Color.FromRgb(0x3f, 0x9f, 0xff));
                default: return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StacksToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stacks = (int) value;
            if (stacks > 1) { return Visibility.Visible; }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DurationToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (int) value;
            if (duration < 0) { return Visibility.Hidden; }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SizeToStackLabelSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (double) value;
            return size / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SizeToDurationLabelSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (double) value;
            return size / 1.8;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SizeToDurationLabelMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (double) value;
            return new Thickness(0, 0, 0, -size * 1.25);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}