using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Nostrum.Controls;

namespace Nostrum.Converters
{
    /// <summary>
    /// Returns a rounded rectangle to use as clip geometry. Inputs values are width, height, radius.
    /// </summary>
    public class RoundedClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3 || !(values[0] is double width) || !(values[1] is double height) || !(values[2] is CornerRadius radius))
                return DependencyProperty.UnsetValue;

            if (width < double.Epsilon || height < double.Epsilon)
                return Geometry.Empty;

            // Actually we need more complex geometry, when CornerRadius has different values.
            // But let me not to take this into account, and simplify example for a common value.
            var clip = new RectangleGeometry(new Rect(0, 0, width, height), radius.TopLeft, radius.TopLeft);
            clip.Freeze();
            return clip;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Nostrum.Extensions
{
    public class ButtonExtensions
    {
        // CornerRadius
        /// <summary>
        /// Specifies the <see cref="CornerRadius"/> of the <see cref="Button"/>.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached("CornerRadius",
                                                                                                             typeof(CornerRadius),
                                                                                                             typeof(ButtonExtensions),
                                                                                                             new PropertyMetadata(new CornerRadius(0)));
        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius) obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }

        // RippleBrush
        /// <summary>
        /// Specifies the <see cref="Brush"/> of the <see cref="Ripple"/> contained in the <see cref="Button"/>.
        /// </summary>
        public static readonly DependencyProperty RippleBrushProperty = DependencyProperty.RegisterAttached("RippleBrush",
                                                                                                            typeof(Brush),
                                                                                                            typeof(ButtonExtensions),
                                                                                                            new PropertyMetadata(Brushes.White));
        public static Brush GetRippleBrush(DependencyObject obj)
        {
            return (Brush) obj.GetValue(RippleBrushProperty);
        }

        public static void SetRippleBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(RippleBrushProperty, value);
        }

        // RippleDuration
        /// <summary>
        /// Specifies the duration of the <see cref="Ripple"/> animation.
        /// </summary>
        public static readonly DependencyProperty RippleDurationProperty = DependencyProperty.RegisterAttached("RippleDuration",
                                                                                                            typeof(int),
                                                                                                            typeof(ButtonExtensions),
                                                                                                            new PropertyMetadata(650));
        public static int GetRippleDuration(DependencyObject obj)
        {
            return (int) obj.GetValue(RippleDurationProperty);
        }

        public static void SetRippleDuration(DependencyObject obj, int value)
        {
            obj.SetValue(RippleDurationProperty, value);
        }

        // RippleStaysVisible
        /// <summary>
        /// Specifies if the <see cref="Ripple"/> should remain expanded.
        /// </summary>
        public static readonly DependencyProperty RippleStaysVisibleProperty = DependencyProperty.RegisterAttached("RippleStaysVisible",
                                                                                                            typeof(bool),
                                                                                                            typeof(ButtonExtensions),
                                                                                                            new PropertyMetadata(false));
        public static bool GetRippleStaysVisible(DependencyObject obj)
        {
            return (bool) obj.GetValue(RippleStaysVisibleProperty);
        }

        public static void SetRippleStaysVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(RippleStaysVisibleProperty, value);
        }

        // RippleReversed
        /// <summary>
        /// Specifies the direction of the <see cref="Ripple"/> animation.
        /// </summary>
        public static readonly DependencyProperty RippleReversedProperty = DependencyProperty.RegisterAttached("RippleReversed",
                                                                                                            typeof(bool),
                                                                                                            typeof(ButtonExtensions),
                                                                                                            new PropertyMetadata(false));
        public static bool GetRippleReversed(DependencyObject obj)
        {
            return (bool) obj.GetValue(RippleReversedProperty);
        }

        public static void SetRippleReversed(DependencyObject obj, bool value)
        {
            obj.SetValue(RippleReversedProperty, value);
        }

        // RippleEnabled
        /// <summary>
        /// Enables or disables the <see cref="Ripple"/>.
        /// </summary>
        public static readonly DependencyProperty RippleEnabledProperty = DependencyProperty.RegisterAttached("RippleEnabled",
                                                                                                            typeof(bool),
                                                                                                            typeof(ButtonExtensions),
                                                                                                            new PropertyMetadata(true));
        public static bool GetRippleEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(RippleEnabledProperty);
        }

        public static void SetRippleEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(RippleEnabledProperty, value);
        }

        // BorderEffect
        /// <summary>
        /// Specifies an <see cref="Effect"/> for the underlying <see cref="Border"/> of the <see cref="Button"/>.
        /// </summary>
        public static readonly DependencyProperty BorderEffectProperty = DependencyProperty.RegisterAttached("BorderEffect",
                                                                                                            typeof(Effect),
                                                                                                            typeof(ButtonExtensions),
                                                                                                            new PropertyMetadata(null));
        public static Effect GetBorderEffect(DependencyObject obj)
        {
            return (Effect) obj.GetValue(BorderEffectProperty);
        }

        public static void SetBorderEffect(DependencyObject obj, Effect value)
        {
            obj.SetValue(BorderEffectProperty, value);
        }
    }
}