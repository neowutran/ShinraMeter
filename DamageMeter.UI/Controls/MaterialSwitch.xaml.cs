using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DamageMeter.UI
{
    public partial class MaterialSwitch
    {
        DoubleAnimation on;
        DoubleAnimation off;

        ColorAnimation fillOn;
        ColorAnimation fillOff;
        ColorAnimation backFillOff;
        ColorAnimation backFillOn;

        //Color onColor = ((SolidColorBrush)Application.Current.Resources["AccentColor"]).Color;
        //Color offColor = ((SolidColorBrush)Application.Current.Resources["bgColor"]).Color;
        //Color backOffColor = Colors.Black;

        private TimeSpan animationDuration = TimeSpan.FromMilliseconds(150);

        DependencyPropertyWatcher<bool> statusWatcher;

        public bool Status
        {
            get { return (bool)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(bool), typeof(MaterialSwitch), new PropertyMetadata(false));

        public new string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly new DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(MaterialSwitch));



        public SolidColorBrush SwitchColor
        {
            get { return (SolidColorBrush)GetValue(SwitchColorProperty); }
            set { SetValue(SwitchColorProperty, value); }
        }
        public static readonly DependencyProperty SwitchColorProperty = DependencyProperty.Register("SwitchColor", typeof(SolidColorBrush), typeof(MaterialSwitch), new PropertyMetadata(((SolidColorBrush)Application.Current.FindResource("AccentColor"))));




        public MaterialSwitch()
        {
            InitializeComponent();

            on = new DoubleAnimation(20, animationDuration) { EasingFunction = new QuadraticEase() };
            off = new DoubleAnimation(0, animationDuration) { EasingFunction = new QuadraticEase() };

            fillOn = new ColorAnimation(SwitchColor.Color, animationDuration) { EasingFunction = new QuadraticEase() };
            fillOff = new ColorAnimation(((SolidColorBrush)this.FindResource("ThumbOff")).Color, animationDuration) { EasingFunction = new QuadraticEase() };
            backFillOff = new ColorAnimation(((SolidColorBrush)this.FindResource("TrackOff")).Color, animationDuration) { EasingFunction = new QuadraticEase() };
            var TrackOn = new SolidColorBrush() { Color = SwitchColor.Color, Opacity = .5 };
            
            backFillOn = new ColorAnimation(TrackOn.Color, animationDuration) { EasingFunction = new QuadraticEase() };
            switchHead.Fill = (SolidColorBrush)this.FindResource("ThumbOff");
            switchBack.Fill = (SolidColorBrush)this.FindResource("TrackOff");

            switchHead.RenderTransform = new TranslateTransform(0, 0);
            statusWatcher = new DependencyPropertyWatcher<bool>(this, "Status");
            statusWatcher.PropertyChanged += StatusWatcher_PropertyChanged;

        }

        public event RoutedEventHandler Off
        {
            add { AddHandler(OffEvent, value); }
            remove { RemoveHandler(OffEvent, value); }
        }
        public static readonly RoutedEvent OffEvent = EventManager.RegisterRoutedEvent("OffEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MaterialSwitch));
        void RaiseOffEvent()
        {
            var e = new RoutedEventArgs(OffEvent, this);
            RaiseEvent(e);
        }

        public event RoutedEventHandler On
        {
            add { AddHandler(OnEvent, value); }
            remove { RemoveHandler(OnEvent, value); }
        }
        public static readonly RoutedEvent OnEvent = EventManager.RegisterRoutedEvent("OnEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MaterialSwitch));
        void RaiseOnEvent()
        {
            var e = new RoutedEventArgs(OnEvent, this);
            RaiseEvent(e);
        }


        private void StatusWatcher_PropertyChanged(object sender, EventArgs e)
        {
            if (Status)
            {
                AnimateOn();
                RaiseOnEvent();
            }
            else
            {
                AnimateOff();
                RaiseOffEvent();
            }
        }
        public void AnimateOn()
        {
            switchHead.RenderTransform.BeginAnimation(TranslateTransform.XProperty, on);
            fillOn.To = SwitchColor.Color;
            switchHead.Fill.BeginAnimation(SolidColorBrush.ColorProperty, fillOn);
            var TrackOn = new SolidColorBrush() { Color = SwitchColor.Color, Opacity = .5 };
            backFillOn.To = TrackOn.Color;
            switchBack.Fill.BeginAnimation(SolidColorBrush.ColorProperty, backFillOn);
        }
        public void AnimateOff()
        {
            switchHead.RenderTransform.BeginAnimation(TranslateTransform.XProperty, off);
            switchHead.Fill.BeginAnimation(SolidColorBrush.ColorProperty, fillOff);
            switchBack.Fill.BeginAnimation(SolidColorBrush.ColorProperty, backFillOff);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {


            if (Status)
            {
                AnimateOn();
            }
            else
            {
                AnimateOff();
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Status = !Status;
        }
    }
}
