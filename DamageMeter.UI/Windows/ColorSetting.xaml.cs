using Nostrum;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DamageMeter.UI.Windows
{
    public partial class ColorSetting
    {
        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorSetting), new PropertyMetadata(Colors.White));

        public Color DefaultColor
        {
            get => (Color)GetValue(DefaultColorProperty);
            set => SetValue(DefaultColorProperty, value);
        }
        public static readonly DependencyProperty DefaultColorProperty = DependencyProperty.Register("DefaultColor", typeof(Color), typeof(ColorSetting), new PropertyMetadata(Colors.White));

        public string SettingName
        {
            get => (string)GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }
        public static readonly DependencyProperty SettingNameProperty = DependencyProperty.Register("SettingName", typeof(string), typeof(ColorSetting), new PropertyMetadata("Color"));


        public ICommand ResetCommand { get; }

        public ColorSetting()
        {
            ResetCommand = new RelayCommand(_ =>
            {
                SelectedColor = DefaultColor;
            });

            InitializeComponent();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            ResetButton.Visibility = Visibility.Visible;
        }
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ResetButton.Visibility = Visibility.Hidden;
        }
    }
}
