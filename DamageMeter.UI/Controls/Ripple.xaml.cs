using Nostrum.Factories;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Nostrum.Controls
{
    public partial class Ripple
    {
        /* Fields ***********************************************/

        private readonly DoubleAnimation _scaleRipple;
        private readonly DoubleAnimation _fadeRipple;
        private bool _firstClick = true;


        /* Constructor ******************************************/

        public Ripple()
        {
            InitializeComponent();
            _scaleRipple = AnimationFactory.CreateDoubleAnimation(Duration, 20, 0, true);
            _fadeRipple = AnimationFactory.CreateDoubleAnimation(Duration, 0, 1, true);
        }


        /* Dependency properties ********************************/

        public int Duration
        {
            get => (int) GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(int), typeof(Ripple), new PropertyMetadata(650));

        public Brush Color
        {
            get => (Brush) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(Ripple), new PropertyMetadata(Brushes.White));

        public bool StaysVisible
        {
            get => (bool) GetValue(StaysVisibleProperty);
            set => SetValue(StaysVisibleProperty, value);
        }

        public static readonly DependencyProperty StaysVisibleProperty =
            DependencyProperty.Register("StaysVisible", typeof(bool), typeof(Ripple), new PropertyMetadata(false));
        public bool Reversed
        {
            get => (bool)GetValue(ReversedProperty);
            set => SetValue(ReversedProperty, value);
        }

        public static readonly DependencyProperty ReversedProperty =
    DependencyProperty.Register("Reversed", typeof(bool), typeof(Ripple), new PropertyMetadata(false));


        //public RippleScaleDirection Direction
        //{
        //    get => (RippleScaleDirection) GetValue(DirectionProperty);
        //    set => SetValue(DirectionProperty, value);
        //}

        //public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction",
        //    typeof(RippleScaleDirection), typeof(Ripple), new PropertyMetadata(RippleScaleDirection.Up));


        public bool Enabled
        {
            get => (bool) GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(Ripple), new PropertyMetadata(true));


        /* Methods **********************************************/

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_firstClick) Reset();
            if (StaysVisible)
            {
                ripple.BeginAnimation(OpacityProperty, null);
                ripple.Opacity = 1;
            }
            var pos = e?.MouseDevice.GetPosition(this) ?? Mouse.GetPosition(this);

            var w = pos.X - ripple.Width / 2;
            var h = pos.Y - ripple.Height / 2;
            if (Reversed) Shrink(w, h);
            else /******/ Expand(w, h);

        }

        private void Shrink(double w, double h)
        {
            var scaleTrans = ((TransformGroup) ripple.RenderTransform).Children[0];
            ((TransformGroup) ripple.RenderTransform).Children[1] = new TranslateTransform(w, h);
            //var fac = ActualWidth * ActualHeight / 150;
            _scaleRipple.To = 1;
            _scaleRipple.From = null;
            _scaleRipple.Duration = TimeSpan.FromMilliseconds(Duration);
            _scaleRipple.Completed += HideRipple;
            scaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, _scaleRipple);
            scaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, _scaleRipple);
        }

        private void HideRipple(object s, object e)
        {
            var scaleTrans = (ScaleTransform) ((TransformGroup) ripple.RenderTransform).Children[0];

            if (scaleTrans.ScaleX == 1) ripple.Visibility = Visibility.Collapsed;
            _scaleRipple.Completed -= HideRipple;
        }

        private void Expand(double w, double h)
        {
            ripple.Visibility = Visibility.Visible;
            var scaleTrans = ((TransformGroup) ripple.RenderTransform).Children[0];
            ((TransformGroup) ripple.RenderTransform).Children[1] = new TranslateTransform(w, h);
            _scaleRipple.To = GetRadius(w, h) / 5;
            _scaleRipple.Duration = TimeSpan.FromMilliseconds(Duration);
            scaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, _scaleRipple);
            scaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, _scaleRipple);
            if (!StaysVisible) ripple.BeginAnimation(OpacityProperty, _fadeRipple);
        }

        private double GetRadius(double x1, double y1)
        {
            var parentW = ActualWidth;
            var parentH = ActualHeight;

            var x2 = parentW - x1;
            var y2 = parentH - y1;

            var r1 = Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2));
            var r2 = Math.Sqrt(Math.Pow(y1, 2) + Math.Pow(x2, 2));
            var r3 = Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y2, 2));
            var r4 = Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2));

            return Math.Max(Math.Max(r1, r2), Math.Max(r3, r4)) * 1.05;
        }

        public void Reset()
        {
            Dispatcher?.Invoke(() => ripple.Opacity = 0);
            _firstClick = true;
        }

        public void Trigger(MouseButtonEventArgs e = null)
        {
            Dispatcher?.BeginInvoke(new Action(() => OnPreviewMouseLeftButtonDown(null, e)));
        }
    }
}


//using System;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;

//namespace TCC.Controls
//{
//    public partial class Ripple
//    {
//        private const int AnimTime = 650;

//        public Ripple()
//        {
//            InitializeComponent();
//            _scaleRipple = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(AnimTime)) { EasingFunction = new QuadraticEase() };
//            _fadeRipple = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(AnimTime)) { EasingFunction = new QuadraticEase() };
//        }

//        private readonly DoubleAnimation _scaleRipple;
//        private readonly DoubleAnimation _fadeRipple;
//        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//        {
//            var scaleTrans = (RippleCircle.RenderTransform as TransformGroup)?.Children[0];
//            ((TransformGroup) RippleCircle.RenderTransform).Children[1] = new TranslateTransform(
//                e.MouseDevice.GetPosition(this).X - RippleCircle.Width / 2,
//                e.MouseDevice.GetPosition(this).Y - RippleCircle.Height / 2);
//            var fac = ActualWidth * ActualHeight / 100;
//            _scaleRipple.To = fac + 5;
//            _scaleRipple.Duration = TimeSpan.FromMilliseconds(fac/30* AnimTime);
//            if (scaleTrans != null)
//            {
//                scaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, _scaleRipple);
//                scaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, _scaleRipple);
//            }

//            RippleCircle.BeginAnimation(OpacityProperty, _fadeRipple);
//        }
//    }
//}