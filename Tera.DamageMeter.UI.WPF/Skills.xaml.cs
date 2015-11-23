using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private readonly PlayerStats _parent;
        private ConcurrentDictionary<DamageMeter.Skill, SkillStats> _skills;

        public Skills(ConcurrentDictionary<DamageMeter.Skill, SkillStats> skills, PlayerStats parent)
        {
            InitializeComponent();
            _skills = skills;
            _parent = parent;
        }

        private void Repaint()
        {
            SkillsList.Items.Clear();

            SkillsList.Items.Add(new Skill(new DamageMeter.Skill("", new List<int>()), null, true));
            var sortedDict = from entry in _skills orderby entry.Value.Damage descending select entry;
            foreach (var skill in sortedDict)
            {
                SkillsList.Items.Add(new Skill(skill.Key, skill.Value));
            }
        }


        public void Update(ConcurrentDictionary<DamageMeter.Skill, SkillStats> skills)
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