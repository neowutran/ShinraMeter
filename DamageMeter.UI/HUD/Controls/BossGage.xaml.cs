using System;
using System.ComponentModel;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tera.Game;

namespace DamageMeter.UI.HUD.Controls
{
    /// <summary>
    ///     Logica di interazione per BossGage.xaml
    /// </summary>
    public partial class BossGage : INotifyPropertyChanged
    {
        private static readonly DoubleAnimation SlideAnimation = new DoubleAnimation();
        private static readonly ColorAnimation ColorChangeAnimation = new ColorAnimation();
        private static readonly DoubleAnimation DoubleAnimation = new DoubleAnimation();

        private readonly int AnimationTime = 350;
        private Boss _boss;
        private float _currentHp;
        private bool _enraged;
        private float _maxHp;


        private Color BaseHpColor = Color.FromRgb(0x00, 0x97, 0xce);
        private int curEnrageTime = 36;


        private float nextEnragePerc = 90;
        private NumberFormatInfo nfi = new NumberFormatInfo {NumberGroupSeparator = ".", NumberDecimalDigits = 0};


        private Timer NumberTimer = new Timer(1000);

        public BossGage()
        {
            InitializeComponent();

            if (PacketProcessor.Instance.EntityTracker?.MeterUser?.RaceGenderClass.Class != PlayerClass.Valkyrie) RunemarksGrid.Visibility=Visibility.Collapsed;
            SlideAnimation.EasingFunction = new QuadraticEase();
            ColorChangeAnimation.EasingFunction = new QuadraticEase();
            DoubleAnimation.EasingFunction = new QuadraticEase();
            SlideAnimation.Duration = TimeSpan.FromMilliseconds(250);
            ColorChangeAnimation.Duration = TimeSpan.FromMilliseconds(AnimationTime);
            DoubleAnimation.Duration = TimeSpan.FromMilliseconds(AnimationTime);
        }

        public float CurrentPercentage => _maxHp == 0 ? 0 : _currentHp / _maxHp * 100;

        public float NextEnragePercentage
        {
            get => nextEnragePerc;
            set
            {
                if (nextEnragePerc != value)
                {
                    nextEnragePerc = value;
                    if (value < 0) { nextEnragePerc = 0; }
                    NotifyPropertyChanged("NextEnragePercentage");
                    NotifyPropertyChanged("EnrageTBtext");
                }
            }
        }

        public int CurrentEnrageTime
        {
            get => curEnrageTime;
            set
            {
                if (curEnrageTime != value)
                {
                    curEnrageTime = value;
                    NotifyPropertyChanged("CurrentEnrageTime");
                    NotifyPropertyChanged("EnrageTBtext");
                }
            }
        }

        public string EnrageTBtext
        {
            get
            {
                if (_enraged) { return CurrentEnrageTime + "s"; }
                return NextEnragePercentage.ToString(CultureInfo.InvariantCulture);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string pr)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pr));
        }

        private void boss_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentHP")
            {
                _currentHp = ((Boss) sender).CurrentHP;
                if (_currentHp > _maxHp) { _maxHp = _currentHp; }
                DoubleAnimation.To = ValueToLength(_currentHp, _maxHp);

                if (_enraged) { SlideEnrageIndicator(CurrentPercentage); }
            }
            if (e.PropertyName == "MaxHP") { _maxHp = ((Boss) sender).MaxHP; }
            if (e.PropertyName == "Enraged")
            {
                var value = ((Boss) sender).Enraged;
                if (_enraged == value) { return; }
                _enraged = value;
                if (_enraged)
                {
                    SlideEnrageIndicator(CurrentPercentage);
                    NumberTimer = new Timer(1000);
                    NumberTimer.Elapsed += (s, ev) => { Dispatcher.BeginInvoke(new Action(() => { CurrentEnrageTime--; })); };
                    NumberTimer.Enabled = true;
                }
                else
                {
                    NumberTimer?.Stop();

                    NextEnragePercentage = CurrentPercentage - 10;
                    SlideEnrageIndicator(NextEnragePercentage);

                    CurrentEnrageTime = 36;
                }
            }
            if (e.PropertyName == "Visible") { Visibility = ((Boss) sender).Visible; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _boss = (Boss) DataContext;
            _boss.PropertyChanged += boss_PropertyChanged;

            _currentHp = _boss.CurrentHP;
            _maxHp = _boss.MaxHP;
            _enraged = _boss.Enraged;
            DoubleAnimation.To = ValueToLength(_currentHp, _maxHp);
            NextEnragePercentage = CurrentPercentage - 10;
            SlideEnrageIndicator(NextEnragePercentage);
            NextEnrage.RenderTransform = new TranslateTransform(HPgauge.Width * .9, 0);
        }

        private void SlideEnrageIndicator(double val)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (val < 0) { SlideAnimation.To = 0; }
                else { SlideAnimation.To = HPgauge.ActualWidth * (val / 100); }

                NextEnrage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, SlideAnimation);
            }));
        }

        private double ValueToLength(double value, double maxValue)
        {
            if (maxValue == 0) { return 1; }
            var n = value / maxValue;
            return n;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            NumberTimer.Enabled = false;
            NumberTimer.Dispose();
            //_boss.PropertyChanged -= boss_PropertyChanged; // seems not needed
            _boss = null;
        }
    }
}