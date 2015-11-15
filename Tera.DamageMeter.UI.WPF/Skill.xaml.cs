using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class Skill
    {
        public Skill(KeyValuePair<int, string> skill, SkillStats stats, bool template = false)
        {
            InitializeComponent();

            if (template)
            {
                LabelName.Content = "Skill name";
                LabelId.Content = "Skill Id";
                LabelCritRate.Content = "CritRate";
                LabelTotalDamage.Content = "Damage";
                //  LabelNumberCrit.Content = "Crits";
                LabelNumberHit.Content = "Hits";
                LabelAverageCrit.Content = "Avg Crit";
                LabelBiggestCrit.Content = "Big Crit";
                LabelLowestCrit.Content = "Low Crit";
                LabelAverageHit.Content = "Avg Blank";
                //   LabelBiggestHit.Content = "Big NCrit";
                //   LabelLowestHit.Content = "Low NCrit";

                LabelName.Foreground = Brushes.Red;
                LabelId.Foreground = Brushes.Red;
                LabelCritRate.Foreground = Brushes.Red;
                LabelTotalDamage.Foreground = Brushes.Red;
                LabelNumberHit.Foreground = Brushes.Red;
                LabelAverageCrit.Foreground = Brushes.Red;
                LabelBiggestCrit.Foreground = Brushes.Red;
                LabelLowestCrit.Foreground = Brushes.Red;
                LabelAverageHit.Foreground = Brushes.Red;
            }
            else
            {
                LabelName.Content = skill.Value;
                LabelId.Content = skill.Key;
                LabelCritRate.Content = stats.CritRate + "%";
                LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(stats.Damage);
                LabelNumberHit.Content = stats.Hits;
                LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue(stats.AverageCrit);
                LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(stats.BiggestCrit);
                LabelLowestCrit.Content = FormatHelpers.Instance.FormatValue(stats.LowestCrit);
                LabelAverageHit.Content = FormatHelpers.Instance.FormatValue(stats.AverageHit);
            }
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine("Exception move");
            }
        }
    }
}