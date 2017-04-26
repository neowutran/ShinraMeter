using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Data;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.UI.HUD.Controls
{
    public partial class BossGage : UserControl
    {
        NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 0 };

        private Boss _boss;
        private float _maxHp;
        private float _currentHp;
        private bool _enraged;
        public float CurrentPercentage => _maxHp==0? 0 : (_currentHp / _maxHp) * 100;

        int AnimationTime = 150;
        float LastEnragePercent = 100;
        int EnrageDuration = 36000;
        int CurrentEnrageTime = 36;

        Timer NumberTimer = new Timer(1000);

        public BossGage()
        {
            InitializeComponent();

            EnrageImage.ImageSource = BasicTeraData.Instance.ImageDatabase.Enraged.Source;

            NextEnrageTB.DataContext = this;

            EnrageGrid.RenderTransform = new ScaleTransform(0, 1, 0, .5);

            SlideAnimation.EasingFunction = new QuadraticEase();
            ColorChangeAnimation.EasingFunction = new QuadraticEase();
            DoubleAnimation.EasingFunction = new QuadraticEase();
            SlideAnimation.Duration = TimeSpan.FromMilliseconds(250);
            ColorChangeAnimation.Duration = TimeSpan.FromMilliseconds(AnimationTime);
            DoubleAnimation.Duration = TimeSpan.FromMilliseconds(AnimationTime);
            NextEnrage.RenderTransform = new TranslateTransform(BaseRect.Width * 0.9, 0);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _boss = (Boss) DataContext;
            _boss.PropertyChanged += boss_PropertyChanged;
            HPrect.Fill = new SolidColorBrush(Color.FromArgb(0xff, 0x4a, 0x82, 0xbd));
            _currentHp = _boss.CurrentHP;
            _maxHp = _boss.MaxHP;
            _enraged = _boss.Enraged;
            DoubleAnimation.To = ValueToLength(_currentHp, _maxHp);
            HPrect.BeginAnimation(WidthProperty, DoubleAnimation);
            SetEnragePercTB(CurrentPercentage - 10);
            SlideNextEnrage(CurrentPercentage - 10);
        }
        private void boss_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentHP")
            {
                _currentHp = ((Boss)sender).CurrentHP;
                if (_currentHp > _maxHp) _maxHp = _currentHp;
                DoubleAnimation.To = ValueToLength(_currentHp, _maxHp);
                HPrect.BeginAnimation(WidthProperty, DoubleAnimation);

                if (_enraged)
                {
                    SlideNextEnrage(CurrentPercentage);
                    SetEnragePercTB(CurrentPercentage);
                }
            }
            if (e.PropertyName == "MaxHP")
            {
                _maxHp = ((Boss)sender).MaxHP;
            }
            if (e.PropertyName == "Enraged")
            {
                var value = ((Boss)sender).Enraged;
                if (_enraged == value) return;
                _enraged = value;
                if (_enraged)
                {
                    number.Text = CurrentEnrageTime.ToString();

                    SlideNextEnrage(CurrentPercentage);
                    SetEnragePercTB(CurrentPercentage);

                    GlowOn();
                    DeployEnrageGrid();
                }
                else
                {
                    LastEnragePercent = CurrentPercentage;
                    //Console.WriteLine("Last enrage percentage = {0} ({1}/{2})", LastEnragePercent, CurrentHP, MaxHP);
                    NumberTimer?.Stop();

                    GlowOff();
                    CloseEnrageGrid();

                    SlideNextEnrage(CurrentPercentage - 10);
                    SetEnragePercTB(CurrentPercentage - 10);

                    CurrentEnrageTime = 36;
                }
            }
            if (e.PropertyName == "Visible")
            {
                Visibility = ((Boss)sender).Visible;
            }
        }

        void SetEnragePercTB(double v)
        {
                if (v < 0) v = 0;
                NextEnrageTB.Text = String.Format("{0:0.#}", v);
        }
        void SlideNextEnrage(double val)
        {
                if(val < 0)
                {
                    SlideAnimation.To = 0;
                }
                else
                {
                    SlideAnimation.To = BaseRect.ActualWidth * (val/100);
                }

                NextEnrage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, SlideAnimation);
        }
        void SlideNextEnrageDirect()
        {
                SlideAnimation.To = HPrect.ActualWidth;
                NextEnrage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, SlideAnimation);
        }
        void GlowOn()
        {
                DoubleAnimation.To = 1;
                HPrect.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, DoubleAnimation);

                ColorChangeAnimation.To = Colors.Red;
                HPrect.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ColorChangeAnimation);
        }
        void GlowOff()
        {
                DoubleAnimation.To = 0;
                HPrect.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, DoubleAnimation);

                ColorChangeAnimation.To = Color.FromArgb(0xff, 0x4a, 0x82, 0xbd);
                HPrect.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ColorChangeAnimation);
        }
        void DeployEnrageGrid()
        {
                DoubleAnimation.To = 1;
                EnrageGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, DoubleAnimation);

                EnrageArc.BeginAnimation(Arc.EndAngleProperty, new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(EnrageDuration)));

                NumberTimer = new Timer(1000);
                NumberTimer.Elapsed += (s, ev) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        number.Text = CurrentEnrageTime.ToString();
                        CurrentEnrageTime--;
                    }));
                };
                NumberTimer.Enabled = true;
        }
        void CloseEnrageGrid()
        {
                DoubleAnimation.To = 0;
                EnrageGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, DoubleAnimation);

        }

        double ValueToLength(double value, double maxValue)
        {
            if (maxValue == 0)
            {
                return 0;
            }
            else
            {
                double n = BaseRect.ActualWidth * ((double)value / (double)maxValue);
                return n;
            }

        }

        static DoubleAnimation SlideAnimation = new DoubleAnimation();
        static ColorAnimation ColorChangeAnimation = new ColorAnimation();
        static DoubleAnimation DoubleAnimation = new DoubleAnimation();

        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            _boss.PropertyChanged -= boss_PropertyChanged;
            _boss = null;
        }
    }
}
