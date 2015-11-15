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

        public Skills(Dictionary<KeyValuePair<int, string>, SkillStats> skills)
        {
            InitializeComponent();
            _skills = skills;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Repaint;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void Repaint(object sender, EventArgs e)
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
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
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