using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logica di interazione per MaterialSwitch.xaml
    /// </summary>
    public partial class MaterialSwitch : UserControl
    {
        DoubleAnimation on;
        DoubleAnimation off;

        ColorAnimation fillOn;
        ColorAnimation fillOff;
        ColorAnimation backFillOff;

        Color onColor = ((SolidColorBrush)Application.Current.Resources["mainColor"]).Color;
        Color offColor = ((SolidColorBrush)Application.Current.Resources["bgColor"]).Color;
        Color backOffColor = Colors.Black;

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

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(MaterialSwitch));

        public MaterialSwitch()
        {
            InitializeComponent();

            on = new DoubleAnimation(20, animationDuration) { EasingFunction = new QuadraticEase() };
            off = new DoubleAnimation(0, animationDuration) { EasingFunction = new QuadraticEase() };

            fillOn = new ColorAnimation(onColor, animationDuration) { EasingFunction = new QuadraticEase() };
            fillOff = new ColorAnimation(offColor, animationDuration) { EasingFunction = new QuadraticEase() };
            backFillOff = new ColorAnimation(backOffColor, animationDuration) { EasingFunction = new QuadraticEase() };
            switchHead.RenderTransform = new TranslateTransform(0, 0);
            switchHead.Fill = new SolidColorBrush(offColor);
            switchBack.Fill = new SolidColorBrush(backOffColor);

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
            RoutedEventArgs e = new RoutedEventArgs(OffEvent, this);
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
            RoutedEventArgs e = new RoutedEventArgs(OnEvent, this);
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
            switchHead.Fill.BeginAnimation(SolidColorBrush.ColorProperty, fillOn);
            switchBack.Fill.BeginAnimation(SolidColorBrush.ColorProperty, fillOn);
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
