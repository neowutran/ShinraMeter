using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using DamageMeter.UI.Annotations;
using Data;

namespace DamageMeter.UI.Windows
{
    public partial class SettingsWindow 
    {
        private readonly DoubleAnimation _effectOffAnim;
        private readonly DoubleAnimation _effectOnAnim;
        private readonly DoubleAnimation _opacityMaskScaleExpandAnim;
        private readonly DoubleAnimation _sliderBackgroundWidthExpandAnim;
        private readonly DoubleAnimation _opacityMaskScaleShrinkAnim;
        private readonly DoubleAnimation _sliderBackgroundWidthShrinkAnim;
        private readonly DoubleAnimation _selRectAnim;
        private static SettingsWindow _window;
        private static bool _visible;


        public SettingsWindow()
        {
            _effectOnAnim = new DoubleAnimation(.2, TimeSpan.FromMilliseconds(100));
            _effectOffAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(50));
            _opacityMaskScaleExpandAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            _opacityMaskScaleShrinkAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            _sliderBackgroundWidthExpandAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            _sliderBackgroundWidthShrinkAnim = new DoubleAnimation(40, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            _selRectAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() };

            InitializeComponent();
            DataContext = new SettingsWindowViewModel();
        }

        private void ActivatorOnMouseEnter(object sender, MouseEventArgs e)
        {
            Slider.IsHitTestVisible = true;

            _opacityMaskScaleExpandAnim.To = Slider.ActualWidth / 40D;
            OpacityMaskRect.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _opacityMaskScaleExpandAnim);

            _sliderBackgroundWidthExpandAnim.To = Slider.ActualWidth;
            SliderBackgroundBorder.BeginAnimation(FrameworkElement.WidthProperty, _sliderBackgroundWidthExpandAnim);

            ((DropShadowEffect)SliderBackgroundBorder.Effect).BeginAnimation(DropShadowEffect.OpacityProperty, _effectOnAnim);
            SliderBackgroundBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(53, 61, 66), TimeSpan.FromMilliseconds(200)));

        }

        private void ActivatorOnMouseLeave(object sender, MouseEventArgs e)
        {
            if (Slider.IsMouseOver) return;
            Slider.IsHitTestVisible = false;

            OpacityMaskRect.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _opacityMaskScaleShrinkAnim);

            SliderBackgroundBorder.BeginAnimation(FrameworkElement.WidthProperty, _sliderBackgroundWidthShrinkAnim);

            ((DropShadowEffect)SliderBackgroundBorder.Effect).BeginAnimation(DropShadowEffect.OpacityProperty, _effectOffAnim);

            SliderBackgroundBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(45, 52, 56), TimeSpan.FromMilliseconds(200)));
        }

        private void OnSliderButtonClick(object sender, RoutedEventArgs e)
        {
            var target = (FrameworkElement)sender;

            var idx = SliderButtonsContainer.Children.IndexOf(target);
            var totY = idx * 40;
            _selRectAnim.To = totY;
            SelRect.RenderTransform.BeginAnimation(TranslateTransform.YProperty, _selRectAnim);
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var resize = ResizeMode;
                ResizeMode = ResizeMode.NoResize;
                DragMove();
                ResizeMode = resize;
            }
            catch { }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Save();
            BasicTeraData.Instance.HotkeysData.Save();
            Close();
            _visible = false;
        }

        public static void ShowWindow()
        {
            if (_visible) return;
            _visible = true;
            _window = new SettingsWindow();
            _window.Show();
            _window.Activate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.LeftShift) return;
            ((SettingsWindowViewModel) DataContext).IsShiftDown = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.LeftShift) return;
            ((SettingsWindowViewModel)DataContext).IsShiftDown = false;
        }
    }
}