using System.ComponentModel;
using System.Windows;
using System.Windows.Media;


namespace DamageMeter.UI.HUD.Controls
{
    public partial class RunemarksControl
    {
        public RunemarksControl()
        {
            InitializeComponent();
            baseBorder.Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x27));
        }

        private void _context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_context.Runmarks)) { SetRunes(_context.Runmarks); }
        }

        private int _currentRunes = 0;

        private void SetRunes(int newRunes)
        {
            var diff = newRunes - _currentRunes;

            if (diff == 0) return;
            if (newRunes == 7)
            {
                //baseBorder.Background = new SolidColorBrush(Color.FromRgb(0xff,0x98,0xbb));
                maxBorder.Opacity = 1;
            }
            if (diff > 0) { for (var i = 0; i < diff; i++) { dotsContainer.Children[_currentRunes + i].Opacity = 1; } }
            else
            {
                //baseBorder.Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x27));
                maxBorder.Opacity = 0;

                for (var i = dotsContainer.Children.Count - 1; i >= 0; i--) { dotsContainer.Children[i].Opacity = 0; }
            }
            _currentRunes = newRunes;
        }

        private Boss _context;

        private void ControlDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _context = (Boss)e.NewValue;
            if (_context == null) return;
            _context.PropertyChanged += _context_PropertyChanged;
        }
    }
}
