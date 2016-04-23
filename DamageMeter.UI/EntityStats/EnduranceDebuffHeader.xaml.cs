using System;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuffHeader
    {
        public EnduranceDebuffHeader()
        {
            InitializeComponent();

            LabelClass.Content = "Class";
            LabelAbnormalityDuration.Content = "Eff Time";
            LabelInterval.Content = "Fight";
            LabelAbnormalityDurationPercentage.Content = "% Fight";
            LabelName.Content = "Name";
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var w = Window.GetWindow(this);
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}