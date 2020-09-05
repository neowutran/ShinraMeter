using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Nostrum.Factories
{
    /// <summary>
    /// Factory class used to quickly create animations in a less verbose way.
    /// </summary>
    public static class AnimationFactory
    {
        /// <summary>
        /// Creates a <see cref="DoubleAnimation"/> and sets its parameters, then returns it.
        /// </summary>
        public static DoubleAnimation CreateDoubleAnimation(int ms, double to, double from = double.NaN, bool easing = false, EventHandler completed = null, int framerate = 60, int delay = 0)
        {
            var ret = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(ms),
                To = to,
                BeginTime = TimeSpan.FromMilliseconds(delay)
            };
            if (!double.IsNaN(from)) ret.From = from;
            if (easing) ret.EasingFunction = new QuadraticEase();
            if (completed != null) ret.Completed += completed;
            Timeline.SetDesiredFrameRate(ret, framerate);
            return ret;
        }

        /// <summary>
        /// Creates a <see cref="ThicknessAnimation"/> and sets its parameters, then returns it.
        /// </summary>
        public static ThicknessAnimation CreateThicknessAnimation(int ms, Thickness to, bool easing = false, EventHandler completed = null, int framerate = 60, int delay = 0)
        {
            var ret = new ThicknessAnimation
            {
                Duration = TimeSpan.FromMilliseconds(ms),
                To = to,
                BeginTime = TimeSpan.FromMilliseconds(delay)
            };
            if (easing) ret.EasingFunction = new QuadraticEase();
            if (completed != null) ret.Completed += completed;
            Timeline.SetDesiredFrameRate(ret, framerate);
            return ret;
        }

        /// <summary>
        /// <inheritdoc cref="CreateThicknessAnimation(int,Thickness,bool,EventHandler,int,int)"/>
        /// </summary>
        public static ThicknessAnimation CreateThicknessAnimation(int ms, Thickness to, Thickness from, bool easing = false, EventHandler completed = null, int framerate = 60, int delay = 0)
        {
            var ret = CreateThicknessAnimation(ms, to, easing: easing, completed, framerate, delay);
            ret.From = from;
            return ret;
        }

        /// <summary>
        /// Creates a <see cref="ColorAnimation"/> and sets its parameters, then returns it.
        /// </summary>
        public static ColorAnimation CreateColorAnimation(int ms, bool easing = false)
        {
            var ret = new ColorAnimation { Duration = TimeSpan.FromMilliseconds(ms) };
            if (easing) ret.EasingFunction = new QuadraticEase();
            return ret;
        }

        /// <summary>
        /// <inheritdoc cref="CreateColorAnimation(int,bool)"/>
        /// </summary>
        public static ColorAnimation CreateColorAnimation(int ms, Color to, bool easing = false)
        {
            var ret = CreateColorAnimation(ms, easing: easing);
            ret.To = to;
            return ret;
        }

        /// <summary>
        /// <inheritdoc cref="CreateColorAnimation(int,bool)"/>
        /// </summary>
        public static ColorAnimation CreateColorAnimation(int ms, Color to, Color from, bool easing = false)
        {
            var ret = CreateColorAnimation(ms, to, easing: easing);
            ret.From = from;
            return ret;
        }
    }
}
