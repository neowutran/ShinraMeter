using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TCC.UI.Controls.Settings
{
    public partial class ValueSetting
    {
        private int _clicks;
        private bool _spinnerPressed;
        private bool _increment;
        private readonly DispatcherTimer _spinnerTimer;

        public string SettingName
        {
            get => (string)GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }

        public static readonly DependencyProperty SettingNameProperty =
            DependencyProperty.Register("SettingName", typeof(string), typeof(ValueSetting));

        public Visibility TextBoxVisibility
        {
            get => (Visibility)GetValue(TextBoxVisibilityProperty);
            set => SetValue(TextBoxVisibilityProperty, value);
        }

        public static readonly DependencyProperty TextBoxVisibilityProperty =
            DependencyProperty.Register("TextBoxVisibility", typeof(Visibility), typeof(ValueSetting));

        public double Max
        {
            get => (double)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(double), typeof(ValueSetting));

        public double Min
        {
            get => (double)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(double), typeof(ValueSetting));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ValueSetting));

        public Geometry SvgIcon
        {
            get => (Geometry)GetValue(SvgIconProperty);
            set => SetValue(SvgIconProperty, value);
        }

        public static readonly DependencyProperty SvgIconProperty =
            DependencyProperty.Register("SvgIcon", typeof(Geometry), typeof(ValueSetting));

        public Brush SliderColor1
        {
            get => (Brush)GetValue(SliderColor1Property);
            set => SetValue(SliderColor1Property, value);
        }

        public static readonly DependencyProperty SliderColor1Property =
            DependencyProperty.Register("SliderColor1", typeof(Brush), typeof(ValueSetting), new PropertyMetadata(Brushes.Gray));

        public Brush SliderColor2
        {
            get => (Brush)GetValue(SliderColor2Property);
            set => SetValue(SliderColor2Property, value);
        }

        public static readonly DependencyProperty SliderColor2Property =
            DependencyProperty.Register("SliderColor2", typeof(Brush), typeof(ValueSetting), new PropertyMetadata(Brushes.LightSlateGray));

        public bool UseMin
        {
            get => (bool)GetValue(UseMinProperty);
            set => SetValue(UseMinProperty, value);
        }

        public static readonly DependencyProperty UseMinProperty =
            DependencyProperty.Register("UseMin", typeof(bool), typeof(ValueSetting), new PropertyMetadata(false));

        public bool UseMax
        {
            get => (bool)GetValue(UseMaxProperty);
            set => SetValue(UseMaxProperty, value);
        }

        public static readonly DependencyProperty UseMaxProperty =
            DependencyProperty.Register("UseMax", typeof(bool), typeof(ValueSetting), new PropertyMetadata(false));

        public bool UseSpinner
        {
            get => (bool)GetValue(UseSpinnerProperty);
            set => SetValue(UseSpinnerProperty, value);
        }

        public static readonly DependencyProperty UseSpinnerProperty =
            DependencyProperty.Register("UseSpinner", typeof(bool), typeof(ValueSetting), new PropertyMetadata(false));

        public GridLength SliderWidth
        {
            get => (GridLength)GetValue(SliderWidthProperty);
            set => SetValue(SliderWidthProperty, value);
        }

        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register("SliderWidth", typeof(GridLength), typeof(ValueSetting), new PropertyMetadata(new GridLength(100, GridUnitType.Pixel)));

        public GridLength TextWidth
        {
            get => (GridLength)GetValue(TextWidthProperty);
            set => SetValue(TextWidthProperty, value);
        }

        public static readonly DependencyProperty TextWidthProperty =
            DependencyProperty.Register("TextWidth", typeof(GridLength), typeof(ValueSetting), new PropertyMetadata(GridLength.Auto));

        public ValueSetting()
        {
            InitializeComponent();

            _spinnerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _spinnerTimer.Tick += OnSpinnerTimerTick;
        }

        private void OnSpinnerTimerTick(object sender, EventArgs e)
        {
            if (!_spinnerPressed) { _spinnerTimer.Stop(); return; }

            if (_increment) { Increment(); }
            else { Decrement(); }
        }

        private void Increment()
        {
            if (UseMax && Value >= Max) return;
            Value++;
        }

        private void Decrement()
        {
            if (UseMin && Value <= Min) return;
            Value--;
        }

        private void AddValue(object sender, MouseButtonEventArgs e)
        {
            Value = Math.Round(Value + 0.01, 2);
        }

        private void SubtractValue(object sender, MouseButtonEventArgs e)
        {
            Value = Math.Round(Value - 0.01, 2);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;

            var s = (Slider)sender;
            if (!s.IsMouseOver) return;

            Value = Math.Round(s.Value, 2);
        }

        private void Slider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Value = 1;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            try
            {
                var result = double.Parse(tb.Text, CultureInfo.InvariantCulture);
                if (result > Max) Value = Max;
                else if (result < Min) Value = Min;
                else Value = result;
            }
            catch (Exception)
            {
                tb.Text = Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /*
                private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
                {
                    var tb = sender as TextBox;
                    if (double.TryParse(tb.Text, out var result))
                    {
                        Value = result;
                    }
                    else
                    {
                        tb.Text = Value.ToString();
                    }
                }
        */

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Keyboard.ClearFocus();
        }

        private async void OnSpinnerUpMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _clicks++;
            _spinnerPressed = true;
            _increment = true;
            var clicks = _clicks;
            await Task.Delay(500);
            if (clicks != _clicks) return;
            if (_spinnerPressed) _spinnerTimer.Start();
        }

        private async void OnSpinnerDownMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _clicks++;
            _spinnerPressed = true;
            _increment = false;
            var clicks = _clicks;
            await Task.Delay(500);
            if (clicks != _clicks) return;
            if (_spinnerPressed) _spinnerTimer.Start();
        }

        private void OnSpinnerUpMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _spinnerPressed = false;
            _spinnerTimer.Stop();
        }

        private void OnSpinnerDownMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _spinnerPressed = false;
            _spinnerTimer.Stop();
        }

        private void OnDecrementClick(object sender, RoutedEventArgs e)
        {
            Decrement();
        }

        private void OnIncrementClick(object sender, RoutedEventArgs e)
        {
            Increment();
        }
    }
}