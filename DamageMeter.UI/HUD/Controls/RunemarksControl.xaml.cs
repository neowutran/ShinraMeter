using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace DamageMeter.UI.HUD.Controls
{
    /// <summary>
    /// Logica di interazione per RunemarksControl.xaml
    /// </summary>
    public partial class RunemarksControl : UserControl
    {
        public RunemarksControl()
        {
            InitializeComponent();
            baseBorder.Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x27));

        }

        private void _context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Runemarks")
            {
                SetRunes(_context.Runmarks);
            }
            else if (e.PropertyName == "MaxRunemarks")
            {
                //baseBorder.Background = new SolidColorBrush(Color.FromRgb(0xff,0x98,0xbb));
                maxBorder.Opacity = 1;
            }
        }
        private int _currentRunes = 0;

        private void SetRunes(int newRunes)
        {
            var diff = newRunes - _currentRunes;

            if (diff == 0) return;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    dotsContainer.Children[_currentRunes + i].Opacity = 1;
                }
            }
            else
            {
                //baseBorder.Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x27));
                maxBorder.Opacity = 0;

                for (int i = dotsContainer.Children.Count - 1; i >= 0; i--)
                {
                    dotsContainer.Children[i].Opacity = 0;
                }
            }
            _currentRunes = newRunes;
        }

        Boss _context;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            _context = (Boss)DataContext;
            _context.PropertyChanged += _context_PropertyChanged;
        }
    }
}
