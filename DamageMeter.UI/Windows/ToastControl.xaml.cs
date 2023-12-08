using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Nostrum.Factories;

namespace DamageMeter.UI.Windows
{
    public partial class ToastControl
    {
        private const int _animDuration = 250;

        private readonly DoubleAnimation _fadeIn;
        private readonly DoubleAnimation _fadeOut;
        private readonly DoubleAnimation _slideIn;
        private readonly DoubleAnimation _slideOut;

        public string ToastText
        {
            get => (string) GetValue(ToastTextProperty);
            set => SetValue(ToastTextProperty, value);
        }
        public static readonly DependencyProperty ToastTextProperty = DependencyProperty.Register(nameof(ToastText), typeof(string), typeof(ToastControl), new PropertyMetadata(""));

        public ToastInfo.Severity ToastSeverity
        {
            get => (ToastInfo.Severity) GetValue(ToastSeverityProperty);
            set => SetValue(ToastSeverityProperty, value);
        }
        public static readonly DependencyProperty ToastSeverityProperty = DependencyProperty.Register(nameof(ToastSeverity), typeof(ToastInfo.Severity), typeof(ToastControl), new PropertyMetadata(ToastInfo.Severity.Success, OnSeverityChanged));

        public double SlideOffset
        {
            get => (double) GetValue(SlideOffsetProperty);
            set => SetValue(SlideOffsetProperty, value);
        }
        public static readonly DependencyProperty SlideOffsetProperty = DependencyProperty.Register(nameof(SlideOffset), typeof(double), typeof(ToastControl), new PropertyMetadata(0D, OnSlideOffsetChanged));

        public bool IsShowed
        {
            get => (bool) GetValue(IsShowedProperty);
            set => SetValue(IsShowedProperty, value);
        }
        public static readonly DependencyProperty IsShowedProperty = DependencyProperty.Register(nameof(IsShowed), typeof(bool), typeof(ToastControl), new PropertyMetadata(false, OnShowedChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius) GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ToastControl));


        public ToastControl()
        {
            _fadeIn = AnimationFactory.CreateDoubleAnimation(_animDuration, 1, 0, true);
            _fadeOut = AnimationFactory.CreateDoubleAnimation(_animDuration, 0, 1, true);
            _slideIn = AnimationFactory.CreateDoubleAnimation(_animDuration, SlideOffset, 0, true);
            _slideOut = AnimationFactory.CreateDoubleAnimation(_animDuration, 0, SlideOffset, true);

            InitializeComponent();
        }

        private static void OnShowedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ToastControl tc)) { return; }
            tc.Toggle();
        }

        private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ToastControl tc)) { return; }
            tc.SetSeverity();
        }

        private static void OnSlideOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ToastControl tc)) { return; }
            tc.UpdateSlideOffset();
        }

        private void Toggle()
        {
            MainBorder.BeginAnimation(OpacityProperty, IsShowed ? _fadeIn : _fadeOut);
            MainBorder.RenderTransform.BeginAnimation(TranslateTransform.YProperty, IsShowed ? _slideIn : _slideOut);
        }

        private void SetSeverity()
        {
            MainBorder.Background = ToastSeverity switch
            {
                ToastInfo.Severity.Info => Brushes.SlateGray,
                ToastInfo.Severity.Success => Brushes.MediumSeaGreen,
                ToastInfo.Severity.Warning => Brushes.DarkOrange,
                ToastInfo.Severity.Error => new SolidColorBrush(Color.FromRgb(0xff, 0x44, 0x55)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void UpdateSlideOffset()
        {
            _slideIn.From = SlideOffset;
            _slideOut.To = SlideOffset;
        }
    }
}