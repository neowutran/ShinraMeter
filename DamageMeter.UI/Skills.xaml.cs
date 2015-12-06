using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DamageMeter.Skills.Skill;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private readonly PlayerStats _parent;
        private Label _currentSortedLabel;
        private IEnumerable<KeyValuePair<DamageMeter.Skills.Skill.Skill, SkillStats>> _skills;
        private SortBy _sortBy = SortBy.Damage;
        private SortOrder _sortOrder = SortOrder.Descending;


        public Skills(ConcurrentDictionary<DamageMeter.Skills.Skill.Skill, SkillStats> skills, PlayerStats parent)
        {
            InitializeComponent();
            var header = new SkillsHeader();

            header.LabelName.MouseRightButtonUp += LabelNameOnMouseRightButtonUp;
            header.LabelAverageCrit.MouseRightButtonUp += LabelAverageCritOnMouseRightButtonUp;
            header.LabelAverageHit.MouseRightButtonUp += LabelAverageHitOnMouseRightButtonUp;
            header.LabelBiggestCrit.MouseRightButtonUp += LabelBiggestCritOnMouseRightButtonUp;

            header.LabelCritRateDmg.MouseRightButtonUp += LabelCritRateDmgOnMouseRightButtonUp;
            header.LabelCritRateHeal.MouseRightButtonUp += LabelCritRateHealOnMouseRightButtonUp;


            header.LabelDamagePercentage.MouseRightButtonUp += LabelDamagePercentageOnMouseRightButtonUp;

            header.LabelNumberHitDmg.MouseRightButtonUp += LabelNumberHitDmgOnMouseRightButtonUp;
            header.LabelNumberHitHeal.MouseRightButtonUp += LabelNumberHitHealOnMouseRightButtonUp;
            header.LabelNumberHitMana.MouseRightButtonUp += LabelNumberHitManaOnMouseRightButtonUp;


            header.LabelTotalDamage.MouseRightButtonUp += LabelTotalDamageOnMouseRightButtonUp;

            header.LabelNumberCritDmg.MouseRightButtonUp += LabelNumberCritDmgOnMouseRightButtonUp;
            header.LabelNumberCritHeal.MouseRightButtonUp += LabelNumberCritHealOnMouseRightButtonUp;

            header.LabelTotalHeal.MouseRightButtonUp += LabelTotalHealOnMouseRightButtonUp;
            header.LabelTotalMana.MouseRightButtonUp += LabelTotalManaOnMouseRightButtonUp;
            _currentSortedLabel = header.LabelTotalDamage;
            SkillsList.Items.Add(header);

            _skills = skills.ToDictionary(pair => pair.Key, pair => pair.Value);
            _parent = parent;
            Repaint();
        }


        private void LabelTotalHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Heal, (Label) sender, SkillsHeader.Heal);
        }

        private void LabelNumberCritHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberCritsHeal, (Label) sender, SkillsHeader.CritsHeal);
        }

        private void LabelNumberCritDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberCritsDmg, (Label) sender, SkillsHeader.CritsDmg);
        }

        private void LabelNumberHitManaOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHitsMana, (Label) sender, SkillsHeader.HitsMana);
        }

        private void LabelNumberHitHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHitsHeal, (Label) sender, SkillsHeader.HitsHeal);
        }

        private void LabelNumberHitDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHitsDmg, (Label) sender, SkillsHeader.HitsDmg);
        }

        private void LabelCritRateHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRateHeal, (Label) sender, SkillsHeader.CritRateHeal);
        }

        private void LabelCritRateDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRateDmg, (Label) sender, SkillsHeader.CritRateDmg);
        }

        private void LabelTotalManaOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Mana, (Label) sender, SkillsHeader.Mana);
        }


        private IEnumerable<KeyValuePair<DamageMeter.Skills.Skill.Skill, SkillStats>> Sort()
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
                        case SortBy.Mana:
                            return from entry in _skills orderby entry.Value.Mana descending select entry;
                        case SortBy.Heal:
                            return from entry in _skills orderby entry.Value.Heal descending select entry;
                        case SortBy.NumberHitsDmg:
                            return from entry in _skills orderby entry.Value.HitsDmg descending select entry;
                        case SortBy.NumberHitsHeal:
                            return from entry in _skills orderby entry.Value.HitsHeal descending select entry;
                        case SortBy.NumberHitsMana:
                            return from entry in _skills orderby entry.Value.HitsMana descending select entry;
                        case SortBy.NumberCritsDmg:
                            return from entry in _skills orderby entry.Value.CritsDmg descending select entry;
                        case SortBy.NumberCritsHeal:
                            return from entry in _skills orderby entry.Value.CritsHeal descending select entry;
                        case SortBy.CritRateDmg:
                            return from entry in _skills orderby entry.Value.CritRateDmg descending select entry;
                        case SortBy.CritRateHeal:
                            return from entry in _skills orderby entry.Value.CritRateHeal descending select entry;
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
                        case SortBy.Mana:
                            return from entry in _skills orderby entry.Value.Mana ascending select entry;
                        case SortBy.Heal:
                            return from entry in _skills orderby entry.Value.Heal ascending select entry;
                        case SortBy.NumberHitsDmg:
                            return from entry in _skills orderby entry.Value.HitsDmg ascending select entry;
                        case SortBy.NumberHitsHeal:
                            return from entry in _skills orderby entry.Value.HitsHeal ascending select entry;
                        case SortBy.NumberHitsMana:
                            return from entry in _skills orderby entry.Value.HitsMana ascending select entry;
                        case SortBy.NumberCritsDmg:
                            return from entry in _skills orderby entry.Value.CritsDmg ascending select entry;
                        case SortBy.NumberCritsHeal:
                            return from entry in _skills orderby entry.Value.CritsHeal ascending select entry;
                        case SortBy.CritRateDmg:
                            return from entry in _skills orderby entry.Value.CritRateDmg ascending select entry;
                        case SortBy.CritRateHeal:
                            return from entry in _skills orderby entry.Value.CritRateHeal ascending select entry;
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
            ChangeSort(SortBy.Damage, (Label) sender, SkillsHeader.TotalDamage);
        }

        private void LabelDamagePercentageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DamagePercent, (Label) sender, SkillsHeader.DamagePercentage);
        }

        private void LabelBiggestCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.BigCrit, (Label) sender, SkillsHeader.BiggestCrit);
        }

        private void LabelAverageHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgHit, (Label) sender, SkillsHeader.AverageHit);
        }

        private void LabelAverageCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgCrit, (Label) sender, SkillsHeader.AverageCrit);
        }

        private void LabelNameOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Name, (Label) sender, SkillsHeader.SkillName);
        }

        private List<Skill> Clear()
        {
            var skills = new List<Skill>();
            var header = (SkillsHeader) SkillsList.Items[0];
            for (var i = 1; i < SkillsList.Items.Count; i++)
            {
                skills.Add((Skill) SkillsList.Items[i]);
            }
            SkillsList.Items.Clear();
            SkillsList.Items.Add(header);
            return skills;
        }

        public void Repaint()
        {
            var oldSkills = Clear();
            var sortedDict = Sort();
            foreach (var skill in sortedDict)
            {
                var updated = -1;
                for (var i = 0; i < oldSkills.Count; i++)
                {
                    if (!skill.Key.SkillName.Equals(oldSkills[i].SkillNameIdent)) continue;
                    oldSkills[i].Update(skill.Key, skill.Value);
                    SkillsList.Items.Add(oldSkills[i]);
                    updated = i;
                    break;
                }

                if (updated != -1)
                {
                    oldSkills.RemoveAt(updated);
                    continue;
                }

                SkillsList.Items.Add(new Skill(skill.Key, skill.Value));
            }
        }


        public void Update(ConcurrentDictionary<DamageMeter.Skills.Skill.Skill, SkillStats> skills)
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
            Heal,
            Mana,
            AvgCrit,
            BigCrit,
            DamagePercent,
            NumberHitsDmg,
            NumberHitsHeal,
            NumberHitsMana,
            NumberCritsDmg,
            NumberCritsHeal,
            AvgHit,
            CritRateDmg,
            CritRateHeal
        };

        private enum SortOrder
        {
            Ascending,
            Descending
        };
    }
}