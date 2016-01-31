using System;
using System.Windows.Controls;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuffHeader : UserControl
    {
        public EnduranceDebuffHeader(EntityStatsMain  parent)
        {
            InitializeComponent();
            _parent = parent;

            LabelClass.Content = "Class";
            LabelAbnormalityDuration.Content = "Duration";
            LabelInterval.Content = "Fight duration";
            LabelAbnormalityDurationPercentage.Content = "% Fight";
            LabelName.Content = "Name";
            LabelId.Content = "Id";
        }

        private readonly EntityStatsMain _parent;

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _parent.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}