using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills : Window
    {
        public Skills(Dictionary<KeyValuePair<int, string>, SkillStats> skills)
        {
            InitializeComponent();
            var sortedDict = from entry in skills orderby entry.Value.Damage descending select entry;
            SkillsList.Items.Add(new Skill(new KeyValuePair<int, string>(0, ""), null, true));
            foreach (var skill in sortedDict)
            {
                SkillsList.Items.Add(new Skill(skill.Key, skill.Value));
            }
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}