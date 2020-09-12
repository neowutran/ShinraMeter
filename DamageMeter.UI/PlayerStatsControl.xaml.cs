using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DamageMeter.UI
{
    public partial class PlayerStatsControl : INotifyPropertyChanged
    {
        private readonly DoubleAnimation _animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) {EasingFunction = new QuadraticEase()};

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                N();
            }
        }

        public PlayerStatsControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is PlayerDamageViewModel vm)) return;
            OnDamageFactorChanged(vm.DamageFactor);
            vm.DamageFactorChanged += OnDamageFactorChanged;
        }

        private void OnDamageFactorChanged(double factor)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _animation.To = factor;
                _animation.Duration = TimeSpan.FromSeconds(0.1);
                IndicatorGovernor.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _animation);
            });

        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e);
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            IsHovered = true;
        }
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsHovered = false;
        }

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void N([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PlayerDamageViewModel dc)
            {
                dc.ShowSkillDetailsCommand.Execute(null);
            }
        }
    }
}
