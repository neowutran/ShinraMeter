using System;
using System.Windows.Controls;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuffHeader
    {
        public EnduranceDebuffHeader(EntityStatsMain  parent)
        {
            InitializeComponent();
            _parent = parent;

            LabelClass.Content = "Class";
            LabelAbnormalityDuration.Content = "DOT";
            LabelInterval.Content = "Fight";
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