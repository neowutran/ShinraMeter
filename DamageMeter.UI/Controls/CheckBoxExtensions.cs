using System.Windows;

namespace TCC.UI.Controls.Settings
{
    public class CheckBoxExtensions
    {

        public static readonly DependencyProperty SizeProperty = DependencyProperty.RegisterAttached("Size",
            typeof(double),
            typeof(CheckBoxExtensions),
            new PropertyMetadata(18D));
        public static double GetSize(DependencyObject obj) => (double)obj.GetValue(SizeProperty);
        public static void SetSize(DependencyObject obj, double value) => obj.SetValue(SizeProperty, value);


    }
}