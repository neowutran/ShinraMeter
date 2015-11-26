using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private readonly PlayerStats _parent;
        private Label _currentSortedLabel;
        private IEnumerable<KeyValuePair<DamageMeter.Skill, SkillStats>> _skills;
        private SortBy _sortBy = SortBy.Damage;
        private SortOrder _sortOrder = SortOrder.Descending;


        public Skills(ConcurrentDictionary<DamageMeter.Skill, SkillStats> skills, PlayerStats parent)
        {
            InitializeComponent();
            var header = new Skill(new DamageMeter.Skill("", new List<int>()), null, true);

            header.LabelName.MouseRightButtonUp += LabelNameOnMouseRightButtonUp;
            header.LabelAverageCrit.MouseRightButtonUp += LabelAverageCritOnMouseRightButtonUp;
            header.LabelAverageHit.MouseRightButtonUp += LabelAverageHitOnMouseRightButtonUp;
            header.LabelBiggestCrit.MouseRightButtonUp += LabelBiggestCritOnMouseRightButtonUp;
            header.LabelCritRate.MouseRightButtonUp += LabelCritRateOnMouseRightButtonUp;
            header.LabelDamagePercentage.MouseRightButtonUp += LabelDamagePercentageOnMouseRightButtonUp;
            header.LabelLowestCrit.MouseRightButtonUp += LabelLowestCritOnMouseRightButtonUp;
            header.LabelNumberHit.MouseRightButtonUp += LabelNumberHitOnMouseRightButtonUp;
            header.LabelTotalDamage.MouseRightButtonUp += LabelTotalDamageOnMouseRightButtonUp;
            _currentSortedLabel = header.LabelTotalDamage;
            SkillsList.Items.Add(header);

            _skills = skills.ToDictionary(pair => pair.Key, pair => pair.Value);
            _parent = parent;
            Repaint();
        }

        private IEnumerable<KeyValuePair<DamageMeter.Skill, SkillStats>> Sort()
        {
            switch (_sortOrder)
            {
                case SortOrder.Descending:
                    switch (_sortBy)
                    {
                        case SortBy.Damage:
                            return from entry in _skills orderby entry.Value.Damage descending select entry;
                        case SortBy.AvgCrit:
                            return from entry in _skills orderby entry.Value.AverageCrit descending select entry;
                        case SortBy.AvgHit:
                            return from entry in _skills orderby entry.Value.AverageHit descending select entry;
                        case SortBy.BigCrit:
                            return from entry in _skills orderby entry.Value.BiggestCrit descending select entry;
                        case SortBy.DamagePercent:
                            return from entry in _skills orderby entry.Value.DamagePercentage descending select entry;
                        case SortBy.Name:
                            return from entry in _skills orderby entry.Key.SkillName descending select entry;
                        case SortBy.LowCrit:
                            return from entry in _skills orderby entry.Value.LowestCrit descending select entry;
                        case SortBy.NumberHits:
                            return from entry in _skills orderby entry.Value.Hits descending select entry;
                        case SortBy.CritRate:
                            return from entry in _skills orderby entry.Value.CritRate descending select entry;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case SortOrder.Ascending:
                    switch (_sortBy)
                    {
                        case SortBy.Damage:
                            return from entry in _skills orderby entry.Value.Damage ascending select entry;
                        case SortBy.AvgCrit:
                            return from entry in _skills orderby entry.Value.AverageCrit ascending select entry;
                        case SortBy.AvgHit:
                            return from entry in _skills orderby entry.Value.AverageHit ascending select entry;
                        case SortBy.BigCrit:
                            return from entry in _skills orderby entry.Value.BiggestCrit ascending select entry;
                        case SortBy.DamagePercent:
                            return from entry in _skills orderby entry.Value.DamagePercentage ascending select entry;
                        case SortBy.Name:
                            return from entry in _skills orderby entry.Key.SkillName ascending select entry;
                        case SortBy.LowCrit:
                            return from entry in _skills orderby entry.Value.LowestCrit ascending select entry;
                        case SortBy.NumberHits:
                            return from entry in _skills orderby entry.Value.Hits ascending select entry;
                        case SortBy.CritRate:
                            return from entry in _skills orderby entry.Value.CritRate ascending select entry;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeSort(SortBy sortby, Label sender, string labelstring)
        {
            if (_sortBy == sortby)
            {
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _currentSortedLabel.Content =
                    ((string) _currentSortedLabel.Content).Remove(((string) _currentSortedLabel.Content).Length - 1);
                _sortBy = sortby;
                _sortOrder = SortOrder.Descending;
                _currentSortedLabel = sender;
            }

            if (_sortOrder == SortOrder.Ascending)
            {
                sender.Content = labelstring + "↑";
            }
            else
            {
                sender.Content = labelstring + "↓";
            }
            Repaint();
        }

        private void LabelTotalDamageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Damage, (Label) sender, Skill.TotalDamage);
        }

        private void LabelNumberHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHits, (Label) sender, Skill.Hits);
        }

        private void LabelLowestCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.LowCrit, (Label) sender, Skill.LowestCrit);
        }

        private void LabelDamagePercentageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DamagePercent, (Label) sender, Skill.DamagePercentage);
        }

        private void LabelCritRateOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRate, (Label) sender, Skill.CritRate);
        }

        private void LabelBiggestCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.BigCrit, (Label) sender, Skill.BiggestCrit);
        }

        private void LabelAverageHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgHit, (Label) sender, Skill.AverageHit);
        }

        private void LabelAverageCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgCrit, (Label) sender, Skill.AverageCrit);
        }

        private void LabelNameOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Name, (Label) sender, Skill.SkillName);
        }

        private void Clear()
        {
            var header = (Skill) SkillsList.Items[0];
            SkillsList.Items.Clear();
            SkillsList.Items.Add(header);
        }

        public void Repaint()
        {
            Clear();
            var sortedDict = Sort();
            foreach (var skill in sortedDict)
            {
                SkillsList.Items.Add(new Skill(skill.Key, skill.Value));
            }
        }


        public void Update(ConcurrentDictionary<DamageMeter.Skill, SkillStats> skills)
        {
            _skills = skills.ToDictionary(pair => pair.Key, pair => pair.Value);
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
                Console.WriteLine(@"Exception move");
            }
        }

        private enum SortBy
        {
            Damage,
            Name,
            AvgCrit,
            BigCrit,
            LowCrit,
            DamagePercent,
            NumberHits,
            AvgHit,
            CritRate
        };

        private enum SortOrder
        {
            Ascending,
            Descending
        };
    }
}