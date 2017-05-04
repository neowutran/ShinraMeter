using DamageMeter.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DamageMeter.UI.HUD.Controls
{
    public partial class GenericGauge : UserControl, INotifyPropertyChanged
    {

        private int animTime = 200;
        private DoubleAnimation a;
        private DependencyPropertyWatcher<float> curValwatcher; //https://blogs.msdn.microsoft.com/flaviencharlon/2012/12/07/getting-change-notifications-from-any-dependency-property-in-windows-store-apps/
        public GenericGauge()
        {
            InitializeComponent();

            curValwatcher = new DependencyPropertyWatcher<float>(this, "CurrentVal");
            curValwatcher.PropertyChanged += CurValWatcher_PropertyChanged;
            a = new DoubleAnimation(0, TimeSpan.FromMilliseconds(animTime)) { EasingFunction = new QuadraticEase() };
            bar.RenderTransform = new ScaleTransform(1, 1, 0, .5);

        }

        private void CurValWatcher_PropertyChanged(object sender, EventArgs e)
        {
            if (MaxVal > 0)
            {
                Factor = curValwatcher.Value / MaxVal;
            }
            else
            {
                Factor = 0;
            }


        }
        double factor;
        public double Factor
        {
            get
            {
                return factor;
            }
            set
            {
                if(factor != value)
                {
                    factor = value;               
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Factor"));
                    AnimateBar(value);
                }
            }
        }

        private void AnimateBar(double factor)
        {
            if (factor > 1)
            {
                a.To = 1;
            }
            else
            {
                a.To = factor;
            }

            bar.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, a);

        }

        public SolidColorBrush BarColor
        {
            get { return (SolidColorBrush)GetValue(BarColorProperty); }
            set { SetValue(BarColorProperty, value); }
        }
        public static readonly DependencyProperty BarColorProperty = DependencyProperty.Register("BarColor", typeof(SolidColorBrush), typeof(GenericGauge));

        public string GaugeName
        {
            get { return (string)GetValue(GaugeNameProperty); }
            set { SetValue(GaugeNameProperty, value); }
        }
        public static readonly DependencyProperty GaugeNameProperty = DependencyProperty.Register("GaugeName", typeof(string), typeof(GenericGauge));

        public int MaxVal
        {
            get { return (int)GetValue(MaxValProperty); }
            set { SetValue(MaxValProperty, value); }
        }
        public static readonly DependencyProperty MaxValProperty = DependencyProperty.Register("MaxVal", typeof(int), typeof(GenericGauge));
        
        public float CurrentVal
        {
            get { return (float)GetValue(CurrentValProperty); }
            set { SetValue(CurrentValProperty, value); }
        }
        public static readonly DependencyProperty CurrentValProperty = DependencyProperty.Register("CurrentVal", typeof(float), typeof(GenericGauge));

        public bool ShowPercentage
        {
            get { return (bool)GetValue(ShowPercentageProperty); }
            set { SetValue(ShowPercentageProperty, value); }
        }
        public static readonly DependencyProperty ShowPercentageProperty = DependencyProperty.Register("ShowPercentage", typeof(bool), typeof(GenericGauge));
               
        public bool ShowValues
        {
            get { return (bool)GetValue(ShowValuesProperty); }
            set { SetValue(ShowValuesProperty, value); }
        }
        public static readonly DependencyProperty ShowValuesProperty = DependencyProperty.Register("ShowValues", typeof(bool), typeof(GenericGauge));

        public bool ShowName
        {
            get { return (bool)GetValue(ShowNameProperty); }
            set { SetValue(ShowNameProperty, value); }
        }
        public static readonly DependencyProperty ShowNameProperty = DependencyProperty.Register("ShowName", typeof(bool), typeof(GenericGauge));

        public event PropertyChangedEventHandler PropertyChanged;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}

