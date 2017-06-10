using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DamageMeter.UI
{
    // from https://www.codeproject.com/Articles/48871/Friction-Scrolling-Now-An-WPF-Attached-Behaviour-T - not use since it works only when dragging
    public class KineticBehaviour
    {
        public static readonly DependencyProperty FrictionProperty = DependencyProperty.RegisterAttached("Friction", typeof(double), typeof(KineticBehaviour), new FrameworkPropertyMetadata((double)0.95));
        public static double GetFriction(DependencyObject d)
        {
            return (double)d.GetValue(FrictionProperty);
        }

        public static void SetFriction(DependencyObject d, double value)
        {
            d.SetValue(FrictionProperty, value);
        }

        private static readonly DependencyProperty ScrollStartPointProperty = DependencyProperty.RegisterAttached("ScrollStartPoint", typeof(Point), typeof(KineticBehaviour), new FrameworkPropertyMetadata((Point)new Point()));

        /// <span class="code-SummaryComment"><summary>
        /// Gets the ScrollStartPoint property.  
        /// <span class="code-SummaryComment"></summary>
        private static Point GetScrollStartPoint(DependencyObject d)
        {
            return (Point)d.GetValue(ScrollStartPointProperty);
        }

        /// <span class="code-SummaryComment"><summary>
        /// Sets the ScrollStartPoint property.  
        /// <span class="code-SummaryComment"></summary>
        private static void SetScrollStartPoint(DependencyObject d, Point value)
        {
            d.SetValue(ScrollStartPointProperty, value);
        }

        private static readonly DependencyProperty ScrollStartOffsetProperty = DependencyProperty.RegisterAttached("ScrollStartOffset", typeof(Point), typeof(KineticBehaviour), new FrameworkPropertyMetadata((Point)new Point()));

        /// <span class="code-SummaryComment"><summary>
        /// Gets the ScrollStartOffset property.  
        /// <span class="code-SummaryComment"></summary>
        private static Point GetScrollStartOffset(DependencyObject d)
        {
            return (Point)d.GetValue(ScrollStartOffsetProperty);
        }

        /// <span class="code-SummaryComment"><summary>
        /// Sets the ScrollStartOffset property.  
        /// <span class="code-SummaryComment"></summary>
        private static void SetScrollStartOffset(DependencyObject d, Point value)
        {
            d.SetValue(ScrollStartOffsetProperty, value);
        }

        /// <span class="code-SummaryComment"><summary>
        /// InertiaProcessor Attached Dependency Property
        /// <span class="code-SummaryComment"></summary>
        private static readonly DependencyProperty InertiaProcessorProperty = DependencyProperty.RegisterAttached("InertiaProcessor", typeof(InertiaHandler), typeof(KineticBehaviour), new FrameworkPropertyMetadata((InertiaHandler)null));
        private static InertiaHandler GetInertiaProcessor(DependencyObject d)
        {
            return (InertiaHandler)d.GetValue(InertiaProcessorProperty);
        }

        /// <span class="code-SummaryComment"><summary>
        /// Sets the InertiaProcessor property.  
        /// <span class="code-SummaryComment"></summary>
        private static void SetInertiaProcessor(DependencyObject d, InertiaHandler value)
        {
            d.SetValue(InertiaProcessorProperty, value);
        }

        /// <span class="code-SummaryComment"><summary>
        /// HandleKineticScrolling Attached Dependency Property
        /// <span class="code-SummaryComment"></summary>
        public static readonly DependencyProperty HandleKineticScrollingProperty = DependencyProperty.RegisterAttached("HandleKineticScrolling", typeof(bool), typeof(KineticBehaviour), new FrameworkPropertyMetadata((bool)false, new PropertyChangedCallback(OnHandleKineticScrollingChanged)));

        /// <span class="code-SummaryComment"><summary>
        /// Gets the HandleKineticScrolling property.  
        /// <span class="code-SummaryComment"></summary>
        public static bool GetHandleKineticScrolling(DependencyObject d)
        {
            return (bool)d.GetValue(HandleKineticScrollingProperty);
        }

        /// <span class="code-SummaryComment"><summary>
        /// Sets the HandleKineticScrolling property.  
        /// <span class="code-SummaryComment"></summary>
        public static void SetHandleKineticScrolling(DependencyObject d,
     bool value)
        {
            d.SetValue(HandleKineticScrollingProperty, value);
        }

        /// <span class="code-SummaryComment"><summary>
        /// Handles changes to the HandleKineticScrolling property.
        /// <span class="code-SummaryComment"></summary>
        private static void OnHandleKineticScrollingChanged(DependencyObject d,
     DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scoller = d as ScrollViewer;
            if ((bool)e.NewValue)
            {
                scoller.PreviewMouseDown += OnPreviewMouseDown;
                scoller.PreviewMouseMove += OnPreviewMouseMove;
                scoller.PreviewMouseUp += OnPreviewMouseUp;
                SetInertiaProcessor(scoller, new InertiaHandler(scoller));
            }
            else
            {
                scoller.PreviewMouseDown -= OnPreviewMouseDown;
                scoller.PreviewMouseMove -= OnPreviewMouseMove;
                scoller.PreviewMouseUp -= OnPreviewMouseUp;
                var inertia = GetInertiaProcessor(scoller);
                if (inertia != null)
                    inertia.Dispose();
            }

        }


        private static void OnPreviewMouseDown(object sender,
            MouseButtonEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.IsMouseOver)
            {
                // Save starting point, used later when 
                //determining how much to scroll.
                SetScrollStartPoint(scrollViewer,
                    e.GetPosition(scrollViewer));
                SetScrollStartOffset(scrollViewer,
                    new Point(scrollViewer.HorizontalOffset,
                        scrollViewer.VerticalOffset));
                scrollViewer.CaptureMouse();
            }
        }


        private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(scrollViewer);

                var scrollStartPoint = GetScrollStartPoint(scrollViewer);
                // Determine the new amount to scroll.
                Point delta = new Point(scrollStartPoint.X -
                    currentPoint.X, scrollStartPoint.Y - currentPoint.Y);

                var scrollStartOffset = GetScrollStartOffset(scrollViewer);
                Point scrollTarget = new Point(scrollStartOffset.X +
                    delta.X, scrollStartOffset.Y + delta.Y);

                var inertiaProcessor = GetInertiaProcessor(scrollViewer);
                if (inertiaProcessor != null)
                    inertiaProcessor.ScrollTarget = scrollTarget;

                // Scroll to the new position.
                scrollViewer.ScrollToHorizontalOffset(scrollTarget.X);
                scrollViewer.ScrollToVerticalOffset(scrollTarget.Y);
            }
        }

        private static void OnPreviewMouseUp(object sender,
            MouseButtonEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.IsMouseCaptured)
            {
                scrollViewer.ReleaseMouseCapture();
            }
        }

        /// <span class="code-SummaryComment"><summary>
        /// Handles the inertia 
        /// <span class="code-SummaryComment"></summary>
        class InertiaHandler : IDisposable
        {
            private Point previousPoint;
            private Vector velocity;
            ScrollViewer scroller;
            DispatcherTimer animationTimer;

            private Point scrollTarget;
            public Point ScrollTarget
            {
                get { return scrollTarget; }
                set { scrollTarget = value; }
            }

            public InertiaHandler(ScrollViewer scroller)
            {
                this.scroller = scroller;
                animationTimer = new DispatcherTimer();
                animationTimer.Interval =
                    new TimeSpan(0, 0, 0, 0, 20);
                animationTimer.Tick +=
                    new EventHandler(HandleWorldTimerTick);
                animationTimer.Start();
            }

            private void HandleWorldTimerTick(object sender,
                EventArgs e)
            {
                if (scroller.IsMouseCaptured)
                {
                    Point currentPoint = Mouse.GetPosition(scroller);
                    velocity = previousPoint - currentPoint;
                    previousPoint = currentPoint;
                }
                else
                {
                    if (velocity.Length > 1)
                    {
                        scroller.ScrollToHorizontalOffset(
                            ScrollTarget.X);
                        scroller.ScrollToVerticalOffset(
                            ScrollTarget.Y);
                        scrollTarget.X += velocity.X;
                        scrollTarget.Y += velocity.Y;
                        velocity *=
                            KineticBehaviour.GetFriction(scroller);
                    }
                }
            }

            public void Dispose()
            {
                animationTimer.Stop();
            }
        }
    }
}
