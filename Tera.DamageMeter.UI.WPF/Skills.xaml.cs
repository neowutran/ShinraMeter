using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private Dictionary<KeyValuePair<int, string>, SkillStats> _skills;
        private PlayerStats _parent;

        public Skills(Dictionary<KeyValuePair<int, string>, SkillStats> skills, PlayerStats parent )
        {
            InitializeComponent();
            _skills = skills;
            _parent = parent;
        }

        private void Repaint()
        {
            SkillsList.Items.Clear();
            var sortedDict = from entry in _skills orderby entry.Value.Damage descending select entry;
            SkillsList.Items.Add(new Skill(new KeyValuePair<int, string>(0, ""), null, true));
            foreach (var skill in sortedDict)
            {
                SkillsList.Items.Add(new Skill(skill.Key, skill.Value));
            }
        }


        public void Update(Dictionary<KeyValuePair<int, string>, SkillStats> skills)
        {
            _skills = skills;
            Repaint();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }

        private void Skills_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
                Console.WriteLine("Exception move");
            }
        }
    }
}